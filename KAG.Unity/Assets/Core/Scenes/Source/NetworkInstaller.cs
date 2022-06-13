using System.Linq;
using System.Reflection;
using KAG.Unity.Gameplay;
using KAG.Unity.Network;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class NetworkInstaller : Installer<NetworkInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind(typeof(NetworkManager), typeof(ILateDisposable)).To<NetworkManager>().AsSingle();
			
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
		}
	}
}