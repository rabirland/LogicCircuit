using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using LogicCircuit.Nodes;

namespace LogicCircuit
{
	public class Circuit
	{
		private Dictionary<Node, bool> _nodes = new Dictionary<Node, bool>();
		private List<Wire> _wires = new List<Wire>();
		private Queue<Node> _nodesToRecalculate = new Queue<Node>();

		public IEnumerable<Node> Nodes => this._nodes.Keys;
		public IEnumerable<Wire> Wires => this._wires;

		public Circuit()
		{
			var firstNode = new AndNode();
			var secondNode1 = new OrNode();
			var secondNode2 = new OrNode();
			var finalNode = new AndNode();

			var firstToSecond1 = new Wire(firstNode, secondNode1, 1);
			var firstToSecond2 = new Wire(firstNode, secondNode2, 2);

			var second1ToFinal = new Wire(secondNode1, finalNode, 1);
			var second2ToFinal = new Wire(secondNode2, finalNode, 2);

			this.AddNode(firstNode);
			this.AddNode(secondNode1);
			this.AddNode(secondNode2);
			this.AddNode(finalNode);

			this.Connect(firstToSecond1);
			this.Connect(firstToSecond2);

			this.Connect(second1ToFinal);
			this.Connect(second2ToFinal);

			this._nodes[firstNode] = true;
			this.UpdateFrom(firstNode);
		}

		public void UpdateFrom(Node node)
		{
			this._nodesToRecalculate.Clear();
			this._nodesToRecalculate.Enqueue(node);

			while (this._nodesToRecalculate.Count > 0)
			{
				var currentNode = this._nodesToRecalculate.Dequeue();
				bool oldOutput = this._nodes[currentNode];
				bool newOutput;
				// Get all wire that connects to any of the current node's input ports
				var connectedToInput = this._wires.Where(w => w.InputNode == currentNode);
				if (connectedToInput.Count() > 0)
				{
					var inputs = new bool[currentNode.InputCount];
					foreach (var wire in connectedToInput)
					{
						inputs[wire.InputNumber - 1] = this._nodes[wire.OutputNode];
					}

					newOutput = currentNode.CalculateOutput(inputs);
				}
				else
				{
					newOutput = oldOutput;
				}


				// Only update the states and only continue to the next node if the output was actually changed OR it is the first node in the loop
				if (currentNode == node || oldOutput != newOutput)
				{
					this._nodes[currentNode] = newOutput;

					var connectedToOutput = this._wires.Where(w => w.OutputNode == currentNode);
					foreach (var nextNodeWire in connectedToOutput)
					{
						this._nodesToRecalculate.Enqueue(nextNodeWire.InputNode);
					}
				}
			}
		}

		public void AddNode(Node node)
		{
			this._nodes.Add(node, false);
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
				if (!this._wires.Contains(wire))
				{
					// Check if the targeted input port is free
					if (!this._wires.Any(w => w.InputNode == wire.InputNode && w.InputNumber == wire.InputNumber))
					{
						// Check if it is a valid wire index for the input node
						if (wire.InputNumber <= wire.InputNode.InputCount && wire.InputNumber > 0)
						{
							this._wires.Add(wire);
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

		public struct Wire
		{
			public static readonly Wire Empty = default(Wire); // null, null, 0

			public Node OutputNode { get; }

			public Node InputNode { get; }

			public int InputNumber { get; }

			public Wire(Node outputNode, Node inputNode, int inputNumber)
			{
				if (outputNode is null) throw new ArgumentNullException(nameof(outputNode));
				if (inputNode is null) throw new ArgumentNullException(nameof(inputNode));
				if (inputNumber <= 0) throw new ArgumentOutOfRangeException(nameof(inputNumber));

				this.OutputNode = outputNode;
				this.InputNode = inputNode;
				this.InputNumber = inputNumber;
			}

			public static bool operator ==(Wire a, Wire b) =>a.OutputNode == b.OutputNode && a.InputNumber == b.InputNumber && a.InputNumber == b.InputNumber;
			public static bool operator !=(Wire a, Wire b) => a.OutputNode != b.OutputNode || a.InputNumber != b.InputNumber || a.InputNumber != b.InputNumber;
			public override bool Equals(object obj) => obj != null && obj is Wire w && w == this;
			public override int GetHashCode() => this.OutputNode.GetHashCode() ^ this.InputNode.GetHashCode() ^ this.InputNumber.GetHashCode();
		}
	}
}
