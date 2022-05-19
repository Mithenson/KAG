namespace KAG.Unity.Network
{
	public abstract class NetworkError
	{
		public abstract string Message { get; }

		public override string ToString() => 
			Message;
	}
}