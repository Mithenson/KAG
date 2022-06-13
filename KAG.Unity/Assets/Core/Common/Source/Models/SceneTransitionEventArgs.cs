using System;

namespace KAG.Unity.Common.Models
{
	public sealed class SceneTransitionEventArgs : EventArgs
	{
		public readonly GameStatus Destination;
		
		public SceneTransitionEventArgs(GameStatus destination) =>
			Destination = destination;
	}
}