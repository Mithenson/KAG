using UnityEngine;

namespace KAG.Unity.Common.Utilities
{
	public static class ConversionUtilities
	{
		public static Vector2 ToUnity(this KAG.Shared.Transform.Vector2 value) => 
			new Vector2(value.X, value.Y);
	}
}