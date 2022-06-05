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
	}
}