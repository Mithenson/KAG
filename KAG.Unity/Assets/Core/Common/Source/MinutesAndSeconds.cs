using System;

namespace KAG.Unity.Common
{
	public readonly struct MinutesAndSeconds
	{
		public static readonly MinutesAndSeconds Zero = new MinutesAndSeconds(0, 0);
		
		public readonly int Minutes;
		public readonly int Seconds;

		public static MinutesAndSeconds GetElapsedTimeSince(DateTime timestamp)
		{
			var elapsedTime = DateTime.UtcNow.Subtract(timestamp);
			return new MinutesAndSeconds(elapsedTime.Minutes, elapsedTime.Seconds);
		}

		public MinutesAndSeconds(int minutes, int seconds)
		{
			Minutes = minutes;
			Seconds = seconds;
		}

		public string Format()
		{
			var minutes = Minutes < 10 ? $"0{Minutes}" : Minutes.ToString();
			var seconds = Seconds < 10 ? $"0{Seconds}" : Seconds.ToString();
			
			return $"{minutes}<space=0.1em><size=75%><sprite name=\"Icon_PictoIcon_TimeColon_Alt\"></size><space=0.125em>{seconds}m";
		}
	}
}