using UnityEngine;

namespace KAG.Unity.UI
{
	[ExecuteInEditMode, RequireComponent(typeof(ParticleSystem))]
	public class ParticleSystemShapeScalingByCanvasBehaviour : MonoBehaviour
	{
		[SerializeField]
		private float _ratio = 1.0f;
		
		private Canvas _canvas;
		private ParticleSystem _particleSystem;
		private bool _isFaulted;
		
		private void OnEnable()
		{
			_canvas = GetComponentInParent<Canvas>();
			_particleSystem = GetComponent<ParticleSystem>();

			_isFaulted = _canvas == null || _particleSystem == null;
		}

		private void Update()
		{
			if (_isFaulted)
				return;

			var rectTransform = (RectTransform)_canvas.transform;
			var shapeModule = _particleSystem.shape;
			
			shapeModule.scale = new Vector3(rectTransform.rect.width * _ratio, rectTransform.rect.height * _ratio, shapeModule.scale.z);
		}
	}
}