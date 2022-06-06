using KAG.Shared;
using KAG.Unity.Network;
using KAG.Unity.UI.ViewModels;
using Zenject;

namespace KAG.Unity.SceneManagement
{
	public class GameInstaller : MonoInstaller
	{
		private ComponentTypeRepository _componentTypeRepository;

		[Inject]
		public void Inject(ComponentTypeRepository componentTypeRepository) =>
			_componentTypeRepository = componentTypeRepository;
		
		public override void InstallBindings()
		{
			SimulationInstaller.Install(Container, _componentTypeRepository);
			
			Container.BindInterfacesAndSelfTo<LeaveMatchViewModel>().AsSingle();

			Container.BindInterfacesAndSelfTo<NetworkManager>().AsSingle();
		}
	}
}