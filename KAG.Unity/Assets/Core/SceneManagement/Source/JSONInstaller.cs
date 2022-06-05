using System.Collections.Generic;
using KAG.Shared.JSON;
using KAG.Shared.Prototype;
using Newtonsoft.Json;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class JSONInstaller : Installer<JSONInstaller>
	{
		public override void InstallBindings()
		{
			var settings = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = new CustomContractResolver(),
				Converters = new List<JsonConverter>()
				{
					new IdentityConverter()
				}
			};

			Container.BindInstance(settings).AsSingle();
		}
	}
}