using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KAG.Unity.Global.Editor
{
	public sealed class SharedDataPostProcessor : AssetPostprocessor
	{
		private const string DarkRiftDataFolder = @".\..\KAG.DarkRift\Data\CorePlugin";
		private const string SharedDataLabel = "SharedData";
		
		private static void OnPostprocessAllAssets(string[] importedAssetPaths, string[] deletedAssetPaths, string[] movedAssetPaths, string[] movedFromAssetPaths)
		{
			var count = 0;
			foreach (var importedAssetPath in importedAssetPaths)
			{
				var guid = AssetDatabase.GUIDFromAssetPath(importedAssetPath);
				var labels = AssetDatabase.GetLabels(guid);

				if (!labels.Contains(SharedDataLabel))
					continue;

				var name = Path.GetFileName(importedAssetPath);
				var destination = @$"{DarkRiftDataFolder}\{name}";
				
				File.Copy(importedAssetPath, destination, true);
				count++;
			}

			if (count == 0)
				return;

			if(!Directory.Exists(Application.streamingAssetsPath))
				Directory.CreateDirectory(Application.streamingAssetsPath);
				
			BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.UncompressedAssetBundle, EditorUserBuildSettings.activeBuildTarget);
		}
	}
}