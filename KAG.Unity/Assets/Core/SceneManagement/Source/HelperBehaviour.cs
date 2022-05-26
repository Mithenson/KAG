using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.UI.ViewModels;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public class HelperBehaviour : MonoBehaviour
	{
		private JoinMatchViewModel _joinMatchViewModel;
		private ApplicationModel _applicationModel;
		private NetworkManager _networkManager;

		[Inject]
		public void Inject(JoinMatchViewModel joinMatchViewModel, ApplicationModel applicationModel, NetworkManager networkManager)
		{
			_joinMatchViewModel = joinMatchViewModel;
			_applicationModel = applicationModel;
			_networkManager = networkManager;
		}

		[Button]
		public void SetJoinMatchStep(string value) => 
			_joinMatchViewModel.Step = value;
		
		[Button]
		public void SetIsJoiningMatch(bool value) => 
			_joinMatchViewModel.IsJoiningMatch = value;

		[Button]
		public void GoInGame() => 
			_applicationModel.GoInGame();
		
		[Button]
		public void GoBackToLobby() => 
			_networkManager.LeaveMatch();
	}
}