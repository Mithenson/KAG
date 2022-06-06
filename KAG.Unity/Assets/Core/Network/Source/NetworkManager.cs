using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Unity.Common.Models;

namespace KAG.Unity.Network
{
	public sealed class NetworkManager
	{
		private readonly UnityClient _client;
		private readonly World _world;
		private readonly ApplicationModel _applicationModel;
		private readonly PlayerModel _playerModel;
		
		public NetworkManager( 
			UnityClient client, 
			World world,
			ApplicationModel applicationModel, 
			PlayerModel playerModel)
		{
			_client = client;
			_world = world;
			_applicationModel = applicationModel;
			_playerModel = playerModel;
			
			_client.MessageReceived += OnClientMessageReceived;
			SendPlayerIdentificationMessage(_playerModel.Name);
		}
		
		private void SendPlayerIdentificationMessage(string playerName)
		{
			using var writer = DarkRiftWriter.Create();
			writer.Write(new PlayerIdentificationMessage()
			{
				Name = playerName
			});

			using var message = Message.Create(NetworkTags.PlayerIdentification, writer);
			_client.SendMessage(message, SendMode.Reliable);
		}

		public void LeaveMatch()
		{
			_client.MessageReceived -= OnClientMessageReceived;
			_client.Disconnect();

			_applicationModel.GoBackToLobby();
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
				
				case NetworkTags.PlayerDeparture:
					OnPlayerDeparture(reader);
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
		
		private void OnPlayerDeparture(DarkRiftReader reader)
		{
			var clientId = reader.ReadUInt16();
			var associatedEntity = default(Entity);
			
			foreach (var entity in _world.Entities)
			{
				if (!entity.TryGetComponent(out PlayerComponent player)
					|| player.Id != clientId)
					continue;

				associatedEntity = entity;
				break;
			}
			
			if (associatedEntity != null)
				_world.Destroy(associatedEntity);
		}
		
		private void CreateEntityFromReader(DarkRiftReader reader)
		{
			var entityId = reader.ReadUInt16();
			var entity = _world.CreateEntity(entityId);
			
			reader.ReadSerializableInto(ref entity);
		}
	}
}