using DarkRift;
using KAG.Shared.Messages;

namespace KAG.Unity.Network
{
	public interface IUnityMessageHandler : IMessageHandler
	{
		void Handle(NetworkManager networkManager, Message message, DarkRiftReader reader);
	}
}