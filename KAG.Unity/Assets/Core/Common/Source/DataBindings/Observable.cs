using System.Runtime.CompilerServices;

namespace KAG.Unity.Common.DataBindings
{
	public abstract class Observable : IObservable
	{
		public event PropertyChangedEventHandler OnPropertyChanged;

		protected void ChangeProperty<T>(ref T property, T value, [CallerMemberName] string name = null)
		{
			if (property.Equals(value))
				return;

			var args = new PropertyChangedEventArgs(new PropertyIdentifier(name), property, value);
			OnPropertyChanged?.Invoke(this, args);
		}
	}
}