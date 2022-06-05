using System;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class InvertBoolConverter : DataBindingConverter<bool, bool>
	{
		public override bool TryConvertExplicitly(bool input, out bool output)
		{
			output = !input;
			return true;
		}
	}
}