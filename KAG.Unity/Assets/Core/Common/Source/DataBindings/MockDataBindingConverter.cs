namespace KAG.Unity.Common.DataBindings
{
	public sealed class MockDataBindingConverter : IDataBindingConverter
	{
		public object Convert(object value) => 
			value;
	}
}