using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Scenes.Models
{
	public sealed class CursorModel : Observable
	{
		public bool IsActive
		{
			get => _isActive;
			set => ChangeProperty(ref _isActive, value);
		}
		private bool _isActive;

		public CursorState State
		{
			get => _state;
			set => ChangeProperty(ref _state, value);
		}
		private CursorState _state;

		public Vector2 WorldPosition => 
			Camera.main.ScreenToWorldPoint(Position);

		public Vector2 Position
		{
			get => _position;
			set => ChangeProperty(ref _position, value);
		}
		private Vector2 _position;
	}
}