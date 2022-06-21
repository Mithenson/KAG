using KAG.Unity.Common;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class PersistentConfigurationInstaller : Installer<PersistentConfigurationInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<ConfigurationMonitor<CursorConfiguration>>().ToSelf().AsSingle();
		}
	}
}