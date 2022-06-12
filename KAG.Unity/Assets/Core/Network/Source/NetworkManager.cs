using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Events;
using KAG.Shared.Network;
using KAG.Unity.Common.Models;
using KAG.Unity.Simulation;
using Zenject;

namespace KAG.Unity.Network
{
	public sealed class NetworkManager : ILateDisposable
	{
		public Player LocalPlayer => 
			_localPlayer;
		public IReadOnlyList<Player> Players =>
			_players;
		
		private readonly UnityClient _client;
		private readonly UnityMessageDispatcher _messageDispatcher;
		private readonly UnityWorld _world;
		private readonly EventHub _eventHub;
		private readonly ApplicationModel _applicationModel;
		private readonly PlayerModel _playerModel;

		private Player _localPlayer;
		private List<Player> _players;
		
		public NetworkManager( 
			UnityClient client, 
			UnityMessageDispatcher messageDispatcher,
			UnityWorld world,
			EventHub eventHub,
			ApplicationModel applicationModel, 
			PlayerModel playerModel)
		{
			_client = client;
			_messageDispatcher = messageDispatcher;
			_world = world;
			_eventHub = eventHub;
			_applicationModel = applicationModel;
			_playerModel = playerModel;
			
			_players = new List<Player>();
			
			_eventHub.Define<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival);
			_eventHub.Define<PlayerDepartureEventArgs>(SharedEventKey.PlayerDeparture);
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
			switch (args.Tag)
			{
				case NetworkTags.PlayerCatchup:
					OnPlayerCatchup(sender, args);
					return;
				
				case NetworkTags.PlayerArrival:
					OnPlayerArrival(sender, args);
					return;
				
				case NetworkTags.PlayerDeparture:
					OnPlayerDeparture(sender, args);
					return;
			}
			
			_messageDispatcher.Dispatch(this, sender, args);
		}

		private void OnPlayerCatchup(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			while (reader.Position < reader.Length)
			{
				var entity = _world.CreateEntity(reader);
				if (!entity.TryGetComponent(out PlayerComponent playerComponent))
					continue;

				var presentation = _world.GetPresentationHandle(entity);
				var player = new Player(entity, playerComponent, presentation.Instance);
				AddPlayer(player);

				if (playerComponent.Id == _client.ID)
					_localPlayer = player;
			}
		}

		private void OnPlayerArrival(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			var playerEntity = _world.CreateEntity(reader);
			
			var presentation = _world.GetPresentationHandle(playerEntity);
			var player = new Player(playerEntity, playerEntity.GetComponent<PlayerComponent>(), presentation.Instance);
			AddPlayer(player);
		}

		private void OnPlayerDeparture(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			var clientId = reader.ReadUInt16();
			var associatedEntity = default(Entity);
			
			foreach (var entity in _world.Entities)
			{
				if (!entity.TryGetComponent(out PlayerComponent playerComponent)
					|| playerComponent.Id != clientId)
					continue;

				associatedEntity = entity;
				break;
			}

			if (associatedEntity == null)
				return;

			var playerIndex = _players.FindIndex(candidate => candidate.Entity == associatedEntity);
			var player = _players[playerIndex];
			
			_eventHub.Invoke(SharedEventKey.PlayerDeparture, this, new PlayerDepartureEventArgs(player));
			_players.RemoveAt(playerIndex);
			_world.Destroy(associatedEntity);
		}

		private void AddPlayer(Player player)
		{
			_players.Add(player);
			_eventHub.Invoke(SharedEventKey.PlayerArrival, this, new PlayerArrivalEventArgs(player));
		}

		void ILateDisposable.LateDispose()
		{
			_eventHub.Remove(SharedEventKey.PlayerArrival);
			_eventHub.Remove(SharedEventKey.PlayerDeparture);
		}
	}
}