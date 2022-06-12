using KAG.Unity.Common;
using KAG.Unity.Network.Models;

namespace KAG.Unity.UI.ViewModels
{
	public sealed class ConnectivityViewModel : ViewModel<ConnectivityModel>
	{
		public bool IsDisconnectionPanelActive
		{
			get => _isDisconnectionPanelActive;
			set => ChangeProperty(ref _isDisconnectionPanelActive, value);
		}
		private bool _isDisconnectionPanelActive;

		public string AutomaticExitDueToDisconnectionText
		{
			get => _automaticExitDueToDisconnectionText;
			set => ChangeProperty(ref _automaticExitDueToDisconnectionText, value);
		}
		private string _automaticExitDueToDisconnectionText;

		public ConnectivityViewModel(ConnectivityModel model)
			: base(model)
		{
			AddMethodBinding(nameof(ConnectivityModel.GotDisconnected), nameof(OnGotDisconnectedChanged));
			AddMethodBinding(nameof(ConnectivityModel.IsLeavingDueToDisconnection), nameof(OnIsLeavingDueToDisconnectionChanged));
			AddMethodBinding(nameof(ConnectivityModel.SecondsLeftUntilAutomaticExit), nameof(OnSecondLeftUntilAutomaticExitChanged));
		}

		public void OnGotDisconnectedChanged(bool gotDisconnected)
		{
			if (gotDisconnected)
				IsDisconnectionPanelActive = true;
		}
		
		public void OnIsLeavingDueToDisconnectionChanged(bool isExiting)
		{
			if (!isExiting)
				return;

			IsDisconnectionPanelActive = false;
			AutomaticExitDueToDisconnectionText = string.Empty;
		}

		public void OnSecondLeftUntilAutomaticExitChanged(int secondsLeft) =>
			AutomaticExitDueToDisconnectionText = $"Exiting automatically in {secondsLeft}s...";
	}
}