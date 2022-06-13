using KAG.Unity.Common;
using KAG.Unity.Network.Models;
using UnityEngine;

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

		public string PingText
		{
			get => _pingText;
			set => ChangeProperty(ref _pingText, value);
		}
		private string _pingText;

		public Color PingColor
		{
			get => _pingColor;
			set => ChangeProperty(ref _pingColor, value);
		}
		private Color _pingColor;

		public ConnectivityViewModel(ConnectivityModel model)
			: base(model)
		{
			AddMethodBinding(nameof(ConnectivityModel.GotDisconnected), nameof(OnGotDisconnectedChanged));
			AddMethodBinding(nameof(ConnectivityModel.IsLeavingDueToDisconnection), nameof(OnIsLeavingDueToDisconnectionChanged));
			AddMethodBinding(nameof(ConnectivityModel.SecondsLeftUntilAutomaticExit), nameof(OnSecondLeftUntilAutomaticExitChanged));
			AddMethodBinding(nameof(ConnectivityModel.Ping), nameof(OnPingChanged));
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

		public void OnPingChanged(int ping)
		{
			PingText = $"{ping:000}ms";

			for (var i = 0; i < UnityConstants.Network.PingData.Length; i++)
			{
				if (ping > UnityConstants.Network.PingData[i].Treshold)
					continue;

				PingColor = UnityConstants.Network.PingData[i].Color;
				break;
			}
		}
	}
}