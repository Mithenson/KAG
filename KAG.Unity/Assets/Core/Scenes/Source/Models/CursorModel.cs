using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Scenes.Models
{
	public class CursorModel : Observable
	{
		public Transform Target
		{
			get => _target;
			set
			{
				ChangeProperty(ref _target, value);
				IsActive = value != null;
			}
		}
		private Transform _target;

		public bool IsActive
		{
			get => _isActive;
			set
			{
				if (Target == null)
					return;
				
				ChangeProperty(ref _isActive, value);
			}
		}
		private bool _isActive;
	}
}