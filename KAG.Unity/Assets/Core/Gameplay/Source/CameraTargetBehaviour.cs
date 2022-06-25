using Cinemachine;
using KAG.Shared.Transform;
using KAG.Unity.Scenes.Models;
using UnityEngine;
using Zenject;
using Vector2 = UnityEngine.Vector2;

namespace KAG.Unity.Gameplay
{
	public sealed class CameraTargetBehaviour : GameplayBehaviour
	{
		[SerializeField]
		private Transform _cameraTarget;

		[SerializeField]
		[Min(0.0f)]
		private float _lookAheadFactor;
		
		private CinemachineVirtualCamera _virtualCamera;
		private CursorModel _cursorModel;

		[Inject]
		public void Inject(CinemachineVirtualCamera virtualCamera, CursorModel cursorModel)
		{
			_virtualCamera = virtualCamera;
			_cursorModel = cursorModel;
		}
		
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
			var cursor = _cursorModel.WorldPosition;

			_cameraTarget.transform.position = self + (cursor - self) * _lookAheadFactor;
		}

		private void OnDisable() => 
			_virtualCamera.Follow = null;
	}
}