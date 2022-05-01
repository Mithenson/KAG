using System;
using System.Collections.Generic;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using TutorialPlugin;
using UnityEngine;
using Random = UnityEngine.Random;

public class NetworkManagementBehaviour : MonoBehaviour
{
	public static NetworkManagementBehaviour Instance { get; private set; }

	[SerializeField]
	private NetworkBehaviour _localPlayerPrefab;

	[SerializeField]
	private NetworkBehaviour _remotePlayerPlayer;

	[SerializeField]
	private BoxCollider _spawnArea;
	
	private UnityClient _client;
	private Dictionary<ushort, NetworkBehaviour> _connectedNetworkBehaviours;
    
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(this.gameObject);
			return;
		}

		Instance = this;

		_connectedNetworkBehaviours = new Dictionary<ushort, NetworkBehaviour>();
		
		_client = GetComponent<UnityClient>();
		_client.MessageReceived += OnMessageReceived;
	}

	private void OnMessageReceived(object sender, MessageReceivedEventArgs args)
	{
		using (var message = args.GetMessage())
		{
			switch (message.Tag)
			{
				case Tags.PlayerConnection:
					OnPlayerConnection(message);
					break;
				
				case Tags.PlayerDisconnection:
					OnPlayerDisconnection(message);
					break;
			}
		}
	}

	private void OnPlayerConnection(Message message)
	{
		using (var reader = message.GetReader())
		{
			var player = reader.ReadSerializable<Player>();

			var networkBehaviour = default(NetworkBehaviour);
			if (player.Id == _client.ID)
				networkBehaviour = Instantiate(_localPlayerPrefab);
			else
				networkBehaviour = Instantiate(_remotePlayerPlayer);

			var position = new Vector3(
				Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
				0.0f,
				Random.Range(_spawnArea.bounds.min.z, _spawnArea.bounds.max.z));

			networkBehaviour.transform.position = position;
			networkBehaviour.AssignTo(player);
			
			_connectedNetworkBehaviours.Add(player.Id, networkBehaviour);
		}
	}
	private void OnPlayerDisconnection(Message message)
	{
		using (var reader = message.GetReader())
		{
			var player = reader.ReadSerializable<Player>();

			if (!_connectedNetworkBehaviours.TryGetValue(player.Id, out var networkBehaviour))
				throw new InvalidOperationException($"A disconnected player is always meant to have a `{nameof(NetworkBehaviour)}`.");
			
			Destroy(networkBehaviour.gameObject);
			_connectedNetworkBehaviours.Remove(player.Id);
		}
	}
}