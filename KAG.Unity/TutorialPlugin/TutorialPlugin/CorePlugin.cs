using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;

namespace TutorialPlugin
{
	public class CorePlugin : Plugin
	{
		public override Version Version => new Version(1, 0, 0);
		public override bool ThreadSafe => false;

		private Dictionary<IClient, Player> _connectedPlayers;

		public CorePlugin(PluginLoadData loadData) : base(loadData)
		{
			_connectedPlayers = new Dictionary<IClient, Player>();
			
			ClientManager.ClientConnected += OnClientConnected;
			ClientManager.ClientDisconnected += OnClientDisconnected;
		}
		
		private void OnClientConnected(object sender, ClientConnectedEventArgs args)
		{
			var player = new Player(args.Client.ID, "John Doe");
			_connectedPlayers.Add(args.Client, player);

			using (var writer = DarkRiftWriter.Create())
			{
				writer.Write(player);
				using (var message = Message.Create(Tags.PlayerConnection, writer))
				{
					foreach (var client in ClientManager.GetAllClients().Where(candidate => candidate != args.Client))
						client.SendMessage(message, SendMode.Reliable);
				}
			}

			foreach (var alreadyConnectedPlayer in _connectedPlayers.Values)
			{
				var message = Message.Create(Tags.PlayerConnection, alreadyConnectedPlayer);
				args.Client.SendMessage(message, SendMode.Reliable);
			}
		}
		
		private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
		{
			_connectedPlayers.Remove(args.Client);

			using (var writer = DarkRiftWriter.Create())
			{
				writer.Write(args.Client.ID);
				using (var message = Message.Create(Tags.PlayerDisconnection, writer))
				{
					foreach (var client in ClientManager.GetAllClients())
						client.SendMessage(message, SendMode.Reliable);
				}
			}
		}
	}
}