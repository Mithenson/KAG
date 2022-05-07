namespace KAG.Shared
{
	public struct PlayerState
	{
		public static readonly PlayerState Default = new PlayerState(false, 0.0f, 0.0f);
		
		public bool IsReady;
		public float X;
		public float Y;
		
		public PlayerState(bool isReady, float x, float y)
		{
			IsReady = isReady;
			X = x;
			Y = y;
		}
	}
}