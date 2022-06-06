using System;
using KAG.Shared;
using Zenject;

namespace KAG.Unity.Simulation
{
	public sealed class ComponentPool : IComponentPool
	{
		private readonly DiContainer _container;
		private readonly ComponentTypeRepository _componentTypeRepository;
		
		public ComponentPool(DiContainer container, ComponentTypeRepository componentTypeRepository)
		{
			_container = container;
			_componentTypeRepository = componentTypeRepository;
		}

		public TComponent Acquire<TComponent>() where TComponent : Component => 
			(TComponent)IMP_Acquire(typeof(TComponent));
		public Component Acquire(Type componentType)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_Acquire(componentType);
		}
		public Component IMP_Acquire(Type componentType)
		{
			var pool = _container.ResolveId<MemoryPool<Component>>(componentType.GetHashCode());
			return pool.Spawn();
		}
		
		public void Return(Component component)
		{
			var pool = _container.ResolveId<MemoryPool<Component>>(component.GetType().GetHashCode());
			pool.Despawn(component);
		}
	}
}