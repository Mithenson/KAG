using System.Runtime.CompilerServices;

namespace KAG.Unity.Common.Observables
{
	public abstract class Observable : IObservable
	{
		public event PropertyChangedEventHandler OnPropertyChanged;

		protected void ChangeProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
		{
			if (ObservableUtilities.TryChangeProperty(ref property, ref value, propertyName, out var args))
				OnPropertyChanged?.Invoke(this, args);
		}
	}
}