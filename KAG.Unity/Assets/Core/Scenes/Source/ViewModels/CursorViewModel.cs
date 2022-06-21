using KAG.Unity.Common;
using KAG.Unity.Scenes.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Zenject;

namespace KAG.Unity.Scenes.ViewModels
{
	public sealed class CursorViewModel : ViewModel<CursorModel>, ITickable
	{
		public Vector2 Position
		{
			get => _position;
			set => ChangeProperty(ref _position, value);
		}
		private Vector2 _position;

		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (!_model.HasTarget && _lookAction.ReadValue<Vector2>() == Vector2.zero)
					return;
				
				ChangeProperty(ref _isActive, value);
			}
		}
		private bool _isActive;

		public Sprite Sprite
		{
			get => _sprite;
			set => ChangeProperty(ref _sprite, value);
		}
		private Sprite _sprite;

		public Vector2 Offset
		{
			get => _Offset;
			set => ChangeProperty(ref _Offset, value);
		}

		private Vector2 _Offset;

		private readonly ConfigurationMonitor<CursorConfiguration> _configurationMonitor;
		private readonly InputAction _lookAction;
		
		public CursorViewModel(
			CursorModel model,
			ConfigurationMonitor<CursorConfiguration> configurationMonitor,
			[Inject(Id = UnityConstants.Inputs.LookAction)] InputAction lookAction) : base(model)
		{
			_configurationMonitor = configurationMonitor;
			_lookAction = lookAction;
			
			AddPropertyBinding(nameof(CursorModel.IsActive), nameof(IsActive));
			AddMethodBinding(nameof(CursorModel.State), nameof(OnStateChanged));

			_configurationMonitor.OnInitialized += OnConfigurationInitialized;
		}

		public void OnStateChanged(CursorState state)
		{
			if (!_configurationMonitor.IsOperational)
				return;

			AssignVisual(_configurationMonitor.Configuration.GetMatchingVisual(state));
		}

		private void OnConfigurationInitialized()
		{
			AssignVisual(_configurationMonitor.Configuration.GetMatchingVisual(_model.State));
			_configurationMonitor.OnInitialized -= OnConfigurationInitialized;
		}

		private void AssignVisual(CursorVisual visual)
		{
			Sprite = visual.Sprite;
			Offset = visual.Offset;
		}

		void ITickable.Tick()
		{
			if (!IsActive)
			{
				if (!_model.IsActive)
					return;

				if (_model.HasTarget)
				{
					Position = Camera.main.WorldToScreenPoint(_model.Target.position);
					IsActive = true;
				}
				else
				{
					var mousePosition = _lookAction.ReadValue<Vector2>();
					if (mousePosition == Vector2.zero)
						return;

					Position = mousePosition;
					IsActive = true;
				}
				
				return;
			}

			Position = _model.HasTarget 
				? Camera.main.WorldToScreenPoint(_model.Target.position)
				: (Vector3)_lookAction.ReadValue<Vector2>();
		}
	}
}