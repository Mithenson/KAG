using KAG.Shared;
using Zenject;

namespace KAG.Unity.Network
{
	public sealed class EntityPool : IEntityPool
	{
		private readonly MemoryPool<Entity> _implementation;
		
		public EntityPool(MemoryPool<Entity> implementation) => 
			_implementation = implementation;

		public Entity Acquire(ushort id)
		{
			var entity = _implementation.Spawn();
			entity.Id = id;

			return entity;
		}

		public void Return(Entity entity)
		{
			entity.RemoveAllComponents();
			_implementation.Despawn(entity);
		}
	}
}