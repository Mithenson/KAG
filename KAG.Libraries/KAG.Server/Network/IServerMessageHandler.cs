using DarkRift;
using DarkRift.Server;
using KAG.Shared.Messages;

namespace KAG.Server.Network
{
	public interface IServerMessageHandler : IMessageHandler
	{
		void Handle(IClient client, Message message, DarkRiftReader reader);
	}
}