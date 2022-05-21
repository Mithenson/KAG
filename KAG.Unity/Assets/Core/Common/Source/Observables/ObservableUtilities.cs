namespace KAG.Unity.Common.Observables
{
	public static class ObservableUtilities 
	{
		public static bool TryChangeProperty<T>(ref T property, ref T value, string propertyName, out PropertyChangedEventArgs args)
		{
			if (property.Equals(value))
			{
				args = default;
				return false;
			}
			
			args = new PropertyChangedEventArgs(new PropertyIdentifier(propertyName), property, value);
			return true;
		}
	}
}