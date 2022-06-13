using KAG.Unity.Common.Models;
using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class PersistentMVVMInstaller : Installer<PersistentMVVMInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<PlayerModel>().AsSingle();
            
			Container.BindInterfacesAndSelfTo<ApplicationModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<ApplicationViewModel>().AsSingle();
            
			Container.BindInterfacesAndSelfTo<SettingsViewModel>().AsSingle();
		}
	}
}