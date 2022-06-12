using DarkRift;
using DarkRift.Client.Unity;
using KAG.Unity.Network;
using KAG.Unity.Simulation;

namespace KAG.Unity.Gameplay
{
	public abstract class GameplayMessageHandler : IUnityMessageHandler
	{
		public abstract ushort Tag { get; }

		protected readonly UnityClient _client;
		protected readonly UnityWorld _world;
		
		protected GameplayMessageHandler(UnityClient client, UnityWorld world)
		{
			_client = client;
			_world = world;
		}

		public abstract void Handle(NetworkManager networkManager, Message message, DarkRiftReader reader);
	}
}