namespace KAG.Unity.Network
{
	public sealed class CustomNetworkError : NetworkError
	{
		public override string Message => _message;
	
		private string _message;
	
		public CustomNetworkError(string message) => 
			_message = message;
	}
}