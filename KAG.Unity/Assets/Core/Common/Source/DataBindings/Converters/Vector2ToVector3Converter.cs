using System;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class Vector2ToVector3Converter : DataBindingConverter<Vector2, Vector3>
	{
		public override bool TryConvertExplicitly(Vector2 input, out Vector3 output)
		{
			output = input;
			return true;
		}
	}
}