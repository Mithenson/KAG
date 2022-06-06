using Zenject;

namespace KAG.Unity.Simulation
{
	public struct PresentationHandle
	{
		public readonly PresentationBehaviour Instance;
		public readonly MemoryPool<PresentationBehaviour> Pool;
		
		public PresentationHandle(PresentationBehaviour instance, MemoryPool<PresentationBehaviour> pool)
		{
			Instance = instance;
			Pool = pool;
		}

		public void Despawn()
		{
			Instance.Entity = null;
			Instance.gameObject.SetActive(false);
			
			Pool.Despawn(Instance);
		}
	}
}