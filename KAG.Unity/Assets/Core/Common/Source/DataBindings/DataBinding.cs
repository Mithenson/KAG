using System;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class DataBinding : IDisposable
	{
		public bool IsActive { get; private set; }
		
		private readonly IDataBindingSource _source;
		private readonly IDataBindingConverter _converter;
		private readonly IDataBindingTarget _target;
		
		public DataBinding(IDataBindingSource source, IDataBindingTarget target, bool isActive = true) 
			: this (source, new MockDataBindingConverter(), target, isActive) { }
		public DataBinding(IDataBindingSource source, IDataBindingConverter converter, IDataBindingTarget target, bool isActive = true)
		{
			_source = source;
			_converter = converter;
			_target = target;

			_source.OnSourceChanged += OnSourceChanged;
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
				IsActive = false;
			else
			{
				IsActive = true;
				IMP_OnSourceChanged(_source.Value);
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
			_source.OnSourceChanged -= OnSourceChanged;
			
			if (_source is IDisposable disposableSource)
				disposableSource.Dispose();

			if (_target is IDisposable disposableTarget)
				disposableTarget.Dispose();
		}
	}
}