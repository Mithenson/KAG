using KAG.Shared.Transform;
using KAG.Unity.Common.Utilities;
using KAG.Unity.Simulation;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public sealed class TransformPresentationBehaviour : MonoBehaviour
	{
		private PresentationBehaviour _presentation;

		[Inject]
		public void Inject(PresentationBehaviour presentation) =>
			_presentation = presentation;

		private void OnEnable()
		{
			var position = _presentation.Entity.GetComponent<PositionComponent>();
			transform.position = position.Value.ToUnity();
		}
	}
}