using System;

namespace KAG.Unity.Common.Observables
{
	public readonly struct PropertyIdentifier : IEquatable<PropertyIdentifier>
	{
		public string PropertyName => _propertyName;
		
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

		public override string ToString() => 
			_propertyName;
	}
}