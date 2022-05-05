using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBehaviour : MonoBehaviour
{
	public static UIBehaviour Instance { get; private set; }

	public string Username => _inputField.text;
	
	[SerializeField]
	private TMP_InputField _inputField;

	[SerializeField]
	private Button _startSessionButton;
    
	[SerializeField]
	private Button _testLocallyButton;

	[SerializeField]
	private VerticalLayoutGroup _connectedPlayersGroup;

	[SerializeField]
	private GameObject _connectedPlayerPrefab;

	[SerializeField]
	private Button _readyButton;
    
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
		_startSessionButton.onClick.AddListener(() => NetworkInterfaceBehaviour.Instance.StartSession(_inputField.text));
		_testLocallyButton.onClick.AddListener(NetworkInterfaceBehaviour.Instance.StartLocalSession);
		_readyButton.onClick.AddListener(NetworkInterfaceBehaviour.Instance.SetPlayerReady);
		
		SetLobbyInteractivity(false);
	}
    
	public void Open() => 
		this.gameObject.SetActive(true);

	public void SetInputInteractivity(bool state)
	{
		_startSessionButton.interactable = state;
		_testLocallyButton.interactable = state;
		_inputField.interactable = state;
	}

	public void SetLobbyInteractivity(bool state) => 
		_readyButton.interactable = state;

	public void PopulateConnectedPlayers(IEnumerable<NetworkBehaviour> networkBehaviours)
	{
		foreach (Transform child in _connectedPlayersGroup.transform)
			Destroy(child.gameObject);

		foreach (var networkBehaviour in networkBehaviours)
		{
			var connectedPlayer = Instantiate(_connectedPlayerPrefab, _connectedPlayersGroup.transform);
            
			var label = connectedPlayer.GetComponentInChildren<TMP_Text>();
			label.text = networkBehaviour.Info.PlayerName;
		}
	}

	public void DisplayNetworkMessage(string message) => 
		_startSessionButton.GetComponentInChildren<TMP_Text>().text = message;

	public void Close() => 
		this.gameObject.SetActive(false);
}