using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using KAG.Server.Network;
using KAG.Shared;

namespace KAG.Server.Gameplay
{
	public abstract class GameplayMessageHandler : IServerMessageHandler
	{
		public abstract ushort Tag { get; }

		protected IClientManager ClientManager =>
			_plugin.ClientManager;
		
		protected IReadOnlyDictionary<IClient, Player> ConnectedPlayers => 
			_plugin.ConnectedPlayers;
		
		private readonly CorePlugin _plugin;
		protected readonly World _world;
		
		protected GameplayMessageHandler(CorePlugin plugin, World world)
		{
			_plugin = plugin;
			_world = world;
		}
		
		public abstract void Handle(IClient client, Message message, DarkRiftReader reader);
	}
}