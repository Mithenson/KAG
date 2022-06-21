using KAG.Unity.Common.Models;
using KAG.Unity.Scenes.Models;
using KAG.Unity.Scenes.ViewModels;
using KAG.Unity.UI.Models;
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

			Container.BindInterfacesAndSelfTo<UIModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<UIViewModel>().AsSingle();
			
			Container.BindInterfacesAndSelfTo<SettingsViewModel>().AsSingle();
			
			Container.BindInterfacesAndSelfTo<CursorService>().AsSingle();
			Container.BindInterfacesAndSelfTo<CursorModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<CursorViewModel>().AsSingle();
		}
	}
}