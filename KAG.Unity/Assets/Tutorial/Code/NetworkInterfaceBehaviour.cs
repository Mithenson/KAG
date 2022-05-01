using System;
using DarkRift;
using DarkRift.Client.Unity;
using TutorialPlugin;
using UnityEngine;

public class NetworkInterfaceBehaviour : MonoBehaviour
{
	public static NetworkInterfaceBehaviour Instance { get; private set; }

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

	private void Start() => 
		_client = GetComponent<UnityClient>();

	public void StartLocalSession()
	{
		_client.ConnectInBackground(_client.Host, _client.Port, _client.Port, true, OnLocalSessionConnectionCompleted);
		UIBehaviour.Instance.SetInputInteractivity(false);
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
}