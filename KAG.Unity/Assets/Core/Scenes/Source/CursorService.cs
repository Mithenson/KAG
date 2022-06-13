using System;
using DarkRift.Client.Unity;
using KAG.Shared.Events;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.Scenes.Models;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class CursorService : IInitializable, IDisposable
	{
		private readonly CursorModel _model;
		private readonly UnityClient _client;
		private readonly EventHub _eventHub;

		private bool _isActive;
		
		public CursorService(CursorModel model, UnityClient client, EventHub eventHub)
		{
			_model = model;
			_client = client;
			_eventHub = eventHub;

			_isActive = true;
			
			OnFocusChanged(Application.isFocused);
			Application.focusChanged += OnFocusChanged;
		}

		private void OnFocusChanged(bool hasFocus)
		{
			_model.IsActive = hasFocus;
			
			if (!hasFocus)
				return;
            
			Cursor.lockState = CursorLockMode.Confined;
			Cursor.visible = false;
		}

		private void OnPlayerArrival(object sender, PlayerArrivalEventArgs args)
		{
			if (args.Player.Component.Id != _client.ID)
				return;
			
			_eventHub.Unsubscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);

			var socketRepository = args.Player.Presentation.GetComponent<SocketRepository>();
			if (socketRepository == null)
				throw new InvalidOperationException($"The local player's `{nameof(args.Player.Presentation)}={args.Player.Presentation}` has no {nameof(SocketRepository)}.");
			
			_model.Target = socketRepository[Socket.Cursor];
		}

		private void OnSceneTransition(object sender, SceneTransitionEventArgs args)
		{
			if (args.Destination != GameStatus.InLobby)
				return;
			
			_eventHub.Unsubscribe<SceneTransitionEventArgs>(EventKey.SceneTransition, OnSceneTransition);
			Dispose();
		}

		void IInitializable.Initialize()
		{
			_eventHub.Subscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);
			_eventHub.Subscribe<SceneTransitionEventArgs>(EventKey.SceneTransition, OnSceneTransition);
		}

		void IDisposable.Dispose() =>
			Dispose();
		private void Dispose()
		{
			if (!_isActive)
				return;
			
			Application.focusChanged -= OnFocusChanged;
			
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			_isActive = false;
		}
	}
}