namespace KAG.Unity.Common.DataBindings
{
	public static class DataBindingExtensions
	{
		public static ReflectedPropertyDataBindingTarget<T> ToReflectedPropertyDataBindingTarget<T>(this string propertyName, object owner) =>
			new ReflectedPropertyDataBindingTarget<T>(owner, propertyName);
		
		public static ReflectedMethodDataBindingTarget<T> ToReflectedMethodDataBindingTarget<T>(this string methodName, object owner) =>
			new ReflectedMethodDataBindingTarget<T>(owner, methodName);
	}
}