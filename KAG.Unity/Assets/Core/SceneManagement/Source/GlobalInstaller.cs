using KAG.Unity.Common.Models;
using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public sealed class GlobalInstaller : Installer<GlobalInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<ApplicationModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<PlayerModel>().AsSingle();
            
			Container.BindInterfacesAndSelfTo<ApplicationViewModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<SettingsViewModel>().AsSingle();
		}
	}
}