using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Json;
using KAG.Shared.Prototype;
using KAG.Unity.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
			InstallPrototypeRepository();
			
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

		private void InstallPrototypeRepository()
		{
			var prototypeDefinitionsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "prototype_definitions"));
			
			try
			{
				var prototypeDefinitions = prototypeDefinitionsBundle.LoadAllAssets<TextAsset>();
				var prototypes = new List<Prototype>(prototypeDefinitions.Length);

				for (var i = 0; i < prototypeDefinitions.Length; i++)
				{
					var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeDefinitions[i].text, JsonUtilities.StandardSerializerSettings);
					prototypes.Add(prototype);
				}

				Container.BindInterfacesAndSelfTo<PrototypeRepository>().AsSingle().WithArguments((IEnumerable<Prototype>)prototypes);
			}
			finally
			{
				prototypeDefinitionsBundle.Unload(true);
			}
		}
	}
}