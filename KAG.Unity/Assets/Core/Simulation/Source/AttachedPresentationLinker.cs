using KAG.Shared;
using KAG.Shared.Prototype;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Simulation
{
	[RequireComponent(typeof(PresentationBehaviour))]
	public sealed class AttachedPresentationLinker : MonoBehaviour, IPresentationLinker
	{
		public Identity Link => _link;
		
		[SerializeField]
		private Identity _link;

		private PresentationLinker _implementation;

		public void Initialize(DiContainer container)
		{
			var prefab = GetComponent<PresentationBehaviour>();
			
			_implementation = new PresentationLinker(container, _link, prefab);
			_implementation.Initialize(container);
		}

		public PresentationHandle Spawn(Entity entity) => 
			_implementation.Spawn(entity);
	}
}