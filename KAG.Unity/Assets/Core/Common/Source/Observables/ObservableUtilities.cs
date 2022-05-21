using System;

namespace KAG.Unity.Common.Observables
{
	public static class ObservableUtilities 
	{
		public static bool TryChangeProperty<T>(ref T from, ref T to, string propertyName, out PropertyChangedEventArgs args)
		{
			if (Equals(from, to))
			{
				args = default;
				return false;
			}

			from = to;
			args = new PropertyChangedEventArgs(new PropertyIdentifier(propertyName), from, to);
			
			return true;
		}
	}
}