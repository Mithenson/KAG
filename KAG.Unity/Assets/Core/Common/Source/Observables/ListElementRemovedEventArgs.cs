using System;

namespace KAG.Unity.Common.Observables
{
	public sealed class ListElementRemovedEventArgs : EventArgs
	{
		public readonly object Item;
		public readonly int Index;
		
		public ListElementRemovedEventArgs(object item, int index)
		{
			Item = item;
			Index = index;
		}
	}
}