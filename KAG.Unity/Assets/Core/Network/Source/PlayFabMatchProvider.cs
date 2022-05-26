using System;
using System.Threading;
using System.Threading.Tasks;
using KAG.Unity.Common.Models;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;
using Zenject;
using EntityKey = PlayFab.ClientModels.EntityKey;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class PlayFabMatchProvider : IMatchProvider
	{
		#region Nested types

		private enum Region
		{
			NorthEurope
		}

		private enum MatchmakingStatus
		{
			Matched,
			Canceled,
		}

		#endregion

		public event Action<string> OnProgress; 
	
		private const int PollingIntervalInMilliseconds = 2_000;
		private const int MatchmakingPollingIntervalInMilliseconds = 10_000;

		[SerializeField]
		private Region _region;
	
		[SerializeField]
		private string _matchmakingQueueName;

		[SerializeField]
		private int _matchmakingTimeoutInSeconds;

		[SerializeField]
		private string _tcpPortName;

		[SerializeField]
		private string _udpPortName;

		private CancellationToken _cancellationToken;
		private NetworkError _error;
		private NetworkSocket _socket;

		public async Task<Match> GetMatch(string playerId, CancellationToken cancellationToken)
		{
			_cancellationToken = cancellationToken;
			LoginWithCustomID(playerId);
		
			while (_socket == null)
			{
				await Task.Delay(PollingIntervalInMilliseconds);

				if (cancellationToken.IsCancellationRequested)
				{
					await Task.FromCanceled<Match>(cancellationToken);
					return null;
				}

				if (_error != null)
				{
					await Task.FromException<Match>(new NetworkException(_error, $"Failed to get a socket through the {nameof(PlayFabClientAPI)}."));
					return null;
				}
			}
	
			return new Match(MatchKind.PlayFab, _socket);	
		}

		private void LoginWithCustomID(string playerId)
		{
			var request = new LoginWithCustomIDRequest()
			{
				CustomId = playerId,
				CreateAccount = true
			};

			OnProgress?.Invoke("Logging in");
			PlayFabClientAPI.LoginWithCustomID(request, OnLoginResult, CachePlayFabError);
		}

		private void OnLoginResult(LoginResult result)
		{
			if (_cancellationToken.IsCancellationRequested)
				return;
			
			CreateMatchmakingTicket(result.EntityToken.Entity);
		}
		
		private void CreateMatchmakingTicket(EntityKey key)
		{
			var request = new CreateMatchmakingTicketRequest()
			{
				Creator = new MatchmakingPlayer()
				{
					Entity = new PlayFab.MultiplayerModels.EntityKey()
					{
						Id = key.Id,
						Type = key.Type
					},
					Attributes = new MatchmakingPlayerAttributes()
					{
						DataObject = new
						{
							Latencies = new object[]
							{
								new
								{
									region = _region.ToString(),
									latency = 100
								}
							}
						}
					}
				},
				
				QueueName = _matchmakingQueueName,
				GiveUpAfterSeconds = _matchmakingTimeoutInSeconds
			};
		
			OnProgress?.Invoke("Asking for match");
			PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnCreateMatchmakingTicketResult, CachePlayFabError);
		}
	
		private async void OnCreateMatchmakingTicketResult(CreateMatchmakingTicketResult ticketResult)
		{
			if (_cancellationToken.IsCancellationRequested)
				return;
			
			OnProgress?.Invoke("Searching for match");
			await PollMatchmakingTicket(ticketResult.TicketId);
		}

		private async Task PollMatchmakingTicket(string ticketId)
		{
			await Task.Delay(MatchmakingPollingIntervalInMilliseconds);
			
			if (_cancellationToken.IsCancellationRequested)
				return;
		
			var request = new GetMatchmakingTicketRequest()
			{
				TicketId = ticketId,
				QueueName = _matchmakingQueueName
			};
		
			PlayFabMultiplayerAPI.GetMatchmakingTicket(request, OnGetMatchmakingTicketResult, CachePlayFabError);
		}
	
		private async void OnGetMatchmakingTicketResult(GetMatchmakingTicketResult ticketResult)
		{
			if (_cancellationToken.IsCancellationRequested)
				return;
			
			switch (ticketResult.Status)
			{
				case nameof(MatchmakingStatus.Matched):
					OnGetMatchResult(ticketResult);
					break;

				case nameof(MatchmakingStatus.Canceled):
					_error = new CustomNetworkError("Matchmaking canceled");
					break;

				default:
					await PollMatchmakingTicket(ticketResult.TicketId);
					break;
			}
		}
	
		private void OnGetMatchResult(GetMatchmakingTicketResult ticketResult)
		{
			var request = new GetMatchRequest()
			{
				MatchId = ticketResult.MatchId,
				QueueName = _matchmakingQueueName
			};

			OnProgress?.Invoke("Fetching match");
			PlayFabMultiplayerAPI.GetMatch(request, OnGetMatchResult, CachePlayFabError);
		}

		private void OnGetMatchResult(GetMatchResult result)
		{
			if (_cancellationToken.IsCancellationRequested)
				return;
			
			var ipAddress = result.ServerDetails.IPV4Address;
		
			int? tcpPort = null;
			int? udpPort = null;

			foreach (var port in result.ServerDetails.Ports)
			{
				if (port.Name == _tcpPortName)
					tcpPort = port.Num;
			
				if (port.Name == _udpPortName)
					udpPort = port.Num;
			}

			if (!tcpPort.HasValue)
			{
				_error = new CustomNetworkError("TCP port not found");
				return;
			}

			if (!udpPort.HasValue)
			{
				_error = new CustomNetworkError("UDP port not found");
				return;
			}

			_socket = new NetworkSocket(ipAddress, tcpPort.Value, udpPort.Value);
		}

		private void CachePlayFabError(PlayFabError error) => 
			_error = new PlayfabNetworkError(error);
	}
}