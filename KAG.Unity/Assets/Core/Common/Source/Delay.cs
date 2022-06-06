using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace KAG.Unity.Common
{
	[Serializable, InlineProperty]
	public struct Delay
	{
		#region Nested types

		private enum Mode
		{
			None,
			Frames,
			Seconds,
			FixedUpdates,
		}

		#endregion

		public bool IsActive => 
			_mode != Mode.None;
		
		[SerializeField]
		[HorizontalGroup("Main", MinWidth = 100.0f)]
		[HideLabel]
		private Mode _mode;

		#if UNITY_EDITOR
		[CustomValueDrawer(nameof(DrawValue))]
		#endif
		[SerializeField]
		[HorizontalGroup("Main")]
		[HideIf("_mode", Mode.None)]
		private float _value;

		public IEnumerator Wait()
		{
			switch (_mode)
			{
				case Mode.None:
					yield break;

				case Mode.Frames:
				{
					var count = Mathf.RoundToInt(_value);
					for (var i = 0; i < count; i++)
						yield return new WaitForEndOfFrame();
					
					yield break;
				}

				case Mode.Seconds:
					yield return new WaitForSeconds(_value);
					yield break;

				case Mode.FixedUpdates:
				{
					var count = Mathf.RoundToInt(_value);
					for (var i = 0; i < count; i++)
						yield return new WaitForFixedUpdate();
					
					yield break;
				}
			}
		}

		#if UNITY_EDITOR

		private float DrawValue()
		{
			if (_mode == Mode.Seconds)
				return EditorGUILayout.FloatField(GUIContent.none, _value);
			else
				return EditorGUILayout.IntField(GUIContent.none, Mathf.RoundToInt(_value));
		}
		
		#endif
	}
}