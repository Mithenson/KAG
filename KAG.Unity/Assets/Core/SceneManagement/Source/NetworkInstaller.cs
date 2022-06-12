using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using KAG.Shared.Messages;
using KAG.Unity.Gameplay;
using KAG.Unity.Network;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class NetworkInstaller : Installer<NetworkInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<NetworkManager>().AsSingle();
			
			Container.BindInterfacesAndSelfTo<UnityMessageDispatcher>().AsSingle();
			InstallMessageHandlers();
		}

		private void InstallMessageHandlers()
		{
			var handlerTypes = new Assembly[]
				{
					typeof(GameplayMessageHandler).Assembly
				}
			   .SelectMany(assembly => assembly.GetTypes())
			   .Where(type => !type.IsAbstract && typeof(IUnityMessageHandler).IsAssignableFrom(type))
			   .ToArray();

			foreach (var handlerType in handlerTypes)
				Container.BindInterfacesAndSelfTo(handlerType).AsSingle();
			
			//Container.Bind<IMessageHandler>().To(handlerTypes).AsSingle();
		}
	}
}