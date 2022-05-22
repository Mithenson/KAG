using System.Reflection;
using Sirenix.OdinInspector;

namespace KAG.Unity.Common.DataBindings
{
	public static class DataBindingConstants
	{
		public const BindingFlags PropertySearchFlags = BindingFlags.Public | BindingFlags.Instance;
		public const BindingFlags MethodSearchFlags = BindingFlags.Public | BindingFlags.Instance;

		#if UNITY_EDITOR
		
		public readonly static ValueDropdownItem<DataBindingTargetBuilder> DefaultDataBindingTargetBuilderDropdownItem = new ValueDropdownItem<DataBindingTargetBuilder>("None", DataBindingTargetBuilder.Default);
		
		#endif
	}
}