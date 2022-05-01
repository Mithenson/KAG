using System;
using DarkRift;
using DarkRift.Client.Unity;
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
			UIBehaviour.Instance.SetLobbyInteractivity(true);
		else
			UIBehaviour.Instance.SetInputInteractivity(true);
	}
}