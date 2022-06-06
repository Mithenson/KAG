using System.Collections.Generic;
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
		public Entity LocalPlayer => 
			_localPlayer;
		public IReadOnlyList<Entity> Players =>
			_players;
		
		private readonly UnityClient _client;
		private readonly World _world;
		private readonly ApplicationModel _applicationModel;
		private readonly PlayerModel _playerModel;

		private Entity _localPlayer;
		private List<Entity> _players;
		
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
			
			_players = new List<Entity>();
		}

		public void Start()
		{
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
			{
				var entity = _world.CreateEntity(reader);
				if (!entity.TryGetComponent(out PlayerComponent player))
					continue;

				_players.Add(entity);

				if (player.Id == _client.ID)
					_localPlayer = entity;
			}
		}

		private void OnPlayerArrival(DarkRiftReader reader)
		{
			var player = _world.CreateEntity(reader);
			_players.Add(player);
		}

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
	}
}