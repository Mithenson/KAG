using System.Collections.Generic;
using System.Diagnostics;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared.Gameplay;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using UnityEngine;
using Zenject;

using Vector2 = KAG.Shared.Transform.Vector2;

namespace KAG.Unity.Gameplay
{
	public sealed class LocalPlayerMovementBehaviour : GameplayBehaviour
	{
		#region Nested types

		private readonly struct CachedInput
		{
			public readonly ushort Id;
			public readonly Vector2 Value;
			
			public CachedInput(ushort id, Vector2 value)
			{
				Id = id;
				Value = value;
			}
		}

		#endregion

		private const string HorizontalInputAxis = "Horizontal";
		private const string VerticalInputAxis = "Vertical";

		private UnityClient _client;

		private MovementComponent _movement;
		private PositionComponent _position;
		private List<CachedInput> _inputBuffer;
		private ushort _latestId;

		[Inject]
		public void Inject(UnityClient client) => 
			_client = client;

		private void Awake() => 
			_inputBuffer = new List<CachedInput>();

		private void OnEnable() 
		{
			_movement = Entity.GetComponent<MovementComponent>();
			_position = Entity.GetComponent<PositionComponent>();
			
			_inputBuffer.Clear();
			_latestId = 0;
		}

		private void FixedUpdate()
		{
			var input = new Vector2(Input.GetAxisRaw(HorizontalInputAxis), Input.GetAxisRaw(VerticalInputAxis));
			if (input == Vector2.Zero)
				return;

			using (var writer = DarkRiftWriter.Create())
			{
				writer.Write(new PlayerMovementMessage()
				{
					Id = _latestId,
					Input = input
				});

				using (var message = Message.Create(NetworkTags.PlayerMovement, writer))
					_client.SendMessage(message, SendMode.Unreliable);
			}
			
			_movement.Move(input);
			_inputBuffer.Add(new CachedInput(_latestId, input));
			_latestId++;
		}

		public void Reconcile(PlayerPositionUpdateMessage message)
		{
			_position.Value = message.Position;
			
			for (var i = 0; i < _inputBuffer.Count; i++)
			{
				if (_inputBuffer[i].Id != message.Id)
				{
					_inputBuffer.RemoveAt(i);
					i--;
					
					continue;
				}
				
				_inputBuffer.RemoveAt(i);
				break;
			}

			for (var i = 0; i < _inputBuffer.Count; i++)
				_movement.Move(_inputBuffer[i].Value);
		}
	}
}