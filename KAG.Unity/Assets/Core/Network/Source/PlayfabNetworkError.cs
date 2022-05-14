using KAG.Shared.Extensions;
using PlayFab;

namespace KAG.Unity.Network
{
	public sealed class PlayfabNetworkError : NetworkError
	{
		protected override string Name => _source.Error.ToString().FormatCamelCase();

		private PlayFabError _source;

		public PlayfabNetworkError(string step, PlayFabError source) : base(step) => 
			_source = source;
	}
}