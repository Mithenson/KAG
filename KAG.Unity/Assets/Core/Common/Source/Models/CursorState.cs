using System;

namespace KAG.Unity.Scenes.Models
{
	[Flags]
	public enum CursorState
	{
		None = 0,
		
		InPanel = 1 << 0,
		InGame = 1 << 1,
		IsHovering = 1 << 2
	}
}