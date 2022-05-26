using System;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class ToStringDataBindingConverter : DataBindingConverter<object, string>
	{
		public override string ConvertExplicitly(object value) => 
			value.ToString();
	}
}