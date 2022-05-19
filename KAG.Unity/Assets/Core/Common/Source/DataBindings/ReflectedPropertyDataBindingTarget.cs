using System;
using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ReflectedPropertyDataBindingTarget<T> : IDataBindingTarget
	{
		private Action<T> _setter;

		public ReflectedPropertyDataBindingTarget(object owner, string propertyName) 
			: this (owner, owner.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)) { }
		public ReflectedPropertyDataBindingTarget(object owner, PropertyInfo property)
		{
			if (!property.PropertyType.DeclaringType.IsInstanceOfType(owner))
				throw new InvalidOperationException($"The `{nameof(property)}=({property.PropertyType.Name}){property.Name}` does not belong to the provided `{nameof(owner)}={owner}`.");
			
			_setter = (Action<T>)property.SetMethod.CreateDelegate(typeof(Action<T>), owner);
		}

		public void Set(object value) => 
			_setter((T)value);
	}
}