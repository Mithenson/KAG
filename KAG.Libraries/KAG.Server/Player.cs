using DarkRift.Server;
using KAG.Shared;

namespace KAG.Server
{
	public sealed class Player
	{
		public readonly IClient Client;
		public readonly string Name;
		public readonly Entity Entity;
		
		public Player(IClient client, string name, Entity entity)
		{
			Client = client;
			Name = name;
			Entity = entity;
		}
	}
}