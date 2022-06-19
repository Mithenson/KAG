using System;
using KAG.Unity.UI.Models;
using UnityEngine;
using Zenject;

namespace KAG.Unity.UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public sealed class PanelBehaviour : MonoBehaviour
	{
		private UIModel _uiModel;
		private CanvasGroup _canvasGroup;
		
		private bool _isActive;
		private bool _isFullyShown;

		[Inject]
		public void Inject(UIModel uiModel) => 
			_uiModel = uiModel;

		private void Awake() => 
			_canvasGroup = GetComponent<CanvasGroup>();

		private void OnEnable()
		{
			_isActive = true;
			_uiModel.ActivePanelsCount++;
		}

		private void Update()
		{
			if (!_isActive)
				return;
			
			if (!_isFullyShown)
			{
				if (Math.Abs(_canvasGroup.alpha - 1.0f) < Mathf.Epsilon)
					_isFullyShown = true;
			}
			else
			{
				if (_canvasGroup.alpha < 1.0f)
					TurnOff();
			}
		}

		private void OnDisable() =>
			TurnOff();

		private void TurnOff()
		{
			if (!_isActive)
				return;
			
			_isActive = false;
			_isFullyShown = false;
			
			_uiModel.ActivePanelsCount--;
		}
	}
}