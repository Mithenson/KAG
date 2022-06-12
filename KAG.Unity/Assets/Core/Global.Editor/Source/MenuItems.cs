using System;
using System.Linq;
using PlayFab;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KAG.Unity.Global.Editor
{
	public static class MenuItems
	{
		[MenuItem("KAG/Temporary/Test functionality")]
		public static void Test()
		{
			var code = PlayFabErrorCode.AccountAlreadyLinked.ToString();

			for (var i = 1; i < code.Length; i++)
			{
				var character = code[i];
				if (!char.IsUpper(character))
					continue;

				code = code.Remove(i, 1).Insert(i, $" {char.ToLower(character)}");
			}
		
			Debug.Log(code);
		}

		[MenuItem("KAG/Build")]
		public static void Build()
		{
			AddressableAssetSettings.BuildPlayerContent();
			var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions()
			{
				locationPathName = "Build/KAG.exe",
				scenes = EditorBuildSettings.scenes.Select(item => item.path).ToArray(),
				target = BuildTarget.StandaloneWindows,
				options = BuildOptions.Development | BuildOptions.ShowBuiltPlayer
			});

			var summary = report.summary;
			switch (summary.result)
			{
				case BuildResult.Succeeded:
					Debug.Log($"Build succeeded with `{nameof(summary.totalSize)}={summary.totalSize} bytes`.");
					return;

				case BuildResult.Cancelled:
					Debug.LogWarning("Build was cancelled.");
					return;
			}
			
			Debug.LogError($"Build failed with `{nameof(summary.result)}={summary.result}`.");
		}
	}
}