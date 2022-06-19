using KAG.Unity.UI.ViewModels;
using UnityEngine;

namespace KAG.Unity.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class InteractivityBehaviour : MonoBehaviour
	{
		[SerializeField, Range(0.0f, 1.0f)]
		private float _offOpacity = 0.625f;

		private bool _hasCanvasGroup;
		private CanvasGroup _canvasGroup;

		public void Set(bool value)
		{
			if (!_hasCanvasGroup)
			{
				_canvasGroup = GetComponent<CanvasGroup>();
				_hasCanvasGroup = true;
			}
			
			if (value)
			{
				_canvasGroup.blocksRaycasts = true;
				_canvasGroup.alpha = 1.0f;
			}
			else
			{
				_canvasGroup.blocksRaycasts = false;
				_canvasGroup.alpha = _offOpacity;
			}
		}
	}
}