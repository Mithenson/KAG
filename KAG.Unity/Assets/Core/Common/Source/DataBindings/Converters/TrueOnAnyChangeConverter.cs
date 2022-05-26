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

		public override bool ConvertExplicitly(object input)
		{
			if (!_hasBeenCalledOnce)
			{
				_hasBeenCalledOnce = true;
				return _countFirst;
			}

			return true;
		}
	}
}