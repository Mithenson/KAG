using Cinemachine;
using KAG.Shared.Transform;
using KAG.Unity.Common;
using UnityEngine;
using Zenject;

using Vector2 = UnityEngine.Vector2;

namespace KAG.Unity.Gameplay
{
	[RequireComponent(typeof(SocketRepository))]
	public sealed class CameraTargetBehaviour : GameplayBehaviour
	{
		[SerializeField]
		private Transform _cameraTarget;

		[SerializeField]
		[Min(0.0f)]
		private float _lookAheadFactor;
		
		private CinemachineVirtualCamera _virtualCamera;
		private SocketRepository _socketRepository;

		[Inject]
		public void Inject(CinemachineVirtualCamera virtualCamera) =>
			_virtualCamera = virtualCamera;

		private void Awake() => 
			_socketRepository = GetComponent<SocketRepository>();

		private void OnEnable()
		{
			var position = Entity.GetComponent<PositionComponent>();
			
			_virtualCamera.ForceCameraPosition(
				new Vector3(position.Value.X, position.Value.Y, _virtualCamera.transform.position.z), 
				_virtualCamera.transform.rotation);
			
			_virtualCamera.Follow = _cameraTarget;
		}

		private void Update()
		{
			var self = (Vector2)transform.position;
			var cursor = (Vector2)_socketRepository[Socket.Cursor].position;

			_cameraTarget.transform.position = self + (cursor - self) * _lookAheadFactor;
		}

		private void OnDisable() => 
			_virtualCamera.Follow = null;
	}
}