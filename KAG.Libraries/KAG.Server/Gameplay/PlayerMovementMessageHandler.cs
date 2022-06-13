using DarkRift;
using DarkRift.Server;
using KAG.Shared;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;

namespace KAG.Server.Gameplay
{
	public sealed class PlayerMovementMessageHandler : GameplayMessageHandler
	{
		public override ushort Tag => NetworkTags.PlayerMovement;

		public PlayerMovementMessageHandler(CorePlugin plugin, World world)
			: base(plugin, world) { }
		
		public override void Handle(IClient client, Message message, DarkRiftReader reader)
		{
			var content = reader.ReadSerializable<PlayerMovementMessage>();
						
			var player = ConnectedPlayers[client];
			var movement = player.Entity.GetComponent<MovementComponent>();
						
			movement.Move(content.Input, out var updatedPosition);
			SendPositionUpdate(client.ID, content.Id, updatedPosition);
		}

		private void SendPositionUpdate(ushort sourceClientId, ushort inputId, Vector2 updatedPosition)
		{
			using var writer = DarkRiftWriter.Create();

			writer.Write(new PlayerPositionUpdateMessage()
			{
				ClientId = sourceClientId,
				Id = inputId,
				Position = updatedPosition
			});

			using var message = Message.Create(NetworkTags.PlayerPositionUpdate, writer);

			foreach (var client in ClientManager.GetAllClients())
				client.SendMessage(message, SendMode.Unreliable);
		}
	}
}