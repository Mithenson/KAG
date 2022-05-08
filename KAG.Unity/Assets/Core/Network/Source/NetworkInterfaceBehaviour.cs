using System;
using System.Collections;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared;
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;

using EntityKey = PlayFab.ClientModels.EntityKey;

public class NetworkInterfaceBehaviour : MonoBehaviour
{
	public static NetworkInterfaceBehaviour Instance { get; private set; }

	[SerializeField]
	private string _region;

	[SerializeField]
	private string _matchmakingQueue;

	[SerializeField]
	private int _matchmakingTimeout;

	[SerializeField]
	private string _playfabTCPPortName;

	[SerializeField]
	private string _playfabUDPPortName;
	
	private UnityClient _client;
    
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		Instance = this;
	}

	private void Start()
	{
		_client = GetComponent<UnityClient>();
		
		UIBehaviour.Instance.AddListenerForStartSessionButton(() => StartSession(UIBehaviour.Instance.Username));
		UIBehaviour.Instance.AddListenerForTestLocallyButton(StartLocalSession);
		UIBehaviour.Instance.AddListenerForReadyButton(SetPlayerReady);
	}

	public void StartLocalSession()
	{
		_client.ConnectInBackground(_client.Host, _client.Port, _client.Port, true, OnLocalSessionConnectionCompleted);
		UIBehaviour.Instance.SetInputInteractivity(false);
	}

	public void StartSession(string clientName)
	{
		var request = new LoginWithCustomIDRequest()
		{
			CustomId = clientName,
			CreateAccount = true
		};
		
		var a = new GetEntityTokenRequest()
		{
			AuthenticationContext = new PlayFabAuthenticationContext()
			{
				
			} 
		}
			
			PlayFabAuthenticationAPI.GetEntityToken();
		PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnPlayfabError);
		UIBehaviour.Instance.SetInputInteractivity(false);
	}

	private void OnLoginSuccess(LoginResult loginResult) => 
		StartMatchmakingRequest(loginResult.EntityToken.Entity);

	private void StartMatchmakingRequest(EntityKey key)
	{
		PlayFabMultiplayerAPI.CreateMatchmakingTicket(new CreateMatchmakingTicketRequest()
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
									region = _region,
									latency = 100
								}
							}
						}
					}
				},
				QueueName = _matchmakingQueue,
				GiveUpAfterSeconds = _matchmakingTimeout
			},
			OnMatchmakingTicketCreated,
			OnPlayfabError);
	}

	private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult ticketResult)
	{
		StartCoroutine(PollMatchmakingTicket(ticketResult.TicketId));
		UIBehaviour.Instance.DisplayNetworkMessage("Matchmaking request sent");
	}

	private IEnumerator PollMatchmakingTicket(string ticketId)
	{
		yield return new WaitForSeconds(10.0f);
		
		PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest()
			{
				TicketId = ticketId,
				QueueName = _matchmakingQueue
			},
			OnMatchmakingTicketGotten,
			OnPlayfabError);
	}

	private void OnMatchmakingTicketGotten(GetMatchmakingTicketResult ticketResult)
	{
		if (ticketResult.Status == "Matched")
			OnMatchFound(ticketResult);
		else if (ticketResult.Status == "Canceled")
		{
			UIBehaviour.Instance.SetInputInteractivity(true);
			UIBehaviour.Instance.DisplayNetworkMessage("Start session");
		}
		else
			StartCoroutine(PollMatchmakingTicket(ticketResult.TicketId));
		
		UIBehaviour.Instance.DisplayNetworkMessage($"Matchmaking status: {ticketResult.Status}");
	}

	private void OnMatchFound(GetMatchmakingTicketResult ticketResult)
	{
		PlayFabMultiplayerAPI.GetMatch(new GetMatchRequest()
			{
				MatchId = ticketResult.MatchId,
				QueueName = _matchmakingQueue
			},
			OnMatchGotten,
			OnPlayfabError);
	}

	private void OnMatchGotten(GetMatchResult result)
	{
		var ip = result.ServerDetails.IPV4Address;
		
		int? tcpPort = null;
		int? udpPort = null;

		foreach (var port in result.ServerDetails.Ports)
		{
			if (port.Name == _playfabTCPPortName)
				tcpPort = port.Num;
			
			if (port.Name == _playfabUDPPortName)
				udpPort = port.Num;
		}
		
		if (tcpPort.HasValue && udpPort.HasValue)
			_client.ConnectInBackground(IPAddress.Parse(ip), tcpPort.Value, udpPort.Value, true, OnPlayfabSessionConnectionCompleted);
	}

	private void OnPlayfabSessionConnectionCompleted(Exception _)
	{
		if (_client.ConnectionState == ConnectionState.Connected)
		{
			NetworkManagementBehaviour.Instance.UpdatePlayerData();
			
			UIBehaviour.Instance.SetInputInteractivity(false);
			UIBehaviour.Instance.SetLobbyInteractivity(true);
		}
		else
		{
			UIBehaviour.Instance.SetInputInteractivity(true);
			UIBehaviour.Instance.SetLobbyInteractivity(false);
		}
	}

	private void OnLocalSessionConnectionCompleted(Exception _)
	{
		if (_client.ConnectionState == ConnectionState.Connected)
		{
			NetworkManagementBehaviour.Instance.UpdatePlayerData();
			UIBehaviour.Instance.SetLobbyInteractivity(true);
		}
		else
			UIBehaviour.Instance.SetInputInteractivity(true);
	}

	public void SetPlayerReady()
	{
		using (var writer = DarkRiftWriter.Create())
		{
			writer.Write(new PlayerReady(_client.ID, true));
			using (var message = Message.Create(Tags.PlayerReady, writer))
				_client.SendMessage(message, SendMode.Reliable);
		}
	}
	
	private void OnPlayfabError(PlayFabError playFabError) =>
		Debug.LogError($"Encountered a network error:\n{playFabError.GenerateErrorReport()}");
}