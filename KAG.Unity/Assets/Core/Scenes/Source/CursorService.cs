using System;
using DarkRift.Client.Unity;
using KAG.Shared.Events;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Network;
using KAG.Unity.Scenes.Models;
using KAG.Unity.UI.ViewModels;
using UnityEditor;
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

				Cursor.visible = !value;
				
				_model.IsActive = value;
				_isActive = value;
			}
		}

		private readonly ConfigurationMonitor<CursorConfiguration> _configurationMonitor;
		private readonly ApplicationModel _applicationModel;
		private readonly UIViewModel _uiViewModel;
		private readonly CursorModel _model;
		private readonly UnityClient _client;
		private readonly EventHub _eventHub;

		private bool _hasFocus;
		private bool _isActive;
		
		public CursorService(
			ConfigurationMonitor<CursorConfiguration> configurationMonitor,
			ApplicationModel applicationModel, 
			UIViewModel uiViewModel,
			CursorModel model, UnityClient client, 
			EventHub eventHub)
		{
			_configurationMonitor = configurationMonitor;
			_applicationModel = applicationModel;
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
			if (args.Destination == GameStatus.InGame)
				_eventHub.Subscribe<PlayerArrivalEventArgs>(SharedEventKey.PlayerArrival, OnPlayerArrival);
			else
				_model.Target = null;
		}

		void IInitializable.Initialize() =>
			_eventHub.Subscribe<SceneTransitionEventArgs>(EventKey.SceneTransition, OnSceneTransition);

		void ITickable.Tick()
		{
			IsActive = _hasFocus && _configurationMonitor.IsOperational;

			if (Cursor.visible != !IsActive)
				Cursor.visible = !IsActive;

			var cursorState = _model.State;
			CheckForInGame(ref cursorState);
			CheckForInUI(ref cursorState);
			CheckForIsHovering(ref cursorState);

			_model.State = cursorState;
		}
		
		public void CheckForInGame(ref CursorState state)
		{
			if (_applicationModel.GameStatus == GameStatus.InGame)
			{
				state |= CursorState.InGame;
			}
			else
			{
				if (state.HasFlag(CursorState.InGame))
					state ^= CursorState.InGame;
			}
		}

		public void CheckForInUI(ref CursorState state)
		{
			if (_applicationModel.GameStatus != GameStatus.InGame
			    || _uiViewModel.IsInPanel)
			{
				state |= CursorState.InUI;
				
				if (state.HasFlag(CursorState.InGame))
					state ^= CursorState.InGame;
			}
			else
			{
				if (state.HasFlag(CursorState.InUI))
					state ^= CursorState.InUI;
			}
		}
		
		public void CheckForIsHovering(ref CursorState state)
		{
			if (_uiViewModel.IsHoveringAnyElement)
			{
				state |= CursorState.IsHovering;
			}
			else
			{
				if (state.HasFlag(CursorState.IsHovering))
					state ^= CursorState.IsHovering;
			}
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