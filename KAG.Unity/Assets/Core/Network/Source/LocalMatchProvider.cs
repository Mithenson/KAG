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
		public event Action<string> OnProgress
		{
			add { }
			remove { }
		} 
		
		[SerializeField]
		private NetworkSocket _socket;

		private TrackedProcess _localServerProcess;

		[Inject]
		public void Inject([Inject(Id = InjectionKey.LocalServerProcess)]
			TrackedProcess localServerProcess) => _localServerProcess = localServerProcess;

		public Task<Match> GetMatch(string playerId, CancellationToken __)
		{
			if (!_localServerProcess.Value.Responding)
				return Task.FromException<Match>(new InvalidOperationException("The local server process is either not responding or has not been started."));
		
			return Task.FromResult(new Match(MatchKind.Local, _socket));
		}
	}
}