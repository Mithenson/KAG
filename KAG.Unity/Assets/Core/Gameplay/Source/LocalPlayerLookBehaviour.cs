using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using KAG.Unity.Common.Utilities;
using KAG.Unity.Scenes.Models;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class LocalPlayerLookBehaviour : GameplayBehaviour
	{
		[SerializeField]
		private Transform _center;
		
		[SerializeField]
		private Transform _rotationTarget;
		
		private UnityClient _client;
		private CursorModel _cursorModel;

		private RotationComponent _rotation;
	
		[Inject]
		public void Inject(
			UnityClient client,
			CursorModel cursorModel)
		{
			_client = client;
			_cursorModel = cursorModel;
		}

		private void OnEnable() =>
			_rotation = Entity.GetComponent<RotationComponent>();

		private void Update()
		{
			var center = (UnityEngine.Vector2)_center.position;
			var lookAt = _cursorModel.WorldPosition;
			var radians = Vector3.SignedAngle(UnityEngine.Vector2.up, (lookAt - center).normalized, Vector3.forward) * Mathf.Deg2Rad;
			
			using (var writer = DarkRiftWriter.Create())
			{
				writer.Write(new PlayerLookMessage()
				{
					Radians = radians
				});

				using (var message = Message.Create(NetworkTags.PlayerLook, writer))
					_client.SendMessage(message, SendMode.Unreliable);
			}

			_rotation.Radians = radians;
			_rotationTarget.rotation = radians.ToRotation();
		}
	}
}