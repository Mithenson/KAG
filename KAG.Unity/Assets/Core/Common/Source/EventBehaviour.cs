using UnityEngine;
using UnityEngine.Events;

namespace KAG.Unity.Common
{
	public sealed class EventBehaviour : MonoBehaviour
	{
		[SerializeField]
		private UnityEvent _event;

		public void Execute() =>
			_event.Invoke();
	}
}