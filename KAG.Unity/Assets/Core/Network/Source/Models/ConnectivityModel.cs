using System;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using KAG.Shared.Network;
using KAG.Unity.Common.Observables;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Network.Models
{
	public sealed class ConnectivityModel : Observable, IFixedTickable, IDisposable
	{
		private const int _pingComputationCadence = 5;
		
		public bool GotDisconnected
		{
			get => _gotDisconnected;
			set => ChangeProperty(ref _gotDisconnected, value);
		}
		private bool _gotDisconnected;

		public int SecondsLeftUntilAutomaticExit
		{
			get => _secondsLeftUntilAutomaticExit;
			set => ChangeProperty(ref _secondsLeftUntilAutomaticExit, value);
		}
		private int _secondsLeftUntilAutomaticExit;

		public bool IsLeavingDueToDisconnection
		{
			get => _isLeavingDueToDisconnection;
			set => ChangeProperty(ref _isLeavingDueToDisconnection, value);
		}
		private bool _isLeavingDueToDisconnection;

		public int Ping
		{
			get => _ping;
			set => ChangeProperty(ref _ping, value);
		}
		private int _ping;

		private readonly UnityClient _client;

		private int _pingComputationCountdown;

		public ConnectivityModel(UnityClient client)
		{
			_pingComputationCountdown = 0;
			_client = client;

			client.MessageReceived += OnClientMessageReceived;
		}
		
		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			if (args.Tag != NetworkTags.PingComputation)
				return;

			using var message = args.GetMessage();
			if (!message.IsPingAcknowledgementMessage)
				return;
			
			Ping = Mathf.RoundToInt(_client.Client.RoundTripTime.SmoothedRtt / 2.0f);
		}

		void IFixedTickable.FixedTick()
		{
			if (_pingComputationCountdown > 0)
			{
				_pingComputationCountdown--;
				return;
			}
			
			using var writer = DarkRiftWriter.Create();
			writer.WriteRaw(Array.Empty<byte>(), 0, 0);

			using var message = Message.Create(NetworkTags.PingComputation, writer);
			
			message.MakePingMessage();
			_client.SendMessage(message, SendMode.Unreliable);

			_pingComputationCountdown = _pingComputationCadence;
		}

		void IDisposable.Dispose() =>
			_client.MessageReceived -= OnClientMessageReceived;
	}
}