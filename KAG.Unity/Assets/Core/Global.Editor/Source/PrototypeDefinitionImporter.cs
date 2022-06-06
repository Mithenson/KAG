using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KAG.Unity.Global.Editor
{
	[ScriptedImporter(1, "proto")]
	public sealed class PrototypeDefinitionImporter : ScriptedImporter
	{
		public override void OnImportAsset(AssetImportContext ctx)
		{
			var json = File.ReadAllText(ctx.assetPath);
			var textAsset = new TextAsset(json);
			
			ctx.AddObjectToAsset("KAG.Unity.PrototypeDefinition", textAsset);
			ctx.SetMainObject(textAsset);
		}
	}
}