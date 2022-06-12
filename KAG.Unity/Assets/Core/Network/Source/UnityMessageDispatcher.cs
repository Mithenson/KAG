using System.Collections.Generic;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared.Messages;

namespace KAG.Unity.Network
{
	public sealed class UnityMessageDispatcher : MessageDispatcher
	{
		public UnityMessageDispatcher(IEnumerable<IMessageHandler> handlers, UnityClient client) 
			: base(handlers) { }
		
		public void Dispatch(NetworkManager networkManager, object sender, MessageReceivedEventArgs args)
		{
			if (!TryGetHandler(args.Tag, out IUnityMessageHandler handler))
				return;
			
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			handler.Handle(networkManager, message, reader);
		}
	}
}