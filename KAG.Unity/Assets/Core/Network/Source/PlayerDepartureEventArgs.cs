using System;

namespace KAG.Unity.Network
{
	public sealed class PlayerDepartureEventArgs : EventArgs
	{
		public readonly Player Player;
		
		public PlayerDepartureEventArgs(Player player) => 
			Player = player;
	}
}