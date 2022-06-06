using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace KAG.Unity.Common
{
	public sealed class EventBehaviour : MonoBehaviour
	{
		[SerializeField]
		private Delay _delay;
		
		[SerializeField]
		private UnityEvent _event;

		public void Execute()
		{
			if (!_delay.IsActive)
			{
				_event.Invoke();
				return;
			}

			StartCoroutine(DelayedCall());
		}

		private IEnumerator DelayedCall()
		{
			yield return _delay.Wait();
			_event.Invoke();
		}
	}
}