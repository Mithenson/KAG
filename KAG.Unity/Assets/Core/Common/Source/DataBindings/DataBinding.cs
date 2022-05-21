using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class DataBinding : IDisposable
	{
		public bool IsActive;
		
		private readonly IDataBindingSource _source;
		private readonly IDataBindingConverter _converter;
		private readonly IDataBindingTarget _target;
		
		public DataBinding(IDataBindingSource source, IDataBindingTarget target) 
			: this (source, new MockDataBindingConverter(), target) { }
		public DataBinding(IDataBindingSource source, IDataBindingConverter converter, IDataBindingTarget target)
		{
			_source = source;
			_converter = converter;
			_target = target;

			IsActive = true;
		}

		private void OnSourceChanged(object value)
		{
			if (!IsActive)
				return;
			
			_target.Set(_converter.Convert(value));
		}

		public void Dispose()
		{
			_source.OnSourceChanged -= OnSourceChanged;
			
			if (_source is IDisposable disposableSource)
				disposableSource.Dispose();

			if (_target is IDisposable disposableTarget)
				disposableTarget.Dispose();
		}
	}
}