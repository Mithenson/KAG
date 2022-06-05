using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class MockDataBindingConverter : IDataBindingConverter
	{
		public Type OutputType => null;

		public bool TryConvert(object input, out object output)
		{
			output = input;
			return true;
		}
	}
}