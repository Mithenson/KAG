using System;
using System.Linq;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Events;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using KAG.Unity.Network;
using KAG.Unity.Simulation;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class PlayerPositionUpdateMessageHandler : GameplayMessageHandler, IInitializable, IDisposable
	{
		public override ushort Tag => NetworkTags.PlayerPositionUpdate;

		private readonly EventHub _eventHub;

		private bool _isReady;
		private LocalPlayerMovementBehaviour _localPlayerMovement;

		public PlayerPositionUpdateMessageHandler(UnityClient client, UnityWorld world, EventHub eventHub)
			: base(client, world) =>
			_eventHub = eventHub;

		void IInitializable.Initialize() =>
			_eventHub.Subscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);

		private void OnPlayerArrival(object sender, PlayerArrivalEventArgs args)
		{
			if (args.Player.Component.Id != _client.ID)
				return;

			_localPlayerMovement = args.Player.Presentation.GetComponent<LocalPlayerMovementBehaviour>();
			if (_localPlayerMovement == null)
				throw new InvalidOperationException($"The local player `{nameof(args.Player.Presentation)}={args.Player.Presentation}` has no {nameof(LocalPlayerMovementBehaviour)}.");

			_isReady = true;
			_eventHub.Unsubscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);
		}

		public override void Handle(NetworkManager networkManager, Message message, DarkRiftReader reader)
		{
			if (!_isReady)
				throw new InvalidOperationException("The handler is not ready yet.");

			var content = reader.ReadSerializable<PlayerPositionUpdateMessage>();
			if (content.ClientId == _client.ID)
			{
				_localPlayerMovement.Reconcile(content);
			}
			else
			{
				var player = networkManager.Players.First(candidate => candidate.Component.Id == content.ClientId);
				var position = player.Entity.GetComponent<PositionComponent>();
				
				position.Value = content.Position;
			}
		}

		void IDisposable.Dispose()
		{
			if (!_isReady)
				_eventHub.Unsubscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);
		}
	}
}