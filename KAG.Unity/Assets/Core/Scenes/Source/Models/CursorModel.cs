using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Scenes.Models
{
	public sealed class CursorModel : Observable
	{
		public Transform Target
		{
			get => _target;
			set
			{
				ChangeProperty(ref _target, value);
				HasTarget = value != null;
			}
		}
		private Transform _target;

		public bool HasTarget
		{
			get => _hasTarget;
			set => ChangeProperty(ref _hasTarget, value);
		}
		private bool _hasTarget;

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
	}
}