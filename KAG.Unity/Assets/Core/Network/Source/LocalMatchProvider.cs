using System;
using System.Threading;
using System.Threading.Tasks;
using KAG.Unity.Common;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class LocalMatchProvider : IMatchProvider
	{
		public event Action<string> OnProgress; 
		
		[SerializeField]
		private NetworkSocket _socket;

		private TrackedProcess _localServerProcess;

		[Inject]
		public void Inject([Inject(Id = InjectionKey.LocalServerProcess, Optional = true)] TrackedProcess localServerProcess) =>
			_localServerProcess = localServerProcess;

		public async Task<Match> GetMatch(string playerId, CancellationToken cancellationToken)
		{
			OnProgress?.Invoke("Waiting");
			await Task.Delay(1_250);
			
			if (cancellationToken.IsCancellationRequested)
			{
				await Task.FromCanceled<Match>(cancellationToken);
				return null;
			}

			#if UNITY_EDITOR
			
			if (!_localServerProcess.IsRunning)
			{
				await Task.FromException<Match>(
					new NetworkException(
						new CustomNetworkError("Local process not started"),
						"The local server process is either not responding or has not been started."));
				
				return null;
			}
			
			#endif

			return new Match(MatchKind.Local, _socket);
		}
	}
}