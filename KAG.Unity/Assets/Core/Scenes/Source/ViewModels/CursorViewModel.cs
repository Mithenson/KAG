using KAG.Unity.Common;
using KAG.Unity.Scenes.Models;
using UnityEngine;
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
				if (_model.Position == Vector2.zero)
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
		
		public CursorViewModel(
			CursorModel model,
			ConfigurationMonitor<CursorConfiguration> configurationMonitor) : base(model)
		{
			_configurationMonitor = configurationMonitor;
			
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
				if (!_model.IsActive 
				    || _model.Position == Vector2.zero)
					return;
				
				Position = _model.Position;
				IsActive = true;
				
				return;
			}

			Position = _model.Position;
		}
	}
}