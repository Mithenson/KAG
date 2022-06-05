using System;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using KAG.Unity.Common.Models;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Network
{
	public sealed class NetworkManager
	{
		private readonly DiContainer _container;
		private readonly UnityClient _client;
		private readonly World _world;
		private readonly ApplicationModel _applicationModel;
		private readonly PlayerModel _playerModel;
		private readonly JoinMatchModel _joinMatchModel;
		
		public NetworkManager(
			DiContainer container, 
			UnityClient client, 
			World world,
			ApplicationModel applicationModel, 
			PlayerModel playerModel,
			JoinMatchModel joinMatchModel)
		{
			_container = container;
			_client = client;
			_world = world;
			_applicationModel = applicationModel;
			_playerModel = playerModel;
			_joinMatchModel = joinMatchModel;
		}

		public async Task JoinMatch(CancellationToken cancellationToken)
		{
			var connectionHandler = _container.Resolve<JoinMatchHandler>();
			var connectionTask = connectionHandler.Execute(_playerModel.Id, cancellationToken);

			try
			{
				await connectionTask;
			}
			catch
			{
				if (connectionTask.IsCanceled)
				{
					await Task.FromCanceled(cancellationToken);
					return;
				}
				
				if (connectionTask.IsFaulted)
				{
					await Task.FromException(connectionTask.Exception);
					return;
				}

				await Task.FromException(new Exception("An unknown exception has occured."));
				return;
			}

			if (!connectionTask.IsCompleted)
				return;

			await _applicationModel.GoInGame();
			
			_client.MessageReceived += OnClientMessageReceived;
			SendPlayerIdentificationMessage(_playerModel.Name);

			await Task.CompletedTask;
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
			
			_world.Clear();
			_joinMatchModel.Status = JoinMatchStatus.Idle;

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
		
		private void CreateEntityFromReader(DarkRiftReader reader)
		{
			var entityId = reader.ReadUInt16();
			var entity = _world.CreateEntity(entityId);
			
			reader.ReadSerializableInto(ref entity);
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