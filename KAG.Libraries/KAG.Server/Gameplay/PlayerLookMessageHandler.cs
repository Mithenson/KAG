using DarkRift;
using DarkRift.Server;
using KAG.Shared;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;

namespace KAG.Server.Gameplay
{
	public sealed class PlayerLookMessageHandler : GameplayMessageHandler
	{
		public override ushort Tag => NetworkTags.PlayerLook;

		public PlayerLookMessageHandler(CorePlugin plugin, World world)
			: base(plugin, world) { }
		
		public override void Handle(IClient client, Message message, DarkRiftReader reader)
		{
			var content = reader.ReadSerializable<PlayerLookMessage>();
						
			var player = ConnectedPlayers[client];
			var rotation = player.Entity.GetComponent<RotationComponent>();
			rotation.Radians = content.Radians;

			SendRotationUpdate(client.ID, content.Radians);
		}

		private void SendRotationUpdate(ushort sourceClientId, float updatedRadians)
		{
			using var writer = DarkRiftWriter.Create();

			writer.Write(new PlayerRotationUpdateMessage()
			{
				ClientId = sourceClientId,
				Radians = updatedRadians
			});

			using var message = Message.Create(NetworkTags.PlayerRotationUpdated, writer);

			foreach (var client in ClientManager.GetAllClients())
				client.SendMessage(message, SendMode.Unreliable);
		}
	}
}