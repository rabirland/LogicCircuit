using System;
using System.Collections.Generic;
using System.Text;

namespace LogicCircuit.Nodes
{
	public class AndNode : Node
	{
		public override int InputCount => 2;

		public override bool CalculateOutput(params bool[] inputs)
		{
			if (inputs.Length < this.InputCount)
			{
				throw new Exception("Invalid input count");
			}

			return inputs[0] && inputs[1];
		}
	}
}
