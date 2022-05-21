using System;
using System.Reflection;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ReflectedParameterlessMethodDataBindingTarget : IDataBindingTarget
	{
		private Action _method;
		
		public ReflectedParameterlessMethodDataBindingTarget(object owner, string methodName) 
			: this (owner, methodName.ToParameterlessMethodForDataBindingTarget(owner)) { }
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