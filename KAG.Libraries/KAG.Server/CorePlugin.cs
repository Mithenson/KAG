using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using DarkRift;
using DarkRift.Server;
using KAG.Server.Pools;
using KAG.Shared;
using KAG.Shared.JSON;
using KAG.Shared.Network;
using KAG.Shared.Prototype;
using KAG.Shared.Transform;
using Newtonsoft.Json;

namespace KAG.Server
{
	public sealed class CorePlugin : Plugin
	{
		private const int MaxPlayers = 12;
		
		public override Version Version => new Version(1, 0, 0);
		public override bool ThreadSafe => false;

		private readonly HashSet<IClient> _clientsOnStandby;
		private readonly Dictionary<IClient, Player> _connectedPlayers;
		private readonly IMultiplayerSDKProxy _multiplayerSdkProxy;
		private readonly IContainer _container;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly World _world;
		
		public CorePlugin(PluginLoadData pluginLoadData) : base(pluginLoadData)
		{
			_clientsOnStandby = new HashSet<IClient>();
			_connectedPlayers = new Dictionary<IClient, Player>();

			_multiplayerSdkProxy = CreateMultiplayerSDKProxy();
			
			_container = CreateDependencyInjectionContainer();
			_lifetimeScope = _container.BeginLifetimeScope();
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

		private IContainer CreateDependencyInjectionContainer()
		{
			var builder = new ContainerBuilder();

			try
			{
				RegisterJSONDependencies(builder);
				RegisterSimulationDependencies(builder);
			}
			catch (Exception exception)
			{
				Logger.Log("An unexpected exception occured during the registration of dependencies.", LogType.Error, exception);
			}

			return builder.Build();
		}

		private void RegisterJSONDependencies(ContainerBuilder builder)
		{
			var settings = new JsonSerializerSettings()
			{
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				ContractResolver = new CustomContractResolver(),
				Converters = new List<JsonConverter>()
				{
					new IdentityConverter()
				}
			};
			
			builder.RegisterInstance(settings).SingleInstance();
		}
		private void RegisterSimulationDependencies(ContainerBuilder builder)
		{
			builder.RegisterType<PrototypeRepository>()
			   .WithParameter(new PositionalParameter(0, new string[] { ResourceDirectory }))
			   .AsSelf()
			   .SingleInstance();
			
			var componentTypeRepository = new ComponentTypeRepository();
			builder.RegisterInstance(componentTypeRepository).SingleInstance();

			builder.RegisterType<World>().AsSelf().SingleInstance();

			builder.RegisterType<EntityPool>().As<IEntityPool>().SingleInstance();
			builder.RegisterType(typeof(Entity)).AsSelf().InstancePerDependency();

			builder.RegisterType<ComponentPool>().As<IComponentPool>().SingleInstance();

			foreach (var componentType in componentTypeRepository.ComponentTypes)
				builder.RegisterType(componentType).AsSelf().InstancePerDependency();
		}

		private void OnClientConnected(object sender, ClientConnectedEventArgs args)
		{
			_clientsOnStandby.Add(args.Client);
			args.Client.MessageReceived += OnClientMessageReceived;
		}

		private void OnClientMessageReceived(object sender, MessageReceivedEventArgs args)
		{
			using var message = args.GetMessage();
			switch (message.Tag)
			{
				case NetworkTags.PlayerIdentification:
					OnPlayerIdentification(args.Client, message);
					break;
			}
		}

		private void OnPlayerIdentification(IClient client, Message message)
		{
			using var reader = message.GetReader();
			var identificationMessage = reader.ReadSerializable<PlayerIdentificationMessage>();
							
			// Create player
			var playerEntity = _world.CreateEntity(Identity.Player);
			var component = playerEntity.GetComponent<PlayerComponent>();
			var position = playerEntity.GetComponent<PositionComponent>();

			component.Id = client.ID;
			component.Name = identificationMessage.Name;
			
			var random = new Random();
			position.Value = new Vector2(random.Next(-20, 20), random.Next(-20, 20));

			var player = new Player(client, identificationMessage.Name, playerEntity);
			_connectedPlayers.Add(client, player);

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