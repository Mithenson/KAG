using System.Runtime.CompilerServices;

namespace KAG.Unity.Common.Observables
{
	public abstract class Observable : IObservable
	{
		public event PropertyChangedEventHandler OnPropertyChanged;

		protected void ChangeProperty<T>(ref T from, T to, [CallerMemberName] string propertyName = null)
		{
			if (ObservableUtilities.TryChangeProperty(ref from, to, propertyName, out var args))
				OnPropertyChanged?.Invoke(this, args);
		}
	}
}