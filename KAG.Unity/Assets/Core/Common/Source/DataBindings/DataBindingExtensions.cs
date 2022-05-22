using System;
using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public static class DataBindingExtensions
	{
		public static ReflectedPropertyDataBindingTarget<T> ToReflectedPropertyDataBindingTarget<T>(this string propertyName, object owner) =>
			new ReflectedPropertyDataBindingTarget<T>(owner, propertyName);
		public static IValueDataBindingTarget ToReflectedPropertyDataBindingTarget(this string propertyName, object owner)
		{
			var property = propertyName.ToPropertyForDataBindingTarget(owner);

			var genericType = typeof(ReflectedPropertyDataBindingTarget<>).MakeGenericType(property.PropertyType);
			return (IValueDataBindingTarget)Activator.CreateInstance(genericType, owner, property);
		}
		
		public static PropertyInfo ToPropertyForDataBindingTarget(this string propertyName, object owner) =>
			propertyName.ToPropertyForDataBindingTarget(owner.GetType());
		public static PropertyInfo ToPropertyForDataBindingTarget(this string propertyName, Type declaringType) => 
			declaringType.GetProperty(propertyName, DataBindingConstants.PropertySearchFlags);
		
		public static ReflectedMethodDataBindingTarget<T> ToReflectedMethodDataBindingTarget<T>(this string methodName, object owner) =>
			new ReflectedMethodDataBindingTarget<T>(owner, methodName);
		public static IValueDataBindingTarget ToReflectedMethodDataBindingTarget(this string methodName, Type parameterType, object owner)
		{
			var method = methodName.ToMethodForDataBindingTarget(parameterType, owner);
			var firstParameterType = method.GetParameters()[0].ParameterType;

			var genericType = typeof(ReflectedMethodDataBindingTarget<>).MakeGenericType(firstParameterType);
			return (IValueDataBindingTarget)Activator.CreateInstance(genericType, owner, method);
		}
		
		public static MethodInfo ToMethodForDataBindingTarget(this string methodName, Type parameterType, object owner) => 
			methodName.ToMethodForDataBindingTarget(parameterType, owner.GetType());
		public static MethodInfo ToMethodForDataBindingTarget(this string methodName, Type parameterType, Type declaringType) => 
			declaringType.GetMethod(methodName, new Type[] { parameterType });

		public static ReflectedParameterlessMethodDataBindingTarget ToReflectedParameterlessMethodDataBindingTarget(this string methodName, object owner) =>
			new ReflectedParameterlessMethodDataBindingTarget(owner, methodName);

		public static MethodInfo ToParameterlessMethodForDataBindingTarget(this string methodName, object owner) => 
			methodName.ToParameterlessMethodForDataBindingTarget(owner.GetType());
		public static MethodInfo ToParameterlessMethodForDataBindingTarget(this string methodName, Type declaringType) => 
			declaringType.GetMethod(methodName, Type.EmptyTypes);
	}
}