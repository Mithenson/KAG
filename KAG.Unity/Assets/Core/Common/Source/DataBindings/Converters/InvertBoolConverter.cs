using System;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class InvertBoolConverter : DataBindingConverter<bool, bool>
	{
		public override bool ConvertExplicitly(bool input) => 
			!input;
	}
}