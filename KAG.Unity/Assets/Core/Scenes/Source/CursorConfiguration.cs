using KAG.Unity.Scenes.Models;
using UnityEngine;

namespace KAG.Unity.Scenes
{
	[CreateAssetMenu(menuName = "KAG/Configurations/Cursor", fileName = nameof(CursorConfiguration))]
	public sealed class CursorConfiguration : ScriptableObject
	{
		[SerializeField]
		private CursorVisual _defaultVisual;

		[SerializeField]
		private CursorVisual[] _visuals;

		public CursorVisual GetMatchingVisual(CursorState state)
		{
			for (var i = 0; i < _visuals.Length; i++)
			{
				if (_visuals[i].AssociatedState == state)
					return _visuals[i];
			}

			return _defaultVisual;
		}
	}
}