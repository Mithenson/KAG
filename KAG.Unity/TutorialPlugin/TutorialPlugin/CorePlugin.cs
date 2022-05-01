﻿using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using DarkRift.Server;
using Microsoft.Playfab.Gaming.GSDK.CSharp;

namespace TutorialPlugin
{
	public class CorePlugin : Plugin
	{
		public override Version Version => new Version(1, 0, 0);
		public override bool ThreadSafe => false;

		private Dictionary<IClient, (Player Value, PlayerState State)> _connectedPlayers;

		public CorePlugin(PluginLoadData loadData) : base(loadData)
		{
			_connectedPlayers = new Dictionary<IClient, (Player Value, PlayerState State)>();
			
			ClientManager.ClientConnected += OnClientConnected;
			ClientManager.ClientDisconnected += OnClientDisconnected;
			
			GameserverSDK.Start();
			if (GameserverSDK.ReadyForPlayers())
			{
				
			}
			else
			{
				
			}
		}
		
		private void OnClientConnected(object sender, ClientConnectedEventArgs args)
		{
			var player = new Player(args.Client.ID, "John Doe");
			_connectedPlayers.Add(args.Client, (player, PlayerState.Default));

			using (var newPlayerWriter = DarkRiftWriter.Create())
			{
				newPlayerWriter.Write(player);
				using (var message = Message.Create(Tags.PlayerConnection, newPlayerWriter))
				{
					foreach (var client in ClientManager.GetAllClients().Where(candidate => candidate != args.Client))
						client.SendMessage(message, SendMode.Reliable);
				}
			}

			foreach (var alreadyConnectedPlayer in _connectedPlayers.Values)
			{
				using (var existingPlayerWriter = DarkRiftWriter.Create())
				{
					existingPlayerWriter.Write(alreadyConnectedPlayer.Value);
					existingPlayerWriter.Write(alreadyConnectedPlayer.State.X);
					existingPlayerWriter.Write(alreadyConnectedPlayer.State.Y);
					
					using (var message = Message.Create(Tags.PlayerConnection, existingPlayerWriter))
						args.Client.SendMessage(message, SendMode.Reliable);
				}
			}
			
			args.Client.MessageReceived += OnClientMessageReceived;
		}

		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			using (var message = args.GetMessage())
			{
				switch (message.Tag)
				{
					case Tags.PlayerDataUpdate:
						OnPlayerDataUpdated(args.Client, message);
						break;
					
					case Tags.PlayerReady:
						OnPlayerReady(args.Client, message);
						break;
					
					case Tags.PlayerMove:
						OnPlayerMoved(args.Client, message);
						break;
				}
			}
		}

		private void OnPlayerDataUpdated(IClient sender, Message message)
		{
			using (var reader = message.GetReader())
			{
				var tuple = _connectedPlayers[sender];
				
				var updatedPlayerData = reader.ReadSerializable<Player>();
				tuple.Value = updatedPlayerData;

				tuple.State.X = reader.ReadSingle();
				tuple.State.Y = reader.ReadSingle();
				
				_connectedPlayers[sender] = tuple;

				foreach (var client in ClientManager.GetAllClients())
					client.SendMessage(message, SendMode.Reliable);
			}
		}

		private void OnPlayerReady(IClient sender, Message message)
		{
			using (var reader = message.GetReader())
			{
				var data = reader.ReadSerializable<PlayerReady>();
				
				var tuple = _connectedPlayers[sender];
				tuple.State.IsReady = data.Value;
				_connectedPlayers[sender] = tuple;
			}

			if (!_connectedPlayers.Values.All(player => player.State.IsReady))
				return;

			using (var writer = DarkRiftWriter.Create())
			{
				using (var gameStartMessage = Message.Create(Tags.GameStart, writer))
				{
					foreach (var client in ClientManager.GetAllClients())
						client.SendMessage(gameStartMessage, SendMode.Reliable);
				}
			}
		}

		private void OnPlayerMoved(IClient sender, Message message)
		{
			using (var reader = message.GetReader())
			{
				var tuple = _connectedPlayers[sender];
				tuple.State.X = reader.ReadSingle();
				tuple.State.Y = reader.ReadSingle();
				_connectedPlayers[sender] = tuple;
				
				foreach (var client in ClientManager.GetAllClients())
					client.SendMessage(message, SendMode.Unreliable);
			}
		}

		private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
		{
			_connectedPlayers.Remove(args.Client);

			using (var writer = DarkRiftWriter.Create())
			{
				writer.Write(args.Client.ID);
				using (var message = Message.Create(Tags.PlayerDisconnection, writer))
				{
					foreach (var client in ClientManager.GetAllClients())
						client.SendMessage(message, SendMode.Reliable);
				}
			}
		}
	}
}