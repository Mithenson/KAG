namespace KAG.Unity.Network
{
	public sealed class Match
	{
		public readonly MatchKind Kind;
		public readonly NetworkSocket Socket;
		
		public Match(MatchKind kind, NetworkSocket socket)
		{
			Kind = kind;
			Socket = socket;
		}
	}
}