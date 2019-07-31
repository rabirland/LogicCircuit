using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using LogicCircuit.Nodes;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace LogicCircuit
{
	public class Circuit
	{
		private ConcurrentDictionary<Node, bool> _nodes = new ConcurrentDictionary<Node, bool>();
		private ConcurrentDictionary<Wire, byte> _wires = new ConcurrentDictionary<Wire, byte>();
		private Task _backgroundTask;

		public IEnumerable<Node> Nodes => this._nodes.Keys;
		public IEnumerable<Wire> Wires => this._wires.Keys;
		public event Action OnUpdate;

		public Circuit()
		{
			this._backgroundTask = Task.Factory.StartNew(this.TaskMethod, TaskCreationOptions.LongRunning);
		}

		public void UpdateFrom(Node node)
		{

		}

		public void AddNode(Node node)
		{
			while (!this._nodes.TryAdd(node, false)) { };
		}

		public void RemoveNode(Node node)
		{
			// Delete all wire associated with the node
			var wiresToDelete = this.Wires.Where(w => w.InputNode == node || w.OutputNode == node).ToArray();
			foreach (var wire in wiresToDelete)
			{
				while (!this._wires.TryRemove(wire, out byte value)) { }
			}

			// Remove the node
			while (!this._nodes.TryRemove(node, out bool value)) { }
		}

		public void Connect(Node output, Node input, int inputIndex)
		{
			var wire = new Wire(output, input, inputIndex);
			this.Connect(wire);
		}

		public void Connect(Wire wire)
		{
			// Check if not empty wire
			if (wire != Wire.Empty)
			{
				// Check if we already has such an entry
				if (!this.Wires.Contains(wire))
				{
					// Check if the targeted input port is free
					if (!this.Wires.Any(w => w.InputNode == wire.InputNode && w.InputNumber == wire.InputNumber))
					{
						// Check if it is a valid wire index for the input node
						if (wire.InputNumber <= wire.InputNode.InputCount && wire.InputNumber > 0)
						{
							while (!this._wires.TryAdd(wire, 0)) { }
						}
						else
						{
							throw new Exception("Invalid input number");
						}
					}
					else
					{
						throw new Exception("Input connection is not free");
					}
				}
				else
				{
					throw new Exception("There is already such a wire");
				}
			}
			else
			{
				throw new Exception("Can not add empty wire");
			}
		}

		public void Disconnect(Wire wire)
		{
			while (!this._wires.TryRemove(wire, out byte value)) { }
		}

		public bool GetNodeState(Node node) => this._nodes[node];

		public bool GetInputPortState(Node node, int portNumber)
		{
			var wire = this.Wires.FirstOrDefault(w => w.InputNode == node && w.InputNumber == portNumber);
			if (wire != default)
			{
				return this._nodes[wire.OutputNode];
			}
			else
			{
				return false;
			}
		}

		private void TaskMethod()
		{
			while (true)
			{
				var update = false;

				Parallel.ForEach(this._nodes, nodeEntry =>
				{
					bool oldValue = nodeEntry.Value;
					bool newValue = this.CalculateNodeOutput(nodeEntry.Key);
					update |= oldValue != newValue;
					this._nodes[nodeEntry.Key] = newValue;
				});

				if (update)
				{
					this.OnUpdate?.Invoke();
				}
			}
		}

		private bool[] GetNodeInputs(Node node)
		{
			var ret = new bool[node.InputCount];
			var connectedToInput = this.Wires.Where(w => w.InputNode == node);
			foreach (var wire in connectedToInput)
			{
				ret[wire.InputNumber - 1] = this._nodes[wire.OutputNode];
			}
			return ret;
		}

		private bool CalculateNodeOutput(Node node)
		{
			var inputs = this.GetNodeInputs(node);
			return node.CalculateOutput(inputs);
		}

		private struct TaskEntry
		{
			public Task Task { get; }
			public CancellationTokenSource TokenSource { get; }

			public TaskEntry(Task task, CancellationTokenSource tokenSource)
			{
				this.Task = task;
				this.TokenSource = tokenSource;
			}

			public static bool operator ==(TaskEntry a, TaskEntry b) => a.Task == b.Task && a.TokenSource == b.TokenSource;
			public static bool operator !=(TaskEntry a, TaskEntry b) => a.Task != b.Task || a.TokenSource != b.TokenSource;
			public override bool Equals(object obj) => obj is TaskEntry te && te == this;
			public override int GetHashCode() => this.Task.GetHashCode() ^ this.TokenSource.GetHashCode();
		}
	}
}
