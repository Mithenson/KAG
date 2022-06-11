using KAG.Shared;
using KAG.Unity.Simulation;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Gameplay
{
	public abstract class GameplayBehaviour : MonoBehaviour
	{
		protected Entity Entity => _presentation.Entity;
		
		protected PresentationBehaviour _presentation;

		[Inject]
		public void Inject(PresentationBehaviour presentation) => 
			_presentation = presentation;
	}
}