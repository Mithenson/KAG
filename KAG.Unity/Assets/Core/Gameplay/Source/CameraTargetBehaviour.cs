using Cinemachine;
using KAG.Shared.Transform;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class CameraTargetBehaviour : GameplayBehaviour
	{
		private CinemachineVirtualCamera _virtualCamera;

		[Inject]
		public void Inject(CinemachineVirtualCamera virtualCamera) =>
			_virtualCamera = virtualCamera;

		private void OnEnable()
		{
			var position = Entity.GetComponent<PositionComponent>();
			
			_virtualCamera.ForceCameraPosition(
				new Vector3(position.Value.X, position.Value.Y, _virtualCamera.transform.position.z), 
				_virtualCamera.transform.rotation);
			
			_virtualCamera.Follow = this.transform;
		}

		private void OnDisable() => 
			_virtualCamera.Follow = null;
	}
}