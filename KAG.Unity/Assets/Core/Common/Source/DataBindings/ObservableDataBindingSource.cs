using System;
using System.Reflection;
using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ObservableDataBindingSource : IValueDataBindingSource, IDisposable
	{
		#region Nested types

		private abstract class Getter
		{
			public abstract object GetValue();
		}

		private sealed class Getter<T> : Getter
		{
			private Func<T> _implementation;

			public Getter(IObservable observable, PropertyInfo property) =>
				_implementation = (Func<T>)property.GetMethod.CreateDelegate(typeof(Func<T>), observable);

			public override object GetValue() => 
				_implementation();
		}

		#endregion
		
		public event Action<object> OnSourceChanged;

		public object Value => _getter.GetValue();

		private IObservable _observable;
		private PropertyIdentifier _propertyIdentifier;
		private Getter _getter;

		public ObservableDataBindingSource(IObservable observable, PropertyIdentifier propertyIdentifier)
		{
			_observable = observable;
			_propertyIdentifier = propertyIdentifier;

			var property = GetProperty();
			_getter = (Getter)Activator.CreateInstance(typeof(Getter<>).MakeGenericType(property.PropertyType), observable, property);

			_observable.OnPropertyChanged += OnObservablePropertyChanged;
		}

		public PropertyInfo GetProperty() =>
			_propertyIdentifier.PropertyName.ToPropertyForDataBindingTarget(_observable);
		
		private void OnObservablePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyIdentifier != _propertyIdentifier)
				return;

			OnSourceChanged?.Invoke(args.To);
		}

		void IDisposable.Dispose() => 
			_observable.OnPropertyChanged -= OnObservablePropertyChanged;
	}
}