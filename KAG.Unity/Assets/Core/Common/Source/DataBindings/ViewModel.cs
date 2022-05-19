using System;
using System.Collections.Generic;

namespace KAG.Unity.Common.DataBindings
{
	public abstract class ViewModel : Observable, IDisposable
	{
		private List<DataBinding> _bindings;

		public ViewModel() => 
			_bindings = new List<DataBinding>();

		protected void AddBinding(DataBinding dataBinding) => 
			_bindings.Add(dataBinding);

		protected ReflectedPropertyDataBindingTarget<T> CreatePropertyDataBindingTarget<T>(string propertyName) => 
			propertyName.ToReflectedPropertyDataBindingTarget<T>(this);
		protected ReflectedMethodDataBindingTarget<T> CreateMethodDataBindingTarget<T>(string methodName) => 
			methodName.ToReflectedMethodDataBindingTarget<T>(this);

		void IDisposable.Dispose()
		{
			foreach (var binding in _bindings)
				binding.Dispose();
			
			_bindings.Clear();
		}
	}

	public abstract class ViewModel<TModel> : ViewModel where TModel : Observable
	{
		protected TModel _model;

		public ViewModel(TModel model) => 
			_model = model;

		protected void AddBinding(string propertyName, IDataBindingTarget dataBindingTarget) =>
			AddBinding(new DataBinding(new ObservableDataBindingSource(_model, new PropertyIdentifier(propertyName)), dataBindingTarget));
	}
}