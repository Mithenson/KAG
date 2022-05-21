using System;
using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ReflectedPropertyDataBindingTarget<T> : IDataBindingTarget
	{
		private Action<T> _setter;

		public ReflectedPropertyDataBindingTarget(object owner, string propertyName) 
			: this (owner, propertyName.ToPropertyForDataBindingTarget(owner)) { }
		public ReflectedPropertyDataBindingTarget(object owner, PropertyInfo property) =>
			_setter = (Action<T>)property.SetMethod.CreateDelegate(typeof(Action<T>), owner);

		public void Set(object value) =>
			_setter((T)value);
	}
}