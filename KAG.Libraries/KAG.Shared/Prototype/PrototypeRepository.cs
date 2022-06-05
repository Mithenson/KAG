using System.Collections.Generic;
using System.Data;

namespace KAG.Shared.Prototype
{
	public class PrototypeRepository
	{
		private Dictionary<Identity, Entity> _prototypeEntities;

		public PrototypeRepository(IEnumerable<Prototype> prototypes, ComponentTypeRepository componentTypeRepository)
		{
			_prototypeEntities = new Dictionary<Identity, Entity>();

			foreach (var prototype in prototypes)
			{
				if (_prototypeEntities.ContainsKey(prototype.Identity))
					throw new InvalidConstraintException($"The `{nameof(prototype.Identity)}={prototype.Identity}` is associated to more than one {nameof(Prototype)}.");
					
				_prototypeEntities.Add(prototype.Identity, prototype.CreateEntity(componentTypeRepository));
			}
		}

		public bool TryGetPrototypeEntity(Identity identity, out Entity entity) => 
			_prototypeEntities.TryGetValue(identity, out entity);
	}
}