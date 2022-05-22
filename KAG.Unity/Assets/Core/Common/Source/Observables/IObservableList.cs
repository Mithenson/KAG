using System.Collections;

namespace KAG.Unity.Common.Observables
{
	public interface IObservableList : IList
	{
		event ListElementAddedEventHandler OnElementAdded;
		event ListElementInsertedEventHandler OnElementInserted;
		event ListElementRemovedEventHandler OnElementRemoved;
		event ListClearedEventHandler OnCleared;
	}
}