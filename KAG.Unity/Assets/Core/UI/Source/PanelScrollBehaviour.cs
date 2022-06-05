using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

namespace KAG.Unity.UI
{
	[ExecuteInEditMode, RequireComponent(typeof(Image))]
	public class PanelScrollBehaviour : MonoBehaviour
	{
		private static readonly int TexturePropertyIndex = Shader.PropertyToID("_MainTex");
		
		private RectTransform RectTransform => (RectTransform)transform;

		[SerializeField]
		private float _uniformTiling;
		
		[SerializeField]
		private Vector2 _speed;
		
		private Image _image;
		private bool _isFaulted;

		private void OnEnable()
		{
			_image = GetComponent<Image>();
			_isFaulted = _image == null;
		}

		private void Update()
		{
			if (_isFaulted)
				return;
			
			var material = _image.material;
			if (material == null)
				return;
			
			var ratio = RectTransform.rect.height / RectTransform.rect.width;
			material.SetTextureScale(TexturePropertyIndex, new Vector2(_uniformTiling, _uniformTiling * ratio));
			material.SetTextureOffset(TexturePropertyIndex, _speed * Time.time);
		}
	}
}