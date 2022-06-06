using UnityEngine;

namespace KAG.Unity.Common
{
	public sealed class ParentingMarkerBehaviour : MonoBehaviour
	{
		public ParentingMarker Value =>
			_value;
		
		[SerializeField]
		private ParentingMarker _value;
	}
}