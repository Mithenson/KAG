using System;
using System.Threading;
using System.Threading.Tasks;

namespace KAG.Unity.Network
{
	public interface IMatchProvider
	{
		event Action<string> OnProgress;

		Task<Match> GetMatch(string clientName, CancellationToken cancellationToken);
	}
}