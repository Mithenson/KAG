using System;
using System.Collections.Generic;

namespace KAG.Server
{
	public interface IMultiplayerSDKProxy
	{
		void UpdateConnectedPlayers(IEnumerable<Player> connectedPlayers);
	}
}