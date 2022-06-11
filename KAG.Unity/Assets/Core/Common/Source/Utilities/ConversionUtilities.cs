using UnityEngine;

namespace KAG.Unity.Common.Utilities
{
	public static class ConversionUtilities
	{
		public static Vector2 ToUnity(this KAG.Shared.Transform.Vector2 value) => 
			new Vector2(value.X, value.Y);

		public static KAG.Shared.Transform.Vector2 ToShared(this Vector2 value) => 
			new KAG.Shared.Transform.Vector2(value.x, value.y);
	}
}