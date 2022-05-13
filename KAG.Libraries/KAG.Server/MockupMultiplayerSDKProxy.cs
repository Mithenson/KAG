using System;
using System.Collections.Generic;

namespace KAG.Server
{
	public class MockupMultiplayerSDKProxy : IMultiplayerSDKProxy
	{
		public void UpdateConnectedPlayers(IEnumerable<Player> connectedPlayers) { }
	}
}