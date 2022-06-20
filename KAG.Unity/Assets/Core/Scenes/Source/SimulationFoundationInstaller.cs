﻿using KAG.Shared;
using KAG.Shared.Prototype;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class SimulationFoundationInstaller : Installer<SimulationFoundationInstaller>
	{
		public override void InstallBindings()
		{
			Container.BindInterfacesAndSelfTo<ComponentTypeRepository>().AsSingle();
			Container.BindInterfacesAndSelfTo<PrototypeRepository>().AsSingle();
		}
	}
}