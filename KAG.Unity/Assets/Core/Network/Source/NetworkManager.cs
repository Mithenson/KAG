using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using Zenject;

namespace KAG.Unity.Network
{
	public sealed class NetworkManager
	{
		private readonly DiContainer _container;
		private readonly UnityClient _client;
		private readonly World _world;
		
		public NetworkManager(DiContainer container, UnityClient client, World world)
		{
			_container = container;
			_client = client;
			_world = world;
		}

		public async Task JoinMatch(string clientName, CancellationToken cancellationToken)
		{
			var connectionHandler = _container.Resolve<NetworkConnectionHandler>();
			var connectionTask = connectionHandler.Connect(clientName, cancellationToken);

			await connectionTask;

			if (!connectionTask.IsCompleted)
				return;
			
			SendPlayerIdentificationMessage(clientName);
			_client.MessageReceived += OnClientMessageReceived;
		}
		private void SendPlayerIdentificationMessage(string clientName)
		{
			using var writer = DarkRiftWriter.Create();
			writer.Write(new PlayerIdentificationMessage()
			{
				Name = clientName
			});

			using var message = Message.Create(NetworkTags.PlayerIdentification, writer);
			_client.SendMessage(message, SendMode.Reliable);
		}

		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			switch (args.Tag)
			{
				case NetworkTags.PlayerCatchup:
					OnPlayerCatchup(reader);
					break;
				
				case NetworkTags.PlayerArrival:
					OnPlayerArrival(reader);
					break;
			}
		}

		private void OnPlayerCatchup(DarkRiftReader reader)
		{
			while (reader.Position < reader.Length)
				CreateEntityFromReader(reader);
		}

		private void OnPlayerArrival(DarkRiftReader reader) => 
			CreateEntityFromReader(reader);

		private void CreateEntityFromReader(DarkRiftReader reader)
		{
			var entityId = reader.ReadUInt16();
			var entity = _world.CreateEntity(entityId);
			
			reader.ReadSerializableInto(ref entity);
		}
	}
}