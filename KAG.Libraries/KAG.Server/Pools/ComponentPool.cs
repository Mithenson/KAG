using System;
using System.Collections.Generic;
using Autofac;
using KAG.Shared;

namespace KAG.Server.Pools
{
	public sealed class ComponentPool : IComponentPool
	{
		#region Nested types

		private abstract class ComponentQueue
		{
			public abstract Component Create(ILifetimeScope lifetimeScope);
			public abstract void Dispose(Component component);
		}
		private sealed class ComponentQueue<TComponent> : ComponentQueue where TComponent : Component
		{
			private Queue<TComponent> _actualQueue;

			public ComponentQueue() => 
				_actualQueue = new Queue<TComponent>();

			public override Component Create(ILifetimeScope lifetimeScope) =>
				_actualQueue.Count == 0 ? lifetimeScope.Resolve<TComponent>() : _actualQueue.Dequeue();

			public override void Dispose(Component component) => 
				_actualQueue.Enqueue(component as TComponent);
		}

		#endregion

		private readonly ILifetimeScope _lifetimeScope;
		private readonly ComponentTypeRepository _componentTypeRepository;
		private readonly Dictionary<Type, ComponentQueue> _queues;
		
		public ComponentPool(ILifetimeScope lifetimeScope, ComponentTypeRepository componentTypeRepository)
		{
			_lifetimeScope = lifetimeScope;
			_componentTypeRepository = componentTypeRepository;

			_queues = new Dictionary<Type, ComponentQueue>();
		}
		
		public TComponent Acquire<TComponent>() where TComponent : Component => 
			IMP_Acquire(typeof(TComponent)) as TComponent;
		public Component Acquire(Type componentType)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_Acquire(componentType);
		}
		private Component IMP_Acquire(Type componentType)
		{
			_componentTypeRepository.ValidateComponentType(componentType);
			if (!_queues.TryGetValue(componentType, out var pool))
			{
				pool = (ComponentQueue)Activator.CreateInstance(typeof(ComponentQueue<>).MakeGenericType(componentType));
				_queues.Add(componentType, pool);
			}

			return pool.Create(_lifetimeScope);
		}

		public void Return(Component component)
		{
			if (!_queues.TryGetValue(component.GetType(), out var pool))
				throw new InvalidOperationException($"Trying to dispose of a `{nameof(component)}={component}` which was instantiated by this factory.");
			
			pool.Dispose(component);
		}
	}
}