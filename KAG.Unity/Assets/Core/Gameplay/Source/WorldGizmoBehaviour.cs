using System;
using DarkRift.Client.Unity;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Shared.Transform;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class WorldGizmoBehaviour : MonoBehaviour
	{
		private UnityClient _client;
		private World _world;
		
		[Inject]
		public void Inject(UnityClient client, World world)
		{
			_client = client;
			_world = world;
		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying)
				return;
		
			foreach (var entity in _world.Entities)
			{
				if (!entity.TryGetComponent(out PositionComponent position))
					continue;

				Color color;

				if (entity.TryGetComponent(out PlayerComponent player))
					color = player.Id == _client.ID ? Color.blue : Color.green;
				else
					color = Color.magenta;

				var prevColor = Gizmos.color;
				Gizmos.color = color;
				
				Gizmos.DrawSphere(new Vector3(position.Value.X, position.Value.Y, 0.0f), 1.0f);

				Gizmos.color = prevColor;
			}
		}
	}
}