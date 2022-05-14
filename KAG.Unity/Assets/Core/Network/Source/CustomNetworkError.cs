namespace KAG.Unity.Network
{
	public sealed class CustomNetworkError : NetworkError
	{
		protected override string Name => _name;
	
		private string _name;
	
		public CustomNetworkError(string step, string name) : base(step) => 
			_name = name;
	}
}