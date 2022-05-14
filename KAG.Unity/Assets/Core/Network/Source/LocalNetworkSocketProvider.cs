using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class LocalNetworkSocketProvider : INetworkSocketProvider
	{
		[SerializeField]
		private NetworkSocket _socket;

		private TrackedProcess _localServerProcess;

		[Inject]
		public void Inject([Inject(Id = InjectionKey.LocalServerProcess)]
			TrackedProcess localServerProcess) => _localServerProcess = localServerProcess;

		public Task<NetworkSocket> GetSocket(string _, CancellationToken __)
		{
			if (!_localServerProcess.Value.Responding)
				return Task.FromException<NetworkSocket>(new InvalidOperationException("The local server process is either not responding or has not been started."));
		
			return Task.FromResult(_socket);
		}
	}
}