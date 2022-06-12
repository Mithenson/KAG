using System;
using KAG.Shared.Events;

namespace KAG.Unity.Common.Observables
{
	public static class EventHubExtensions
	{
		public static void Define<TArgs>(this EventHub eventHub, EventKey evtKey) where TArgs : EventArgs =>
			eventHub.Define<TArgs>((ushort)evtKey);

		public static bool Remove(this EventHub eventHub, EventKey evtKey) => 
			eventHub.Remove((ushort)evtKey);

		public static void Subscribe<TArgs>(this EventHub eventHub, EventKey evtKey, Action<object, TArgs> del) where TArgs : EventArgs =>
			eventHub.Subscribe((ushort)evtKey, del);
		
		public static void Unsubscribe<TArgs>(this EventHub eventHub, EventKey evtKey, Action<object, TArgs> del) where TArgs : EventArgs =>
			eventHub.Unsubscribe((ushort)evtKey, del);
		
		public static void Invoke<TArgs>(this EventHub eventHub, EventKey evtKey, object sender, TArgs args) where TArgs : EventArgs =>
			eventHub.Invoke((ushort)evtKey, sender, args);
	}
}