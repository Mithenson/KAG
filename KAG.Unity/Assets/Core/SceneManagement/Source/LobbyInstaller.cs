using KAG.Unity.Common.Models;
using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class LobbyInstaller : Installer<LobbyInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<JoinMatchModel>().AsSingle();
            
			Container.BindInterfacesAndSelfTo<JoinMatchViewModel>().AsSingle();
		}
	}
}