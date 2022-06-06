using System.Collections.Generic;
using KAG.Shared.Prototype;
using Newtonsoft.Json;

namespace KAG.Shared.Json
{
	public static class JsonUtilities
	{
		public static readonly JsonSerializerSettings StandardSerializerSettings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			TypeNameHandling = TypeNameHandling.Auto,
			ContractResolver = new CustomContractResolver(),
			Converters = new List<JsonConverter>()
			{
				new IdentityConverter()
			}
		};
	}
}