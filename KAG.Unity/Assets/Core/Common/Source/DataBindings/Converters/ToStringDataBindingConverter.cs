using System;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class ToStringDataBindingConverter : DataBindingConverter<object, string>
	{
		public override bool TryConvertExplicitly(object input, out string output)
		{
			output = input.ToString();
			return true;
		}
		
	}
}