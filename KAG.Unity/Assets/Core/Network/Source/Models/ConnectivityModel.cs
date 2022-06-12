using KAG.Unity.Common.Observables;

namespace KAG.Unity.Network.Models
{
	public sealed class ConnectivityModel : Observable
	{
		private const int SecondsBeforeAutomaticExit = 10;
		
		public bool GotDisconnected
		{
			get => _gotDisconnected;
			set => ChangeProperty(ref _gotDisconnected, value);
		}
		private bool _gotDisconnected;

		public int SecondsLeftUntilAutomaticExit
		{
			get => _secondsLeftUntilAutomaticExit;
			set => ChangeProperty(ref _secondsLeftUntilAutomaticExit, value);
		}
		private int _secondsLeftUntilAutomaticExit;

		public bool IsLeavingDueToDisconnection
		{
			get => _isLeavingDueToDisconnection;
			set => ChangeProperty(ref _isLeavingDueToDisconnection, value);
		}
		private bool _isLeavingDueToDisconnection;
	}
}