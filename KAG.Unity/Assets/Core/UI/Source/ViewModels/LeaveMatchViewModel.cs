using KAG.Unity.Common;
using KAG.Unity.Common.Observables;
using KAG.Unity.Network;
using UnityEngine.InputSystem;
using Zenject;

namespace KAG.Unity.UI.ViewModels
{
	public class LeaveMatchViewModel : Observable
	{
		public bool IsActive
		{
			get => _isActive;
			set => ChangeProperty(ref _isActive, value);
		}
		private bool _isActive;

		private NetworkManager _networkManager;
		
		public LeaveMatchViewModel(NetworkManager networkManager) => 
			_networkManager = networkManager;

		public void LeaveMatch()
		{
			IsActive = false;
			
			_networkManager.LeaveMatch();
		}
	}
}