using System;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public sealed class TrueOnAnyChangeConverter : DataBindingConverter<object, bool>
	{
		[SerializeField]
		private bool _countFirst;

		private bool _hasBeenCalledOnce;

		public override bool TryConvertExplicitly(object input, out bool output)
		{
			if (!_hasBeenCalledOnce)
			{
				_hasBeenCalledOnce = true;
				output = _countFirst;
				
				return true;
			}

			output = true;
			return true;
		}
	}
}