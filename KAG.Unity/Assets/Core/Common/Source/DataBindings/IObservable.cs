namespace KAG.Unity.Common.DataBindings
{
	public interface IObservable
	{
		event PropertyChangedEventHandler OnPropertyChanged;
	}
}