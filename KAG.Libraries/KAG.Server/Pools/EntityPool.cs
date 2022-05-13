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

		public Entity Acquire(ushort id) =>
			_queue.Count == 0 ? _lifetimeScope.Resolve<Entity>(new PositionalParameter(0, id)) : _queue.Dequeue();

		public void Return(Entity entity)
		{
			entity.RemoveAllComponents();
			_queue.Enqueue(entity);
		}
	}
}