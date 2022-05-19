using System;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Extensions;
using KAG.Unity.Common.Models;

namespace KAG.Unity.Network
{
	public sealed class JoinMatchHandler
	{
		private const int PollingIntervalInMilliseconds = 1_000;
		
		private readonly UnityClient _client;
		private readonly IMatchProvider _provider;
		private readonly JoinMatchModel _model;

		private CancellationToken _cancellationToken;
		private NetworkError _error;
		private bool _isDone;
		
		public JoinMatchHandler(UnityClient client, IMatchProvider provider, JoinMatchModel model)
		{
			_client = client;
			_provider = provider;
			_model = model;
		}

		public async Task Execute(string clientName, CancellationToken cancellationToken)
		{
			_model.StartTimestamp = DateTime.UtcNow;
			
			_provider.OnProgress += OnProviderProgress;
			var task = _provider.GetMatch(clientName, cancellationToken);
			
			await task;
			_provider.OnProgress -= OnProviderProgress;
			
			if (task.IsFaulted)
			{
				await Task.FromException(task.Exception);
				
				_model.Status = JoinMatchStatus.Faulted;
				return;
			}

			if (task.IsCanceled)
			{
				await Task.FromCanceled(cancellationToken);
				
				_model.Status = JoinMatchStatus.Faulted;
				return;
			}
			
			_cancellationToken = cancellationToken;

			_model.Status = JoinMatchStatus.JoiningMatch;
			_model.Step = "Connecting";

			var match = task.Result;
			match.Socket.ConnectInBackground(_client, OnConnectionComplete);

			while (!_isDone)
			{
				await Task.Delay(PollingIntervalInMilliseconds);

				if (cancellationToken.IsCancellationRequested)
				{
					await Task.FromCanceled(cancellationToken);
					
					_model.Status = JoinMatchStatus.Faulted;
					return;
				}

				if (_error != null)
				{
					await Task.FromException(new NetworkException(_error, "Failed to connect to the DarkRift server."));
					
					_model.Status = JoinMatchStatus.Faulted;
					return;
				}
			}

			_model.Status = JoinMatchStatus.Completed;
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
				_error = new CustomNetworkError("Unhandled error");
				return;
			}

			_isDone = true;
		}

		private void OnProviderProgress(string progress) => 
			_model.Step = progress;
	}
}