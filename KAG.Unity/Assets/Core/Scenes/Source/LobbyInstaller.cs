using KAG.Unity.Network;
using KAG.Unity.Network.Models;
using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class LobbyInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<JoinMatchModel>().AsSingle();
			Container.BindInterfacesAndSelfTo<JoinMatchViewModel>().AsSingle();
            
			Container.BindInterfacesAndSelfTo<JoinMatchHandler>().AsTransient();
		}
	}
}