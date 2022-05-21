namespace KAG.Unity.Common.Observables
{
	public interface IObservable
	{
		event PropertyChangedEventHandler OnPropertyChanged;
	}
}