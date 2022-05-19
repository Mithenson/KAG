using System;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ObservableDataBindingSource : IDataBindingSource, IDisposable
	{
		public event Action<object> OnSourceChanged;

		private Observable _observable;
		private PropertyIdentifier _propertyIdentifier;

		public ObservableDataBindingSource(Observable observable, PropertyIdentifier propertyIdentifier)
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