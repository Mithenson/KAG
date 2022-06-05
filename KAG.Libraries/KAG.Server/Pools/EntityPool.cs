using System.Collections.Generic;
using Autofac;
using KAG.Shared;

namespace KAG.Server.Pools
{
	public sealed class EntityPool : IEntityPool
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly Queue<Entity> _queue;

		public EntityPool(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
			
			_queue = new Queue<Entity>();
		}

		public Entity Acquire(ushort id)
		{
			var entity = _queue.Count == 0 ? _lifetimeScope.Resolve<Entity>() : _queue.Dequeue();
			entity.Id = id;

			return entity;
		}

		public void Return(Entity entity)
		{
			entity.RemoveAllComponents();
			_queue.Enqueue(entity);
		}
	}
}