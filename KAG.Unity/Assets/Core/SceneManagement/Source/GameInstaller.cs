using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public class GameInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<LeaveMatchViewModel>().AsSingle();
		}
	}
}