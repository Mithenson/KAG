using System.Reflection;

namespace KAG.Unity.Common.DataBindings
{
	public static class DataBindingConstants
	{
		public const BindingFlags PropertySearchFlags = BindingFlags.Public | BindingFlags.Instance;
		public const BindingFlags MethodSearchFlags = BindingFlags.Public | BindingFlags.Instance;
	}
}