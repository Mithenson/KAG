using UnityEngine;

public class MovementBehaviour : MonoBehaviour
{
	[SerializeField]
	private CharacterController _controller;
    
	[SerializeField]
	private float _speed;

	private NetworkBehaviour _networkBehaviour;

	private void Awake() =>
		_networkBehaviour = GetComponent<NetworkBehaviour>();

	private void Update()
	{
		if (!_networkBehaviour.IsActive)
			return;

		var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical")).normalized;
		if (input == Vector3.zero)
			return;
		
		var delta = input * (_speed * Time.deltaTime);
		_controller.Move(delta);
		
		NetworkManagementBehaviour.Instance.UpdatePlayerPosition(this.transform.position);
	}
}