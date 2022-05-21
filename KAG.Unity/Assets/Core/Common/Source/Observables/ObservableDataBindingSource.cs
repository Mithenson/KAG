using System;
using KAG.Unity.Common.DataBindings;

namespace KAG.Unity.Common.Observables
{
	public sealed class ObservableDataBindingSource : IDataBindingSource, IDisposable
	{
		public event Action<object> OnSourceChanged;

		private IObservable _observable;
		private PropertyIdentifier _propertyIdentifier;

		public ObservableDataBindingSource(IObservable observable, PropertyIdentifier propertyIdentifier)
		{
			_observable = observable;
			_propertyIdentifier = propertyIdentifier;

			_observable.OnPropertyChanged += OnObservablePropertyChanged;
		}

		private void OnObservablePropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyIdentifier == _propertyIdentifier)
				OnSourceChanged?.Invoke(args.To);
		}

		void IDisposable.Dispose() => 
			_observable.OnPropertyChanged -= OnObservablePropertyChanged;
	}
}