using System;
using System.Collections.Generic;
using Microsoft.Playfab.Gaming.GSDK.CSharp;

namespace KAG.Server.Network
{
	public class PlayfabMultiplayerSDKProxy : IMultiplayerSDKProxy
	{
		private const float Timeout = 600.0f;
		
		private DateTime _sessionIdAssignmentTimestamp;
		private bool _isSessionIdAssigned;
		private int _connectedPlayersCount;
		
		public PlayfabMultiplayerSDKProxy()
		{
			_isSessionIdAssigned = false;
			_connectedPlayersCount = 0;
			
			GameserverSDK.RegisterHealthCallback(OnHealthCheck);
			GameserverSDK.RegisterShutdownCallback(Shutdown);

			GameserverSDK.Start();
			GameserverSDK.ReadyForPlayers();
		}
		
		private bool OnHealthCheck()
		{
			if (!_isSessionIdAssigned)
			{
				var config = GameserverSDK.getConfigSettings();
				if (config.TryGetValue(GameserverSDK.ServerIdKey, out _))
				{
					_sessionIdAssignmentTimestamp = DateTime.Now;
					_isSessionIdAssigned = true;
				}

				return true;
			}
			
			var runTime = (float)(DateTime.Now - _sessionIdAssignmentTimestamp).TotalSeconds;
			if (runTime > Timeout && _connectedPlayersCount <= 0)
			{
				Shutdown();
				return false;
			}

			return true;
		}

		public void UpdateConnectedPlayers(IEnumerable<Player> connectedPlayers)
		{
			var players = new List<ConnectedPlayer>();
			foreach (var connectedPlayer in connectedPlayers)
				players.Add(new ConnectedPlayer(connectedPlayer.Name));

			_connectedPlayersCount = players.Count;
			GameserverSDK.UpdateConnectedPlayers(players);
		}

		private void Shutdown() =>
			Environment.Exit(1);
	}
}