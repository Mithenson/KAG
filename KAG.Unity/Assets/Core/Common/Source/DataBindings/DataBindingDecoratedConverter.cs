using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class DataBindingDecoratedConverter : IDataBindingConverter
	{
		public Type OutputType => _child.OutputType;

		private IDataBindingConverter _self;
		private IDataBindingConverter _child;
		
		public DataBindingDecoratedConverter(IDataBindingConverter self, IDataBindingConverter child)
		{
			_self = self;
			_child = child;
		}

		public bool TryConvert(object input, out object output)
		{
			if (!_self.TryConvert(input, out var selfOutput))
			{
				output = default;
				return false;
			}
			
			return _child.TryConvert(selfOutput, out output);
		}
	}
}