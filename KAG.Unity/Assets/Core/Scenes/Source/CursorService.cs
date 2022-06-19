using System;
using DarkRift.Client.Unity;
using KAG.Shared.Events;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.Scenes.Models;
using KAG.Unity.UI.ViewModels;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class CursorService : IInitializable, ITickable, IDisposable
	{
		public bool IsActive
		{
			get => _isActive;
			private set
			{
				if (_isActive == value)
					return;

				if (value)
				{
					Cursor.lockState = CursorLockMode.Confined;
					Cursor.visible = false;
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}

				_model.IsActive = value;
				_isActive = value;
			}
		}

		private readonly ApplicationViewModel _applicationViewModel;
		private readonly UIViewModel _uiViewModel;
		private readonly CursorModel _model;
		private readonly UnityClient _client;
		private readonly EventHub _eventHub;

		private bool _hasFocus;
		private bool _isActive;
		
		public CursorService(
			ApplicationViewModel applicationViewModel, 
			UIViewModel uiViewModel,
			CursorModel model, UnityClient client, 
			EventHub eventHub)
		{
			_applicationViewModel = applicationViewModel;
			_uiViewModel = uiViewModel;
			_model = model;
			_client = client;
			_eventHub = eventHub;
			
			_hasFocus = Application.isFocused;
			Application.focusChanged += OnFocusChanged;
		}

		private void OnFocusChanged(bool hasFocus) =>
			_hasFocus = hasFocus;

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

		void ITickable.Tick()
		{
			IsActive = _hasFocus 
			           && !_applicationViewModel.IsLoading
			           && !_uiViewModel.IsHoveringAnyElement 
			           && !_uiViewModel.IsInPanel;
		}

		void IDisposable.Dispose() =>
			Dispose();
		private void Dispose()
		{
			if (!IsActive)
				return;
			
			Application.focusChanged -= OnFocusChanged;
			IsActive = false;
		}
	}
}