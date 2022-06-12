using System.Collections.Generic;

namespace KAG.Server.Network
{
	public interface IMultiplayerSDKProxy
	{
		void UpdateConnectedPlayers(IEnumerable<Player> connectedPlayers);
	}
}