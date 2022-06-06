using KAG.Shared;
using KAG.Shared.Prototype;
using KAG.Unity.Common;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Simulation
{
	public sealed class PresentationLinker : IPresentationLinker
	{
		private static uint _incrementalId;

		public Identity Link => 
			_link;
		
		private Identity _link;
		private PresentationBehaviour _template;
		private MemoryPool<PresentationBehaviour> _pool;
		
		public PresentationLinker(DiContainer container, Identity link, PresentationBehaviour prefab)
		{
			_link = link;
			
			var templatesRoot = container.ResolveId<Transform>(ParentingMarker.PresentationsTemplate);
			_template = Object.Instantiate(prefab, templatesRoot);
			
			if (_template.gameObject.activeInHierarchy)
				_template.gameObject.SetActive(false);
		}

		public void Initialize(DiContainer container)
		{
			var root = container.ResolveId<Transform>(ParentingMarker.Presentations);
			
			_incrementalId++;
			container.BindMemoryPool<PresentationBehaviour, MemoryPool<PresentationBehaviour>>()
			   .WithId(_incrementalId)
			   .FromComponentInNewPrefab(_template)
			   .UnderTransform(root);

			_pool = container.ResolveId<MemoryPool<PresentationBehaviour>>(_incrementalId);
		}

		public PresentationHandle Spawn(Entity entity)
		{
			var instance = _pool.Spawn();
			
			instance.Entity = entity;
			instance.gameObject.SetActive(true);
			
			return new PresentationHandle(instance, _pool);
		}
	}
}