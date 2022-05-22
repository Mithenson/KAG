using System;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ValueDataBinding : IDataBinding
	{
		public bool IsActive { get; private set; }
		
		private readonly IValueDataBindingSource _source;
		private readonly IDataBindingConverter _converter;
		private readonly IValueDataBindingTarget _target;
		
		public ValueDataBinding(IValueDataBindingSource source, IValueDataBindingTarget target, bool isActive = true) 
			: this (source, new MockDataBindingConverter(), target, isActive) { }
		public ValueDataBinding(IValueDataBindingSource source, IDataBindingConverter converter, IValueDataBindingTarget target, bool isActive = true)
		{
			_source = source;
			_converter = converter;
			_target = target;
			
			IMP_SetActive(isActive);
		}

		public void SetActive(bool value)
		{
			if (IsActive == value)
				return;

			IMP_SetActive(value);
		}
		private void IMP_SetActive(bool value)
		{
			if (!value)
			{
				IsActive = false;
				_source.OnSourceChanged -= OnSourceChanged;
			}
			else
			{
				IsActive = true;
				IMP_OnSourceChanged(_source.Value);
				
				_source.OnSourceChanged += OnSourceChanged;
			}
		}

		private void OnSourceChanged(object value)
		{
			if (!IsActive)
				return;
			
			IMP_OnSourceChanged(value);
		}
		private void IMP_OnSourceChanged(object value) =>
			_target.Set(_converter.Convert(value));
		
		public void Dispose()
		{
			IMP_SetActive(false);
			
			if (_source is IDisposable disposableSource)
				disposableSource.Dispose();
			
			if (_converter is IDisposable disposableConverter)
				disposableConverter.Dispose();

			if (_target is IDisposable disposableTarget)
				disposableTarget.Dispose();
		}
	}
}