using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class MockDataBindingConverter : IDataBindingConverter
	{
		public Type OutputType => null;
		
		public object Convert(object value) => 
			value;
	}
}