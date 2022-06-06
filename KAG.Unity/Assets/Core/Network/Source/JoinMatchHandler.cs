using System;
using System.Threading;
using System.Threading.Tasks;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Extensions;
using KAG.Unity.Common.Models;
using KAG.Unity.Network.Models;

namespace KAG.Unity.Network
{
	public sealed class JoinMatchHandler
	{
		private const int PollingIntervalInMilliseconds = 500;
		
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

		public async Task Execute(string playerId, CancellationToken cancellationToken)
		{
			_model.StartTimestamp = DateTime.UtcNow;
			
			_provider.OnProgress += OnProviderProgress;
			var task = _provider.GetMatch(playerId, cancellationToken);

			try
			{
				await task;
			}
			catch
			{
				if (task.IsCanceled)
				{
					await Task.FromCanceled(cancellationToken);
					return;
				}
				
				if (task.IsFaulted)
				{
					_model.Status = JoinMatchStatus.Faulted;
					_model.Exception = task.Exception;
					
					await Task.FromException(task.Exception);
					return;
				}

				_model.Status = JoinMatchStatus.Faulted;

				var exception = new Exception("An unknown exception has occured.");
				_model.Exception = exception;
				
				await Task.FromException(exception);
				return;
			}
			finally
			{
				_provider.OnProgress -= OnProviderProgress;
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
					if (_client.ConnectionState == ConnectionState.Connected)
						_client.Disconnect();
					
					await Task.FromCanceled(cancellationToken);
					return;
				}

				if (_error != null)
				{
					if (_client.ConnectionState == ConnectionState.Connected)
						_client.Disconnect();
					
					_model.Status = JoinMatchStatus.Faulted;
					await Task.FromException(new NetworkException(_error, "Failed to connect to the DarkRift server."));
					
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