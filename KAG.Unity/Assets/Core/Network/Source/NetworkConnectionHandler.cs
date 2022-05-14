using System;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Extensions;

namespace KAG.Unity.Network
{
	public sealed class NetworkConnectionHandler
	{
		private const int PollingIntervalInMilliseconds = 1_000;
		
		private readonly UnityClient _client;
		private readonly INetworkSocketProvider _socketProvider;

		private CancellationToken _cancellationToken;
		private NetworkError _error;
		private bool _isDone;
		
		public NetworkConnectionHandler(UnityClient client, INetworkSocketProvider socketProvider)
		{
			_client = client;
			_socketProvider = socketProvider;
		}

		public async Task Connect(string clientName, CancellationToken cancellationToken)
		{
			var socketTask = _socketProvider.GetSocket(clientName, cancellationToken);
			await socketTask;

			if (socketTask.IsFaulted)
			{
				await Task.FromException(socketTask.Exception);
				return;
			}

			if (socketTask.IsCanceled)
			{
				await Task.FromCanceled(cancellationToken);
				return;
			}
			
			_cancellationToken = cancellationToken;

			var socket = socketTask.Result;
			socket.ConnectInBackground(_client, OnConnectionComplete);

			while (!_isDone)
			{
				await Task.Delay(PollingIntervalInMilliseconds);

				if (cancellationToken.IsCancellationRequested)
				{
					await Task.FromCanceled(cancellationToken);
					return;
				}

				if (_error != null)
				{
					await Task.FromException(new NetworkException(_error, "Failed to connect to the DarkRift server."));
					return;
				}
			}
		}

		private void OnConnectionComplete(Exception exception)
		{
			if (_cancellationToken.IsCancellationRequested)
			{
				if (_client.ConnectionState == ConnectionState.Connected)
					_client.Disconnect();
				
				return;
			}

			if (exception != null)
			{
				_error = new CustomNetworkError(_client.ConnectionState.ToString().FormatCamelCase(), "Unhandled error");
				return;
			}

			_isDone = true;
		}
	}
}