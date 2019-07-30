using System;
using System.Collections.Generic;
using System.Text;

namespace LogicCircuit.Nodes
{
	public class SwitchNode : InteractiveNode
	{
		private bool _enabled = false;
		public override int InputCount => 0;

		public override bool CalculateOutput(params bool[] inputs)
		{
			if (inputs.Length < this.InputCount)
			{
				throw new Exception("Invalid input count");
			}

			return this._enabled;
		}

		public override void Press() => this._enabled = !this._enabled;
		public override void Release() { }
	}
}
