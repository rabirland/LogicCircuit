using System;
using System.Collections.Generic;
using System.Text;

namespace LogicCircuit
{
	public abstract class Node
	{
		public abstract int InputCount { get; }

		public string Name { get; set; }

		public abstract bool CalculateOutput(params bool[] inputs);
	}

	public abstract class InteractiveNode : Node
	{
		public abstract void Press();
		public abstract void Release();
	}

	public abstract class InputOnlyNode : Node
	{
		public override bool CalculateOutput(params bool[] inputs) => false;
	}
}
