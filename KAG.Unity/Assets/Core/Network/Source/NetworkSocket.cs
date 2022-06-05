using System;
using System.Net;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;

namespace KAG.Unity.Network
{
	[Serializable]
	public sealed class NetworkSocket
	{
		#region Nested types

		private enum Mode
		{
			IPAddress,
			Host
		}

		#endregion

		public IPAddress Address
		{
			get
			{
				if (_cachedAddress != null)
					return _cachedAddress;

				if (!IPAddress.TryParse(_endPoint, out var address))
					throw new FormatException($"Could not parse `{nameof(_endPoint)}={_endPoint}` for `{nameof(_mode)}={_mode}` to an ip address.");

				_cachedAddress = address;
				return address;
			}
		}

		[SerializeField]
		private Mode _mode;

		[SerializeField]
		private string _endPoint;

		[SerializeField]
		private int _tcpPort;
	
		[SerializeField]
		private int _udpPort;

		private IPAddress _cachedAddress;

		public NetworkSocket(string host, int tcpPort, int udpPort)
		{
			_mode = Mode.Host;

			_endPoint = host;
			_tcpPort = tcpPort;
			_udpPort = udpPort;
		}
		public NetworkSocket(IPAddress address, int tcpPort, int udpPort)
		{
			_mode = Mode.Host;
		
			_tcpPort = tcpPort;
			_udpPort = udpPort;

			_cachedAddress = address;
		}
	
		public void ConnectInBackground(UnityClient client, DarkRiftClient.ConnectCompleteHandler callback)
		{
			if (_mode == Mode.Host)
				client.ConnectInBackground(_endPoint, _tcpPort, _udpPort, true, callback);
			else
				client.ConnectInBackground(Address, _tcpPort, _udpPort, true, callback);
		}
	}
}