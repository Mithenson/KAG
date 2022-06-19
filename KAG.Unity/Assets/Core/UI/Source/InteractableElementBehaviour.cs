using KAG.Unity.UI.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace KAG.Unity.UI
{
	public sealed class InteractableElementBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private UIModel _uiModel;

		[Inject]
		public void Inject(UIModel uiViewModel) => 
			_uiModel = uiViewModel;
		
		public void OnPointerEnter(PointerEventData eventData) =>
			_uiModel.IsHoveringAnyElement = true;
		
		public void OnPointerExit(PointerEventData eventData) => 
			_uiModel.IsHoveringAnyElement = false;
	}
}