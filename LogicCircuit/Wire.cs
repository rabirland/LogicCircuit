using System;
using System.Collections.Generic;
using System.Text;

namespace LogicCircuit
{
	public struct Wire
	{
		public static readonly Wire Empty = default; // null, null, 0

		/// <summary>
		/// The node that his wire connects to the output port of
		/// </summary>
		public Node OutputNode { get; }

		/// <summary>
		/// The node that this wire connects to the input port of
		/// </summary>
		public Node InputNode { get; }

		/// <summary>
		/// The number (1-based) of the input port
		/// </summary>
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

		public static bool operator ==(Wire a, Wire b) => a.OutputNode == b.OutputNode && a.InputNode == b.InputNode && a.InputNumber == b.InputNumber;
		public static bool operator !=(Wire a, Wire b) => a.OutputNode != b.OutputNode || a.InputNode != b.InputNode || a.InputNumber != b.InputNumber;
		public override bool Equals(object obj) => obj != null && obj is Wire w && w == this;
		public override int GetHashCode() => this.OutputNode.GetHashCode() ^ this.InputNode.GetHashCode() ^ this.InputNumber.GetHashCode();
	}
}
