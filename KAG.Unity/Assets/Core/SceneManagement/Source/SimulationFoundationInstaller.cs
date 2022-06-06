using System.Collections.Generic;
using System.IO;
using KAG.Shared;
using KAG.Shared.Json;
using KAG.Shared.Prototype;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class SimulationFoundationInstaller : Installer<SimulationFoundationInstaller>
	{
		public override void InstallBindings()
		{
			InstallPrototypeRepository();
			
			Container.BindInterfacesAndSelfTo<ComponentTypeRepository>().AsSingle();
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