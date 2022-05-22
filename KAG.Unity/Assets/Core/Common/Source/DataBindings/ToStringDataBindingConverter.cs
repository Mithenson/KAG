using System;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class ToStringDataBindingConverter : IDataBindingConverter
	{
		public Type OutputType => typeof(string);
		
		public object Convert(object value) => 
			value.ToString();
	}
}