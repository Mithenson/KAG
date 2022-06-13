using System;
using KAG.Shared;
using KAG.Unity.Simulation;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class SimulationInstaller : Installer<ComponentTypeRepository, SimulationInstaller>
	{
		#region Nested types

		private abstract class ComponentPoolBinder
		{
			public abstract void Bind(DiContainer container);
		}
		private sealed class ComponentPoolBinder<TComponent> : ComponentPoolBinder where TComponent : Component
		{
			public override void Bind(DiContainer container) =>
				container.BindMemoryPool<TComponent, MemoryPool<Component>>().WithId(typeof(TComponent).GetHashCode());
		}

		#endregion

		private readonly ComponentTypeRepository _componentTypeRepository;
		
		public SimulationInstaller(ComponentTypeRepository componentTypeRepository) =>
			_componentTypeRepository = componentTypeRepository;

		public override void InstallBindings()
		{
			Container.BindMemoryPool<Entity, MemoryPool<Entity>>();
			Container.BindInterfacesAndSelfTo<EntityPool>().AsSingle();
			
			foreach (var componentType in _componentTypeRepository.ComponentTypes)
			{
				var poolBinderType = typeof(ComponentPoolBinder<>).MakeGenericType(componentType);
				var poolBinder = (ComponentPoolBinder)Activator.CreateInstance(poolBinderType);
				poolBinder.Bind(Container);
			}

			Container.BindInterfacesAndSelfTo<ComponentPool>().AsSingle();

			Container.Bind(typeof(World), typeof(UnityWorld)).To<UnityWorld>().AsSingle();
		}
	}
}