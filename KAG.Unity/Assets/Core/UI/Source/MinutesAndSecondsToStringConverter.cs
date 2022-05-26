using System;
using KAG.Unity.Common;
using KAG.Unity.Common.DataBindings;
using UnityEngine;

namespace KAG.Unity.UI
{
	[Serializable]
	public sealed class MinutesAndSecondsToStringConverter : DataBindingConverter<MinutesAndSeconds, string>
	{
		[SerializeField]
		private float _leftSpacing = 0.1f;

		[SerializeField]
		private float _rightSpacing = 0.125f;

		[SerializeField, Range(0, 100)]
		private int _separatorIconSize = 75;
		
		public override string ConvertExplicitly(MinutesAndSeconds input)
		{
			var minutes = input.Minutes < 10 ? $"0{input.Minutes}" : input.Minutes.ToString();
			var seconds = input.Seconds < 10 ? $"0{input.Seconds}" : input.Seconds.ToString();
			
			return $"{minutes}<space={_leftSpacing}em><size={_separatorIconSize}%><sprite name=\"Icon_PictoIcon_TimeColon_Alt\"></size><space={_rightSpacing}em>{seconds}m";	
		}
	}
}