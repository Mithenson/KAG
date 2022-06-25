using System;
using KAG.Unity.Common;
using KAG.Unity.Common.Models;
using KAG.Unity.Scenes.Models;
using KAG.Unity.UI.ViewModels;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class CursorService : ITickable, IDisposable
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
		private readonly InputAction _lookAction;
		private readonly ApplicationModel _applicationModel;
		private readonly UIViewModel _uiViewModel;
		private readonly CursorModel _model;

		private bool _hasFocus;
		private bool _isActive;
		
		public CursorService(
			ConfigurationMonitor<CursorConfiguration> configurationMonitor,
			[Inject(Id = UnityConstants.Inputs.LookAction)] InputAction lookAction,
			ApplicationModel applicationModel, 
			UIViewModel uiViewModel,
			CursorModel model)
		{
			_configurationMonitor = configurationMonitor;
			_lookAction = lookAction;
			_applicationModel = applicationModel;
			_uiViewModel = uiViewModel;
			_model = model;
			
			_hasFocus = Application.isFocused;
			Application.focusChanged += OnFocusChanged;
		}

		private void OnFocusChanged(bool hasFocus) =>
			_hasFocus = hasFocus;

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
			_model.Position = _lookAction.ReadValue<Vector2>();
		}
		
		public void CheckForInGame(ref CursorState state) =>
			state = UpdateState(_applicationModel.GameStatus == GameStatus.InGame, state, CursorState.InGame);

		public void CheckForInUI(ref CursorState state) => 
			state = UpdateState(_uiViewModel.IsInPanel, state, CursorState.InPanel);

		public void CheckForIsHovering(ref CursorState state) =>
			state = UpdateState(_uiViewModel.IsHoveringAnyElement, state, CursorState.IsHovering);

		private CursorState UpdateState(bool condition, CursorState state, CursorState flags)
		{
			if (condition)
				state |= flags;
			else if (state.HasFlag(flags))
				state ^= flags;

			return state;
		}

		void IDisposable.Dispose()
		{
			Application.focusChanged -= OnFocusChanged;
			IsActive = false;
		}
	}
}