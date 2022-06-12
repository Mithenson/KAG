using KAG.Shared;
using KAG.Shared.Network;
using KAG.Unity.Simulation;

namespace KAG.Unity.Network
{
	public sealed class Player
	{
		public readonly Entity Entity;
		public readonly PlayerComponent Component;
		public readonly PresentationBehaviour Presentation;
		
		public Player(Entity entity, PlayerComponent component, PresentationBehaviour presentation)
		{
			Component = component;
			Entity = entity;
			Presentation = presentation;
		}
	}
}