using System;

namespace KAG.Unity.Network
{
	public sealed class PlayerArrivalEventArgs : EventArgs
	{
		public readonly Player Player;
		
		public PlayerArrivalEventArgs(Player player) => 
			Player = player;
	}
}