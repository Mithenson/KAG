using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace KAG.Shared.Prototype
{
	public class PrototypeRepository
	{
		private const string PrototypeDefinitionSearchPattern = "*.proto";
		
		private Dictionary<Identity, Entity> _prototypeEntities;

		public PrototypeRepository(string[] foldersContainingPrototypeDefinitions, JsonSerializerSettings serializerSettings, ComponentTypeRepository componentTypeRepository)
		{
			_prototypeEntities = new Dictionary<Identity, Entity>();

			for (var i = 0; i < foldersContainingPrototypeDefinitions.Length; i++)
			{
				var prototypeDefinitions = Directory.GetFiles(foldersContainingPrototypeDefinitions[i], PrototypeDefinitionSearchPattern);

				foreach (var prototypeDefinition in prototypeDefinitions)
				{
					var json = File.ReadAllText(prototypeDefinition);
					var prototype = JsonConvert.DeserializeObject<Prototype>(json, serializerSettings);

					if (_prototypeEntities.ContainsKey(prototype.Identity))
					{
						throw new InvalidConstraintException(
							$"The {nameof(Prototype)} definition at `Path={prototypeDefinition}` "
							+ $"is associated to `{nameof(prototype.Identity)}={prototype.Identity}` but another {nameof(Prototype)} has already been mapped to it.");
					}
					
					_prototypeEntities.Add(prototype.Identity, prototype.CreateEntity(componentTypeRepository));
				}
			}
		}

		public bool TryGetPrototypeEntity(Identity identity, out Entity entity) => 
			_prototypeEntities.TryGetValue(identity, out entity);
	}
}