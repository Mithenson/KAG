using System;

namespace KAG.Unity.Network
{
	public sealed class NetworkException : Exception
	{
		public override string Message => $"{base.Message}\nError={_error.Message}";

		private NetworkError _error;
	
		public NetworkException(NetworkError error) => 
			_error = error;
		public NetworkException(NetworkError error, string message) : base(message) => 
			_error = error;
		public NetworkException(NetworkError error, string message, Exception innerException) : base(message, innerException) => 
			_error = error;
	}
}