﻿using DarkRift;
using DarkRift.Client.Unity;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using KAG.Unity.Common;
using KAG.Unity.Common.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class LocalPlayerLookBehaviour : GameplayBehaviour
	{
		public Transform CursorTarget =>
			_cursorTarget;
		
		[SerializeField]
		private Transform _rotationTarget;
		
		[SerializeField]
		private Transform _cursorTarget;
		
		private UnityClient _client;
		private InputAction _lookAction;

		private RotationComponent _rotation;
		
		[Inject]
		public void Inject(
			UnityClient client,
			[Inject(Id = UnityConstants.Inputs.LookAction)] InputAction lookAction)
		{
			_client = client;
			_lookAction = lookAction;
		}

		private void OnEnable() =>
			_rotation = Entity.GetComponent<RotationComponent>();

		private void Update()
		{
			var self = (UnityEngine.Vector2)transform.position;
			var lookAt = (UnityEngine.Vector2)Camera.main.ScreenToWorldPoint(_lookAction.ReadValue<UnityEngine.Vector2>());
			var radians = Vector3.SignedAngle(UnityEngine.Vector2.up, (lookAt - self).normalized, Vector3.forward) * Mathf.Deg2Rad;
			
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
			_cursorTarget.transform.position = lookAt;
		}
	}
}