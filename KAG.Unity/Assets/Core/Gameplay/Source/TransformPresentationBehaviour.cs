using KAG.Shared.Gameplay;
using KAG.Shared.Transform;
using KAG.Unity.Common.Utilities;
using UnityEngine;

namespace KAG.Unity.Gameplay
{
	public sealed class TransformPresentationBehaviour : GameplayBehaviour
	{
		private PositionComponent _position;
		
		private void OnEnable()
		{
			_position = Entity.GetComponent<PositionComponent>();
			transform.position = _position.Value.ToUnity();
		}

		private void Update()
		{
			if (Entity.TryGetComponent(out MovementComponent movement))
			{
				var maxDistanceDelta = movement.Speed * Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, _position.Value.ToUnity(), maxDistanceDelta);
			}
			else
			{
				transform.position = _position.Value.ToUnity();
			}
		}
	}
}