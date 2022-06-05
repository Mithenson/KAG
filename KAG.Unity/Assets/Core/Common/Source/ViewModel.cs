using System;
using System.Collections.Generic;
using System.Diagnostics;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Observables;

namespace KAG.Unity.Common
{
	public abstract class ViewModel : Observable, IDisposable
	{
		private List<IDataBinding> _bindings;

		public ViewModel() => 
			_bindings = new List<IDataBinding>();

		protected void AddBinding(IDataBinding dataBinding) => 
			_bindings.Add(dataBinding);
		
		protected void AddBinding(IObservable observable, string propertyName, IValueDataBindingTarget dataBindingTarget) =>
			AddBinding(new ValueDataBinding(new ObservableDataBindingSource(observable, new PropertyIdentifier(propertyName)), dataBindingTarget));
		
		protected void AddPropertyBinding(IObservable observable, string propertyName, string targetPropertyName) => 
			AddBinding(observable, propertyName, targetPropertyName.ToReflectedPropertyDataBindingTarget(this));
		
		protected void AddMethodBinding(IObservable observable, string propertyName, string methodName)
		{
			var property = propertyName.ToPropertyForDataBindingTarget(observable);
			AddBinding(observable, propertyName, methodName.ToReflectedMethodDataBindingTarget(property.PropertyType, this));
		}
		
		protected void AddParameterlessMethodBinding(IObservable observable, string propertyName, string methodName) =>
			AddBinding(observable, propertyName, methodName.ToReflectedParameterlessMethodDataBindingTarget(this));
		
		protected void AddListBinding<TSource, TTarget>(ObservableList<TSource> source, Func<TSource, TTarget> conversion, IList<TTarget> target) =>
			AddBinding(new ListDataBinding<TSource, TTarget>(source, new LambdaDataBindingConverter<TSource, TTarget>(conversion), target));

		void IDisposable.Dispose()
		{
			foreach (var binding in _bindings)
				binding.Dispose();
			
			_bindings.Clear();
		}
	}

	public abstract class ViewModel<TModel> : ViewModel where TModel : Observable
	{
		protected readonly TModel _model;

		public ViewModel(TModel model) => 
			_model = model;

		protected void AddBinding(string propertyName, IValueDataBindingTarget dataBindingTarget) =>
			AddBinding(_model, propertyName, dataBindingTarget);

		protected void AddPropertyBinding(string sourcePropertyName, string targetPropertyName) => 
			AddPropertyBinding(_model, sourcePropertyName, targetPropertyName);
		
		protected void AddMethodBinding(string propertyName, string methodName) => 
			AddMethodBinding(_model, propertyName, methodName);

		protected void AddParameterlessMethodBinding(string propertyName, string methodName) =>
			AddParameterlessMethodBinding(_model, propertyName, methodName);
	}
}