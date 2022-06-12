using System.Collections.Generic;
using DarkRift;

namespace KAG.Shared.Messages
{
	public abstract class MessageDispatcher
	{
		private Dictionary<ushort, IMessageHandler> _handlers;

		public MessageDispatcher(IEnumerable<IMessageHandler> handlers)
		{
			_handlers = new Dictionary<ushort, IMessageHandler>();
			foreach (var handler in handlers)
				_handlers.Add(handler.Tag, handler);
		}
		
		protected bool TryGetHandler<THandler>(ushort tag, out THandler handler)
			where THandler : IMessageHandler
		{
			handler = default;
			
			if (!_handlers.TryGetValue(tag, out var rawHandler)
				|| !(rawHandler is THandler castedHandler))
				return false;
			
			handler = castedHandler;
			return true;
		}
	}
}