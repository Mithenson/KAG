using UnityEngine;
using Zenject;

namespace KAG.Unity.Simulation
{
	[RequireComponent(typeof(PresentationBehaviour))]
	public sealed class PresentationInstaller : MonoInstaller
	{
		public override void InstallBindings()
		{
			Container.Bind<PresentationBehaviour>().FromComponentOn(gameObject).AsSingle();
		}
	}
}