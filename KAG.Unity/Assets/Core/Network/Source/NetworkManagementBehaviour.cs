using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared;
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
	private Vector3 _localSpawnPosition;
    
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
		
		_localSpawnPosition = new Vector3(
			Random.Range(_spawnArea.bounds.min.x, _spawnArea.bounds.max.x),
			0.0f,
			Random.Range(_spawnArea.bounds.min.z, _spawnArea.bounds.max.z));
	}

	public void UpdatePlayerData()
	{
		using (var writer = DarkRiftWriter.Create())
		{
			writer.Write(new Player(_client.ID, UIBehaviour.Instance.Username));
			writer.Write(_localSpawnPosition.x);
			writer.Write(_localSpawnPosition.z);
			
			using (var message = Message.Create(Tags.PlayerDataUpdate, writer))
				_client.SendMessage(message, SendMode.Reliable);
		}
	}

	public void UpdatePlayerPosition(Vector3 position)
	{
		using (var writer = DarkRiftWriter.Create())
		{
			writer.Write(_client.ID);
			writer.Write(position.x);
			writer.Write(position.z);
			
			using (var message = Message.Create(Tags.PlayerMove, writer))
				_client.SendMessage(message, SendMode.Unreliable);
		}
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
				
				case Tags.PlayerDataUpdate:
					OnPlayerDataUpdated(message);
					break;
			
				case Tags.GameStart:
					OnGameStart(message);
					break;
				
				case Tags.PlayerMove:
					OnPlayerMoved(message);
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
				networkBehaviour = Instantiate(_localPlayerPrefab, _localSpawnPosition, Quaternion.identity);
			else
			{
				var position = Vector3.zero;
				if (reader.Position != reader.Length)
				{
					position = new Vector3(
						reader.ReadSingle(),
						0.0f,
						reader.ReadSingle());
				}

				networkBehaviour = Instantiate(_remotePlayerPlayer, position, Quaternion.identity);
			}
			
			networkBehaviour.AssignTo(player);
			_connectedNetworkBehaviours.Add(player.Id, networkBehaviour);
		}
		
		UIBehaviour.Instance.PopulateConnectedPlayers(_connectedNetworkBehaviours.Values.Select(behaviour => behaviour.Info));
	}

	private void OnPlayerDataUpdated(Message message)
	{
		using (var reader = message.GetReader())
		{
			var updatedPlayerData = reader.ReadSerializable<Player>();
			var networkBehaviour = _connectedNetworkBehaviours[updatedPlayerData.Id];
			
			networkBehaviour.UpdateData(updatedPlayerData);
			
			var position = new Vector3(
				reader.ReadSingle(),
				0.0f,
				reader.ReadSingle());
			networkBehaviour.transform.position = position;
		}
		
		UIBehaviour.Instance.PopulateConnectedPlayers(_connectedNetworkBehaviours.Values.Select(behaviour => behaviour.Info));
	}

	private void OnGameStart(Message _)
	{
		UIBehaviour.Instance.Close();

		foreach (var networkBehaviour in _connectedNetworkBehaviours.Values)
			networkBehaviour.IsActive = true;
	}

	private void OnPlayerMoved(Message message)
	{
		using (var reader = message.GetReader())
		{
			var id = reader.ReadUInt16();
			var position = new Vector3(
				reader.ReadSingle(),
				0.0f,
				reader.ReadSingle());

			_connectedNetworkBehaviours[id].transform.position = position;
		}
	}
	
	private void OnPlayerDisconnection(Message message)
	{
		using (var reader = message.GetReader())
		{
			var id = reader.ReadUInt16();
			var networkBehaviour = _connectedNetworkBehaviours[id];
			
			Destroy(networkBehaviour.gameObject);
			_connectedNetworkBehaviours.Remove(id);
		}
		
		UIBehaviour.Instance.PopulateConnectedPlayers(_connectedNetworkBehaviours.Values.Select(behaviour => behaviour.Info));
	}
}