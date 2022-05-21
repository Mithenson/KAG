using System;

namespace KAG.Unity.Common.Observables
{
	public sealed class PropertyChangedEventArgs : EventArgs
	{
		public readonly PropertyIdentifier PropertyIdentifier;
		public readonly object From;
		public readonly object To;
		
		public PropertyChangedEventArgs(PropertyIdentifier propertyIdentifier, object from, object to)
		{
			PropertyIdentifier = propertyIdentifier;
			From = from;
			To = to;
		}
	}
}