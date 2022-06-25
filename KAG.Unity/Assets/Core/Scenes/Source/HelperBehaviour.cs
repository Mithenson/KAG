using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.UI.ViewModels;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Zenject;

namespace KAG.Unity.Scenes
{
	public class HelperBehaviour : MonoBehaviour
	{
		[SerializeField]
		private InputSystemUIInputModule _inputUi;
		
		private JoinMatchViewModel JoinMatchViewModel => _container.Resolve<JoinMatchViewModel>();
		private ApplicationModel ApplicationModel => _container.Resolve<ApplicationModel>();
		private NetworkManager NetworkManager => _container.Resolve<NetworkManager>();

		private DiContainer _container;

		[Inject]
		public void Inject(DiContainer container) => 
			_container = container;

		[Button]
		public void SetJoinMatchStep(string value) => 
			JoinMatchViewModel.Step = value;
		
		[Button]
		public void SetIsJoiningMatch(bool value) => 
			JoinMatchViewModel.IsJoiningMatch = value;

		[Button]
		public void GoInGame() => 
			ApplicationModel.GoInGame();
		
		[Button]
		public void GoBackToLobby() => 
			NetworkManager.LeaveMatch();

		[Button]
		public void JackOfAllTrade(GameObject prefab)
		{
			Instantiate(prefab);
		}
	}
}