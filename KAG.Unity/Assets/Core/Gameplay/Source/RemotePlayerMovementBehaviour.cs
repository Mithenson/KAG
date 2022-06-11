using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class RemotePlayerMovementBehaviour : GameplayBehaviour
	{
		private UnityClient _client;
		private PlayerComponent _player;
		private PositionComponent _position;
		
		[Inject]
		public void Inject(UnityClient client) => 
			_client = client;
		
		private void OnEnable() 
		{
			_player = Entity.GetComponent<PlayerComponent>();
			_position = Entity.GetComponent<PositionComponent>();
			
			_client.MessageReceived += OnClientMessageReceived;
		}

		private void OnDisable() =>
			_client.MessageReceived -= OnClientMessageReceived;
		
		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();

			switch (args.Tag)
			{
				case NetworkTags.RemotePlayerPositionUpdate:
					var positionUpdateMessage = reader.ReadSerializable<PlayerPositionUpdateMessage>();
					_position.Value = positionUpdateMessage.Position;
					break;
			}
		}
	}
}