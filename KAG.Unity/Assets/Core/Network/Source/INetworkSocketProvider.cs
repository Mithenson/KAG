using System.Threading;
using System.Threading.Tasks;

namespace KAG.Unity.Network
{
	public interface INetworkSocketProvider
	{
		Task<NetworkSocket> GetSocket(string clientName, CancellationToken cancellationToken);
	}
}