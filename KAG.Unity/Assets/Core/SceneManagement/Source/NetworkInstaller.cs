using KAG.Unity.Network;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class NetworkInstaller : Installer<NetworkInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<JoinMatchHandler>().AsTransient();
			Container.BindInterfacesAndSelfTo<NetworkManager>().AsSingle();
		}
	}
}