using KAG.Shared.Transform;
using KAG.Unity.Common.Utilities;
using UnityEngine;

namespace KAG.Unity.Gameplay
{
	public sealed class RotationInterpolationBehaviour : GameplayBehaviour
	{
		[SerializeField]
		private Transform _target;
		
		[SerializeField, Min(0.0f)]
		private float _speed;
		
		private RotationComponent _rotation;

		private void OnEnable()
		{
			_rotation = Entity.GetComponent<RotationComponent>();
			_target.rotation = _rotation.Radians.ToRotation();
		}

		private void Update() =>
			_target.rotation = Quaternion.Lerp(_target.rotation, _rotation.Radians.ToRotation(), _speed * Time.deltaTime);
	}
}