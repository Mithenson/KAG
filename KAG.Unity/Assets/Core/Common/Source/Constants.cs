using KAG.Unity.Common.Observables;
using UnityEngine;

namespace KAG.Unity.Common
{
	public static class Constants
	{
		public const string RootAssemblyName = "KAG";

		public static class Addressables
		{
			public const string PrototypeDefinitionLabel = "prototype_definition";
			public const string PresentationLinkerLabel = "presentation_linker";
		}

		public static class Scenes
		{
			public const int LobbySceneIndex = 1;
			public const int GameSceneIndex = 2;
		}

		public static class Inputs
		{
			public const string GameplayMap = "Gameplay";

			public const string MoveAction = "Gameplay/Move";
		}

		public static class Network
		{
			public readonly static (int Treshold, Color Color)[] PingData = new[]
			{
				(50, new Color(0.45f, 1f, 0.55f)),
				(100, new Color(1f, 0.85f, 0.45f)),
				(150, new Color(1f, 0.625f, 0.375f)),
				(200, new Color(1f, 0.415f, 0.415f)),
			};
		}
	}
}