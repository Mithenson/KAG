using System;
using System.Collections.Generic;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Observables;

namespace KAG.Unity.Common
{
	public abstract class ViewModel : Observable, IDisposable
	{
		private List<DataBinding> _bindings;

		public ViewModel() => 
			_bindings = new List<DataBinding>();

		protected void AddBinding(DataBinding dataBinding) => 
			_bindings.Add(dataBinding);

		protected IDataBindingTarget CreatePropertyDataBindingTarget(string propertyName) => 
			propertyName.ToReflectedPropertyDataBindingTarget(this);
		protected IDataBindingTarget CreateMethodDataBindingTarget(string methodName) => 
			methodName.ToReflectedMethodDataBindingTarget(this);
		protected IDataBindingTarget CreateParameterlessMethodDataBindingTarget<T>(string methodName) => 
			methodName.ToReflectedParameterlessMethodDataBindingTarget(this);

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