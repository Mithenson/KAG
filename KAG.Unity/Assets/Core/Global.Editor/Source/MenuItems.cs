using PlayFab;
using UnityEditor;
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
	}
}