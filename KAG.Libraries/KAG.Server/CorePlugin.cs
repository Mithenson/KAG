using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using DarkRift;
using DarkRift.Server;
using KAG.Server.Pools;
using KAG.Shared;
using KAG.Shared.Gameplay;
using KAG.Shared.Json;
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

			if (!TryCreateDependencyInjectionContainer(out _container))
				return;
			
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

		private bool TryCreateDependencyInjectionContainer(out IContainer container)
		{
			var builder = new ContainerBuilder();

			try
			{
				RegisterSimulationDependencies(builder);
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
		
		private void RegisterSimulationDependencies(ContainerBuilder builder)
		{
			var componentTypeRepository = new ComponentTypeRepository();
			builder.RegisterInstance(componentTypeRepository).SingleInstance();
			
			RegisterPrototypeRepository(builder, componentTypeRepository);

			builder.RegisterType<World>().AsSelf().SingleInstance();

			builder.RegisterType<EntityPool>().As<IEntityPool>().SingleInstance();
			builder.RegisterType(typeof(Entity)).AsSelf().InstancePerDependency();

			builder.RegisterType<ComponentPool>().As<IComponentPool>().SingleInstance();

			foreach (var componentType in componentTypeRepository.ComponentTypes)
				builder.RegisterType(componentType).AsSelf().InstancePerDependency();
		}
		private void RegisterPrototypeRepository(ContainerBuilder builder, ComponentTypeRepository componentTypeRepository)
		{
			var prototypeDefinitionPaths = Directory.GetFiles(ResourceDirectory, "*.proto");
			var prototypes = new List<Prototype>(prototypeDefinitionPaths.Length);

			for (var i = 0; i < prototypeDefinitionPaths.Length; i++)
			{
				var prototypeDefinition = File.ReadAllText(prototypeDefinitionPaths[i]);
				var prototype = JsonConvert.DeserializeObject<Prototype>(prototypeDefinition, JsonUtilities.StandardSerializerSettings);
				prototypes.Add(prototype);
			}

			var prototypeRepository = new PrototypeRepository();
			prototypeRepository.Initialize(prototypes, componentTypeRepository);
			
			builder.RegisterInstance(prototypeRepository).SingleInstance();
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

				case NetworkTags.PlayerMovement:
				{
					using (var reader = message.GetReader())
					{
						var movementMessage = reader.ReadSerializable<PlayerMovementMessage>();
						
						var player = _connectedPlayers[args.Client];
						var movement = player.Entity.GetComponent<MovementComponent>();
						
						movement.Move(movementMessage.Input, out var updatedPosition);

						using (var writer = DarkRiftWriter.Create())
						{
							writer.Write(new PlayerPositionUpdateMessage()
							{
								ClientId = args.Client.ID,
								Id = movementMessage.Id,
								Position = updatedPosition
							});

							using (var messageToSend = Message.Create(NetworkTags.PlayerPositionUpdate, writer))
								args.Client.SendMessage(messageToSend, SendMode.Unreliable);
							
							using (var messageToSend = Message.Create(NetworkTags.RemotePlayerPositionUpdate, writer))
							{
								foreach (var client in ClientManager.GetAllClients().Where(client => client != args.Client))
									client.SendMessage(messageToSend, SendMode.Unreliable);
							}
						}
					}

					break;
				}
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