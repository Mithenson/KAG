using KAG.Shared.Extensions;
using PlayFab;

namespace KAG.Unity.Network
{
	public sealed class PlayfabNetworkError : NetworkError
	{
		public override string Message => _source.Error.ToString().FormatCamelCase();

		private PlayFabError _source;

		public PlayfabNetworkError(PlayFabError source) => 
			_source = source;
	}
}