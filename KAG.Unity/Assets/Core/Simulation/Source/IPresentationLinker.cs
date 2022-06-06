using KAG.Shared;
using KAG.Shared.Prototype;
using Zenject;

namespace KAG.Unity.Simulation
{
	public interface IPresentationLinker
	{
		Identity Link { get; }

		void Initialize(DiContainer container);
		PresentationHandle Spawn(Entity entity);
	}
}