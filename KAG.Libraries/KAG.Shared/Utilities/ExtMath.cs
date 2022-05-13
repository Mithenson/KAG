namespace KAG.Shared.Utilities
{
	public static class ExtMath
	{
		public static float Clamp01(float value) => 
			Clamp(value, 0.0f, 1.0f);
		public static float Clamp(float value, float min, float max)
		{
			if (value < min)
				return min;
			
			return value > max ? max : value;
		}

		public static float Lerp(float lhs, float rhs, float ratio)
		{
			ratio = Clamp01(ratio);
			return LerpUnclamped(lhs, rhs, ratio);
		}
		public static float LerpUnclamped(float lhs, float rhs, float ratio) => 
			lhs + (rhs - lhs) * ratio;
	}
}