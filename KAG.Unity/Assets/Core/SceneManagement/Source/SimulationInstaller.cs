using System;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Unity.Network;
using UnityEngine;
using Zenject;
using Component = KAG.Shared.Component;

namespace KAG.Unity.SceneManagement
{
	public sealed class SimulationInstaller : Installer<SimulationInstaller>
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
        
		public override void InstallBindings()
		{
			Container.BindMemoryPool<Entity, MemoryPool<Entity>>();
			Container.BindInterfacesAndSelfTo<EntityPool>().AsSingle();
            
			var componentTypeRepository = new ComponentTypeRepository();
			Container.BindInterfacesAndSelfTo<ComponentTypeRepository>().FromInstance(componentTypeRepository).AsSingle();

			foreach (var componentType in componentTypeRepository.ComponentTypes)
			{
				var poolBinderType = typeof(ComponentPoolBinder<>).MakeGenericType(componentType);
				var poolBinder = (ComponentPoolBinder)Activator.CreateInstance(poolBinderType);
				poolBinder.Bind(Container);
			}

			Container.BindInterfacesAndSelfTo<ComponentPool>().AsSingle();

			Container.BindInterfacesAndSelfTo<World>().AsSingle();
		}
	}
}