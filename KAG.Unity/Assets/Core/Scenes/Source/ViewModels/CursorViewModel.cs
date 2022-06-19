using KAG.Unity.Common;
using KAG.Unity.Scenes.Models;
using KAG.Unity.UI.ViewModels;
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
			set => ChangeProperty(ref _isActive, value);
		}
		private bool _isActive;
		
		public CursorViewModel(CursorModel model) : base(model) =>
			AddPropertyBinding(nameof(CursorModel.IsActive), nameof(IsActive));
		
		void ITickable.Tick()
		{
			if (!IsActive)
				return;
			
			Position = Camera.main.WorldToScreenPoint(_model.Target.position);
		}
	}
}