using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using DarkRift;
using DarkRift.Server;
using KAG.Server.DependencyInjection;
using KAG.Server.Network;
using KAG.Shared;
using KAG.Shared.Network;
using KAG.Shared.Prototype;
using KAG.Shared.Transform;

namespace KAG.Server
{
	public sealed class CorePlugin : Plugin
	{
		private const int MaxPlayers = 12;
		
		public override Version Version => 
			new Version(1, 0, 0);
		
		public override bool ThreadSafe => 
			false;
		
		public IReadOnlyDictionary<IClient, Player> ConnectedPlayers => 
			_connectedPlayers;

		private readonly HashSet<IClient> _clientsOnStandby;
		private readonly Dictionary<IClient, Player> _connectedPlayers;
		private readonly IMultiplayerSDKProxy _multiplayerSdkProxy;
		private readonly IContainer _container;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ServerMessageDispatcher _messageDispatcher;
		private readonly World _world;
		
		public CorePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
		{
			_clientsOnStandby = new HashSet<IClient>();
			_connectedPlayers = new Dictionary<IClient, Player>();

			_multiplayerSdkProxy = CreateMultiplayerSDKProxy();

			if (!TryCreateDependencyInjectionContainer(out _container))
				return;
			
			_lifetimeScope = _container.BeginLifetimeScope();
			_messageDispatcher = _lifetimeScope.Resolve<ServerMessageDispatcher>();
			_world = _lifetimeScope.Resolve<World>();

			ClientManager.ClientConnected += OnClientConnected;
			ClientManager.ClientDisconnected += OnClientDisconnected;
		}

		private IMultiplayerSDKProxy CreateMultiplayerSDKProxy()
		{
			IMultiplayerSDKProxy instance;

			try
			{
				instance = new PlayfabMultiplayerSDKProxy();
			}
			catch
			{
				Logger.Log("Playfab multiplayer sdk is unavailable. Switching to mockup.", LogType.Info);
				instance = new MockupMultiplayerSDKProxy();
			}

			return instance;
		}

		private bool TryCreateDependencyInjectionContainer(out IContainer container)
		{
			var builder = new ContainerBuilder();

			try
			{
				builder.RegisterInstance(this).SingleInstance();
				builder.RegisterInstance(Logger).SingleInstance();
				
				NetworkInstaller.Install(builder);
				SimulationInstaller.Install(builder, ResourceDirectory);
			}
			catch (Exception exception)
			{
				Logger.Log("An unexpected exception occured during the registration of dependencies.", LogType.Error, exception);

				container = default;
				return false;
			}

			container = builder.Build();
			return true;
		}

		private void OnClientConnected(object sender, ClientConnectedEventArgs args)
		{
			_clientsOnStandby.Add(args.Client);
			args.Client.MessageReceived += OnClientMessageReceived;
		}

		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			switch (args.Tag)
			{
				case NetworkTags.PlayerIdentification:
					OnPlayerIdentification(sender, args);
					return;
				
				case NetworkTags.PingComputation:
					OnPingComputation(sender, args);
					return;
			}
			
			_messageDispatcher.Dispatch(sender, args);
		}

		private void OnPlayerIdentification(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			using var reader = message.GetReader();
			
			var identificationMessage = reader.ReadSerializable<PlayerIdentificationMessage>();
							
			// Create player
			var playerEntity = _world.CreateEntity(Identity.Player);
			var component = playerEntity.GetComponent<PlayerComponent>();
			var position = playerEntity.GetComponent<PositionComponent>();

			component.Id = args.Client.ID;
			component.Name = identificationMessage.Name;
			
			var random = new Random();
			position.Value = new Vector2(random.Next(-20, 20), random.Next(-20, 20));

			var player = new Player(args.Client, identificationMessage.Name, playerEntity);
			_connectedPlayers.Add(args.Client, player);

			SendPlayerArrivalMessage(player);
			SendPlayerCatchupMessage(player);
			
			_multiplayerSdkProxy.UpdateConnectedPlayers(_connectedPlayers.Values);
		}
		private void SendPlayerCatchupMessage(Player player)
		{
			using var writer = DarkRiftWriter.Create();
			foreach (var entity in _world.Entities)
				writer.Write(entity);

			using var message = Message.Create(NetworkTags.PlayerCatchup, writer);
			player.Client.SendMessage(message, SendMode.Reliable);
		}
		private void SendPlayerArrivalMessage(Player joiningPlayer)
		{
			using var writer = DarkRiftWriter.Create();
			writer.Write(joiningPlayer.Entity);

			using var message = Message.Create(NetworkTags.PlayerArrival, writer);
			foreach (var client in ClientManager.GetAllClients().Where(client => client != joiningPlayer.Client))
				client.SendMessage(message, SendMode.Reliable);
		}

		private void OnPingComputation(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			if (!message.IsPingMessage)
				return;

			using var writer = DarkRiftWriter.Create();
			writer.WriteRaw(Array.Empty<byte>(), 0, 0);
			
			using var response = Message.Create(NetworkTags.PingComputation, writer);

			response.MakePingAcknowledgementMessage(message);
			args.Client.SendMessage(response, SendMode.Unreliable);
		}
	
		private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs args)
		{
			if (!_connectedPlayers.TryGetValue(args.Client, out var player))
				return;
			
			_world.Destroy(player.Entity);

			using var writer = DarkRiftWriter.Create();
			writer.Write(new PlayerDepartureMessage()
			{
				Id = args.Client.ID
			});

			using var message = Message.Create(NetworkTags.PlayerDeparture, writer);
			foreach (var client in ClientManager.GetAllClients())
				client.SendMessage(message, SendMode.Reliable);

			_connectedPlayers.Remove(args.Client);
			_multiplayerSdkProxy.UpdateConnectedPlayers(_connectedPlayers.Values);
		}
		
		protected override void Dispose(bool disposing)
		{
			_lifetimeScope.Dispose();
			base.Dispose(disposing);
		}
	}
}