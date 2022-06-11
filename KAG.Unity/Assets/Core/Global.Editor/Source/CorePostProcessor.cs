using System.IO;
using System.Linq;
using KAG.Unity.Common;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace KAG.Unity.Global.Editor
{
	public sealed class CorePostProcessor : AssetPostprocessor
	{
		private const string CommonAddressableGroupName = "Common";
		
		private static void OnPostprocessAllAssets(string[] importedAssetPaths, string[] deletedAssetPaths, string[] movedAssetPaths, string[] movedFromAssetPaths)
		{
			if (AddressableAssetSettingsDefaultObject.Settings == null)
			{
				Debug.LogWarning("Asset post processing can't be done without being able to access the addressables.");
				return;
			}
			
			var commonGroup = AddressableAssetSettingsDefaultObject.Settings.FindGroup(CommonAddressableGroupName);
			var needAdditionalSave = false;
			
			foreach (var importedAssetPath in importedAssetPaths)
			{
				var guid = AssetDatabase.GUIDFromAssetPath(importedAssetPath);
				var labels = AssetDatabase.GetLabels(guid);

				if (labels.Contains(EditorConstants.SharedDataLabel))
					CopySharedData(importedAssetPath);
				
				if (Path.GetExtension(importedAssetPath) == EditorConstants.PrototypeDefinitionExtension)
					EnsureAddressableEntry(commonGroup, guid, importedAssetPath, Constants.Addressables.PrototypeDefinitionLabel, ref needAdditionalSave);
				
				if (labels.Contains(EditorConstants.PresentationLinkerLabel))
					EnsureAddressableEntry(commonGroup, guid, importedAssetPath, Constants.Addressables.PresentationLinkerLabel, ref needAdditionalSave);
			}

			if (!needAdditionalSave)
				return;
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private static void CopySharedData(string path)
		{
			var name = Path.GetFileName(path);
			var destination = @$"{EditorConstants.DarkRiftDataFolder}\{name}";
				
			File.Copy(path, destination, true);
		}

		private static void EnsureAddressableEntry(AddressableAssetGroup commonGroup, GUID guid, string path, string label, ref bool needAdditionalSave)
		{
			var guidAsString = guid.ToString();
			var existingEntry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(guidAsString);
			
			if (existingEntry != null)
			{
				if (string.IsNullOrEmpty(label) || existingEntry.labels.Contains(label))
					return;

				existingEntry.SetLabel(label, true, true);
				needAdditionalSave = true;
			}
			else
			{
				var entry = AddressableAssetSettingsDefaultObject.Settings.CreateOrMoveEntry(guid.ToString(), commonGroup);
				entry.address = Path.GetFileName(path);
				
				if (!string.IsNullOrEmpty(label))
					entry.SetLabel(label, true, true);
						
				needAdditionalSave = true;
			}
		}
	}
}