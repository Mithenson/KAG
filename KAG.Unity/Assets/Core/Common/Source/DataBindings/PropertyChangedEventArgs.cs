using System;

namespace KAG.Unity.Common.DataBindings
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

	public readonly struct PropertyIdentifier : IEquatable<PropertyIdentifier>
	{
		private readonly string _propertyName;
		
		public PropertyIdentifier(string propertyName) => 
			_propertyName = propertyName;

		public override bool Equals(object obj) => 
			obj is PropertyIdentifier other && Equals(other);
		public bool Equals(PropertyIdentifier other) => 
			_propertyName == other._propertyName;
		
		public static bool operator ==(PropertyIdentifier left, PropertyIdentifier right) => 
			left.Equals(right);
		public static bool operator !=(PropertyIdentifier left, PropertyIdentifier right) => 
			!left.Equals(right);
		
		public override int GetHashCode() => 
			_propertyName != null ? _propertyName.GetHashCode() : 0;
	}
}