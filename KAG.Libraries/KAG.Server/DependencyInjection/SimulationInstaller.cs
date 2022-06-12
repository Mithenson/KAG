using System.Collections.Generic;
using System.IO;
using Autofac;
using KAG.Server.Pools;
using KAG.Shared;
using KAG.Shared.Json;
using KAG.Shared.Prototype;
using Newtonsoft.Json;

namespace KAG.Server.DependencyInjection
{
	public sealed class SimulationInstaller : Installer<SimulationInstaller, string>
	{
		private string _resourceDirectory;

		public SimulationInstaller(ContainerBuilder builder, string resourceDirectory)
			: base(builder) => 
			_resourceDirectory = resourceDirectory;
		
		public override void Install()
		{
			var componentTypeRepository = new ComponentTypeRepository();
			Builder.RegisterInstance(componentTypeRepository).SingleInstance();
			
			RegisterPrototypeRepository(componentTypeRepository);

			Builder.RegisterType<World>().AsSelf().SingleInstance();

			Builder.RegisterType<EntityPool>().As<IEntityPool>().SingleInstance();
			Builder.RegisterType(typeof(Entity)).AsSelf().InstancePerDependency();

			Builder.RegisterType<ComponentPool>().As<IComponentPool>().SingleInstance();

			foreach (var componentType in componentTypeRepository.ComponentTypes)
				Builder.RegisterType(componentType).AsSelf().InstancePerDependency();
		}

		private void RegisterPrototypeRepository(ComponentTypeRepository componentTypeRepository)
		{
			var prototypeDefinitionPaths = Directory.GetFiles(_resourceDirectory, "*.proto");
			var prototypes = new List<Prototype>(prototypeDefinitionPaths.Length);

			for (var i = 0; i < prototypeDefinitionPaths.Length; i++)
			{
				var prototypeDefinition = File.ReadAllText(prototypeDefinitionPaths[i]);
				var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeDefinition, JsonUtilities.StandardSerializerSettings);
				prototypes.Add(prototype);
			}

			var prototypeRepository = new PrototypeRepository();
			prototypeRepository.Initialize(prototypes, componentTypeRepository);
			
			Builder.RegisterInstance(prototypeRepository).SingleInstance();
		}
	}
}