namespace KAG.Unity.Network
{
	public abstract class NetworkError
	{
		public string Message => $"{Step}: {Name}";
		public string Step { get; private set; }
	
		protected abstract string Name { get; }

		public NetworkError(string step) => 
			Step = step;

		public override string ToString() => 
			Message;
	}
}