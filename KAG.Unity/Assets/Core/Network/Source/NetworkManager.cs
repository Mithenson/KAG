using System;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Unity.Common.Models;
using Zenject;

namespace KAG.Unity.Network
{
	public sealed class NetworkManager
	{
		private readonly DiContainer _container;
		private readonly UnityClient _client;
		private readonly World _world;
		private readonly PlayerModel _playerModel;
		
		public NetworkManager(DiContainer container, /*UnityClient client, World world,*/ PlayerModel playerModel)
		{
			_container = container;
			_client = null;
			_world = null;
			_playerModel = playerModel;
		}

		public async Task JoinMatch(CancellationToken cancellationToken)
		{
			var connectionHandler = _container.Resolve<JoinMatchHandler>();
			var connectionTask = connectionHandler.Execute(_playerModel.Id, cancellationToken);

			await connectionTask;

			if (!connectionTask.IsCompleted)
				return;
			
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