using System;

namespace KAG.Shared.Events
{
	public static class EventHubExtensions
	{
		public static void Define<TArgs>(this EventHub eventHub, SharedEventKey evtKey) where TArgs : EventArgs =>
			eventHub.Define<TArgs>((ushort)evtKey);

		public static bool Remove(this EventHub eventHub, SharedEventKey evtKey) => 
			eventHub.Remove((ushort)evtKey);

		public static void Subscribe<TArgs>(this EventHub eventHub, SharedEventKey evtKey, Action<object, TArgs> del) where TArgs : EventArgs =>
			eventHub.Subscribe((ushort)evtKey, del);
		
		public static void Unsubscribe<TArgs>(this EventHub eventHub, SharedEventKey evtKey, Action<object, TArgs> del) where TArgs : EventArgs =>
			eventHub.Unsubscribe((ushort)evtKey, del);
		
		public static void Invoke<TArgs>(this EventHub eventHub, SharedEventKey evtKey, object sender, TArgs args) where TArgs : EventArgs =>
			eventHub.Invoke((ushort)evtKey, sender, args);
	}
}