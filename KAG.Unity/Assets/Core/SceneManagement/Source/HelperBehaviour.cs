using KAG.Unity.Common.Models;
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

		[Inject]
		public void Inject(JoinMatchViewModel joinMatchViewModel, ApplicationModel applicationModel)
		{
			_joinMatchViewModel = joinMatchViewModel;
			_applicationModel = applicationModel;
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
			_applicationModel.GoBackToLobby();
	}
}