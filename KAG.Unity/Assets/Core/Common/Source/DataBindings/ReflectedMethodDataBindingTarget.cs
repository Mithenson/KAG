﻿using System;
using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ReflectedMethodDataBindingTarget<T> : IDataBindingTarget
	{
		private Action<T> _method;

		public ReflectedMethodDataBindingTarget(object owner, string methodName) 
			: this (owner, owner.GetType().GetMethod(methodName, BindingFlags.Public)) { }
		public ReflectedMethodDataBindingTarget(object owner, MethodInfo method) =>
			_method = (Action<T>)method.CreateDelegate(typeof(Action<T>), owner);

		public void Set(object value) => 
			_method((T)value);
	}
}