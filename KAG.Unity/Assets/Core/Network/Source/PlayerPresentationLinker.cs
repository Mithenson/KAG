using System;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Shared.Prototype;
using KAG.Unity.Simulation;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Network
{
	[CreateAssetMenu(menuName = "KAG/Presentation linkers/Player", fileName = nameof(PlayerPresentationLinker))]
	public sealed class PlayerPresentationLinker : ScriptableObject, IPresentationLinker
	{
		public Identity Link =>
			Identity.Player;
		
		[SerializeField]
		private PresentationBehaviour _localPrefab;

		[SerializeField]
		private PresentationBehaviour _remotePrefab;

		private UnityClient _client;
		private PresentationLinker _localImplementation;
		private PresentationLinker _remoteImplementation;

		public void Initialize(DiContainer container)
		{
			_client = container.Resolve<UnityClient>();
			
			_localImplementation = new PresentationLinker(container, Link, _localPrefab);
			_localImplementation.Initialize(container);

			_remoteImplementation = new PresentationLinker(container, Link, _remotePrefab);
			_remoteImplementation.Initialize(container);
		}

		public PresentationHandle Spawn(Entity entity)
		{
			if (!entity.TryGetComponent(out PlayerComponent player))
				throw new InvalidOperationException(
					"Tried to spawn a presentation for..."
					+ $"\n`{nameof(entity)}={entity}"
					+ $"\nHowever, no {nameof(PlayerComponent)} was found, so it failed.");

			return player.Id == _client.ID ? _localImplementation.Spawn(entity) : _remoteImplementation.Spawn(entity);
		}
	}
}