using System.IO;
using System.Linq;
using KAG.Unity.Common;
using UnityEditor;
using UnityEditor.AddressableAssets;

namespace KAG.Unity.Global.Editor
{
	public sealed class SharedDataPostProcessor : AssetPostprocessor
	{
		private const string DarkRiftDataFolder = @".\..\KAG.DarkRift\Data\CorePlugin";
		private const string SharedDataLabel = "SharedData";
		private const string PrototypeDefinitionExtension = ".proto";
		private const string PrototypeDefinitionLabel = "prototype_defintion";
		
		private static void OnPostprocessAllAssets(string[] importedAssetPaths, string[] deletedAssetPaths, string[] movedAssetPaths, string[] movedFromAssetPaths)
		{
			var commonGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Common");
			var needAdditionalSave = false;
			
			foreach (var importedAssetPath in importedAssetPaths)
			{
				var guid = AssetDatabase.GUIDFromAssetPath(importedAssetPath);
				var labels = AssetDatabase.GetLabels(guid);

				if (labels.Contains(SharedDataLabel))
				{
					var name = Path.GetFileName(importedAssetPath);
					var destination = @$"{DarkRiftDataFolder}\{name}";
				
					File.Copy(importedAssetPath, destination, true);
				}
				
				var extension = Path.GetExtension(importedAssetPath);
				if (extension == PrototypeDefinitionExtension)
				{
					var guidAsString = guid.ToString();
					var existingEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guidAsString);

					if (existingEntry != null)
					{
						if (!existingEntry.labels.Contains(Constants.Addressables.PrototypeDefinitionLabel))
						{
							existingEntry.SetLabel(Constants.Addressables.PrototypeDefinitionLabel, true, true);
							needAdditionalSave = true;
						}
					}
					else
					{
						var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid.ToString(), commonGroup);

						entry.address = Path.GetFileName(importedAssetPath);
						entry.SetLabel(Constants.Addressables.PrototypeDefinitionLabel, true, true);
						
						needAdditionalSave = true;
					}
				}
			}

			if (!needAdditionalSave)
				return;
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}
}