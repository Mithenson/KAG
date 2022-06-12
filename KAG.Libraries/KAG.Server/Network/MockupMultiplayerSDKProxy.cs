using System.Collections.Generic;

namespace KAG.Server.Network
{
	public class MockupMultiplayerSDKProxy : IMultiplayerSDKProxy
	{
		public void UpdateConnectedPlayers(IEnumerable<Player> connectedPlayers) { }
	}
}