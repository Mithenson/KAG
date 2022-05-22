using System;

namespace KAG.Unity.Common.Observables
{
	public sealed class ListElementAddedEventArgs : EventArgs
	{
		public readonly object Item;
		
		public ListElementAddedEventArgs(object item) => 
			Item = item;
	}
}