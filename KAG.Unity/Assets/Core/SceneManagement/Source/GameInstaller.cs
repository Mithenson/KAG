using System.Collections.Generic;
using System.Threading.Tasks;
using KAG.Shared;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.Simulation;
using KAG.Unity.UI.ViewModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public class GameInstaller : SceneInstaller
	{
		private const int DelayBeforeLoadingAssetsInMilliseconds = 250;
		
		private ComponentTypeRepository _componentTypeRepository;

		[Inject]
		public void Inject(ComponentTypeRepository componentTypeRepository) =>
			_componentTypeRepository = componentTypeRepository;
		
		public override void InstallBindings()
		{
			SimulationInstaller.Install(Container, _componentTypeRepository);
			NetworkInstaller.Install(Container);
			
			Container.BindInterfacesAndSelfTo<LeaveMatchViewModel>().AsSingle();

			for (var i = 0; i < SceneManager.sceneCount; i++)
			{
				var scene = SceneManager.GetSceneAt(i);
				if (scene.buildIndex != Constants.Scenes.GameSceneIndex)
					continue;
				
				foreach (var root in scene.GetRootGameObjects())
				{
					if (root.TryGetComponent(out ParentingMarkerBehaviour parentingMarker))
						Container.Bind<Transform>().WithId(parentingMarker.Value).FromInstance(parentingMarker.transform);
				}
			}
		}

		private new async void Start()
		{
			var presentationLinkersLoadOperation = new AssetLoadOperation<Object>(Constants.Addressables.PresentationLinkerLabel);

			await Task.Delay(DelayBeforeLoadingAssetsInMilliseconds);
			await LoadAssets(presentationLinkersLoadOperation);
			
			SetupPresentationLinkers(presentationLinkersLoadOperation);

			var networkManager = Container.Resolve<NetworkManager>();
			networkManager.Start();

			var applicationModel = Container.Resolve<ApplicationModel>();
			applicationModel.IsLoading = false;
			applicationModel.GameStatus = GameStatus.InGame;
		}

		private void SetupPresentationLinkers(AssetLoadOperation<Object> loadOperation)
		{
			var presentationLinkers = new List<IPresentationLinker>();

			foreach (var candidate in loadOperation.Results)
			{
				IPresentationLinker presentationLinker;

				if (candidate is IPresentationLinker)
					presentationLinker = (IPresentationLinker)candidate;
				else if (candidate is GameObject gameObject)
				{
					presentationLinker = gameObject.GetComponent<IPresentationLinker>();

					if (presentationLinker == null)
						continue;
				}
				else continue;
				
				presentationLinker.Initialize(Container);
				presentationLinkers.Add(presentationLinker);
			}

			var world = Container.Resolve<UnityWorld>();
			world.Initialize(presentationLinkers);
		}
	}
}