namespace KAG.Unity.Common.DataBindings
{
	public sealed class DataBindingDecoratedConverter : IDataBindingConverter
	{
		private IDataBindingConverter _self;
		private IDataBindingConverter _child;
		
		public DataBindingDecoratedConverter(IDataBindingConverter self, IDataBindingConverter child)
		{
			_self = self;
			_child = child;
		}

		public object Convert(object value) => 
			_child.Convert(_self.Convert(value));
	}
}