using KAG.Shared;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KAG.Unity.Simulation
{
	public sealed class PresentationBehaviour : MonoBehaviour
	{
		[ShowInInspector, ReadOnly]
		public Entity Entity;
	}
}