using System.Collections.Generic;
using System.Linq;
using KAG.Shared;
using KAG.Shared.Prototype;
using UnityEngine;

namespace KAG.Unity.Simulation
{
	public class UnityWorld : World
	{
		private Dictionary<Identity, IPresentationLinker> _prefabLinkers;
		private Dictionary<Entity, PresentationHandle> _presentationHandles;

		public UnityWorld(IEntityPool entityPool, PrototypeRepository prototypeRepository)
			: base(entityPool, prototypeRepository) { }

		public void Initialize(IEnumerable<IPresentationLinker> prefabLinkers)
		{
			_prefabLinkers = prefabLinkers.ToDictionary(prefabLinker => prefabLinker.Link);
			_presentationHandles = new Dictionary<Entity, PresentationHandle>();
		}

		public PresentationHandle GetPresentationHandle(Entity entity) => 
			_presentationHandles[entity];

		protected override void OnEntityCreated(Entity entity)
		{
			if (!entity.TryGetComponent(out IdentityComponent identity)
			    || !_prefabLinkers.TryGetValue(identity.Value, out var prefabLinker))
				return;

			var presentationHandle = prefabLinker.Spawn(entity);
			presentationHandle.Instance.Entity = entity;
			
			_presentationHandles.Add(entity, presentationHandle);
		}

		protected override void OnEntityDestroyed(Entity entity)
		{
			if (!_presentationHandles.TryGetValue(entity, out var presentationHandle))
				return;

			presentationHandle.Despawn();
			_presentationHandles.Remove(entity);
		}
	}
}