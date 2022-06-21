using System;
using KAG.Unity.Scenes.Models;
using UnityEngine;

namespace KAG.Unity.Scenes
{
	[Serializable]
	public sealed class CursorVisual
	{
		public CursorState AssociatedState => 
			_associatedState;

		public Sprite Sprite =>
			_sprite;

		public Vector2 Offset => 
			_offset;
			
		[SerializeField]
		private CursorState _associatedState;
			
		[SerializeField]
		private Sprite _sprite;

		[SerializeField]
		private Vector2 _offset;

		public CursorVisual(CursorState associatedState, Sprite sprite, Vector2 offset)
		{
			_associatedState = associatedState;
			_sprite = sprite;
			_offset = offset;
		}
	}
}