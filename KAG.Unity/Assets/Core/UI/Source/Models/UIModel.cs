using System.Collections.Generic;
using KAG.Unity.Common;
using KAG.Unity.Common.Observables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace KAG.Unity.UI.Models
{
	public class UIModel : Observable, ITickable
	{
		public bool IsHoveringAnyElement
		{
			get => _isHoveringAnyElement;
			set => ChangeProperty(ref _isHoveringAnyElement, value);
		}
		private bool _isHoveringAnyElement;
		
		public ushort ActivePanelsCount
		{
			get => _activePanelsCount;
			set => ChangeProperty(ref _activePanelsCount, value);
		}
		private ushort _activePanelsCount;
		
		private readonly InputAction _lookAction;
		private readonly List<RaycastResult> _hoverBuffer;

		public UIModel([Inject(Id = UnityConstants.Inputs.LookAction)] InputAction lookAction)
		{
			_lookAction = lookAction;

			_hoverBuffer = new List<RaycastResult>();
		}

		void ITickable.Tick()
		{
			var evt = new PointerEventData(EventSystem.current);
			evt.position = _lookAction.ReadValue<Vector2>();
			EventSystem.current.RaycastAll(evt, _hoverBuffer);

			if (_hoverBuffer.Count == 0)
			{
				IsHoveringAnyElement = false;
				return;
			}
			
			var current = _hoverBuffer[0].gameObject.transform;
			while (current != null)
			{
				if (current.TryGetComponent<InteractableElementBehaviour>(out _))
				{
					IsHoveringAnyElement = true;
					return;
				}

				current = current.parent;
			}
			
			IsHoveringAnyElement = false;
		}
	}
}