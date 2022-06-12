using System.Linq;
using System.Reflection;
using Autofac;
using KAG.Server.Gameplay;
using KAG.Server.Network;
using KAG.Shared.Messages;

namespace KAG.Server.DependencyInjection
{
	public sealed class NetworkInstaller : Installer<NetworkInstaller>
	{
		public NetworkInstaller(ContainerBuilder builder)
			: base(builder) { }
		
		public override void Install()
		{
			Builder.RegisterType<ServerMessageDispatcher>().AsSelf().SingleInstance();
			
			var handlerTypes = new Assembly[]
				{
					typeof(GameplayMessageHandler).Assembly
				}
			   .SelectMany(assembly => assembly.GetTypes())
			   .Where(type => !type.IsAbstract && typeof(IServerMessageHandler).IsAssignableFrom(type))
			   .ToArray();

			Builder.RegisterTypes(handlerTypes).As<IMessageHandler>().SingleInstance();
		}
	}
}