using System;
using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ReflectedParameterlessMethodDataBindingTarget : IDataBindingTarget
	{
		private Action _method;
		
		public ReflectedParameterlessMethodDataBindingTarget(object owner, string methodName) 
			: this (owner, methodName.ToMethodForDataBindingTarget(owner)) { }
		public ReflectedParameterlessMethodDataBindingTarget(object owner, MethodInfo method) =>
			_method = (Action)method.CreateDelegate(typeof(Action), owner);

		public void Set(object value)
		{
			var toggle = (bool)value;
			
			if (toggle)
				_method();
		}
	}
}