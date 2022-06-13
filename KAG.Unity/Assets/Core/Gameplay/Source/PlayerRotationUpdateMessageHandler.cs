using System.Linq;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using KAG.Unity.Network;
using KAG.Unity.Simulation;

namespace KAG.Unity.Gameplay
{
	public sealed class PlayerRotationUpdateMessageHandler : GameplayMessageHandler
	{
		public override ushort Tag => NetworkTags.PlayerRotationUpdated;

		public PlayerRotationUpdateMessageHandler(UnityClient client, UnityWorld world)
			: base(client, world) { }

		public override void Handle(NetworkManager networkManager, Message message, DarkRiftReader reader)
		{
			var content = reader.ReadSerializable<PlayerRotationUpdateMessage>();
			
			var player = networkManager.Players.FirstOrDefault(candidate => candidate.Component.Id == content.ClientId);
			if (player == null)
				return;
			
			var rotation = player.Entity.GetComponent<RotationComponent>();
			rotation.Radians = content.Radians;
		}
	}
}