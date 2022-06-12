using System.Collections.Generic;
using DarkRift.Server;
using KAG.Shared.Messages;

namespace KAG.Server.Network
{
	public sealed class ServerMessageDispatcher : MessageDispatcher
	{
		public ServerMessageDispatcher(IEnumerable<IMessageHandler> handlers) : base(handlers) { }

		public void Dispatch(object sender, MessageReceivedEventArgs args)
		{
			if (!TryGetHandler<IServerMessageHandler>(args.Tag, out var handler))
				return;
			
			using var message = args.GetMessage();
			using var reader = message.GetReader();

			handler.Handle(args.Client, message, reader);
		}
	}
}