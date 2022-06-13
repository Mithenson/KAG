using KAG.Unity.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace KAG.Unity.Scenes
{
	public sealed class InputInstaller : Installer<InputActionAsset, InputInstaller>
	{
		private InputActionAsset _inputs;
		
		public InputInstaller(InputActionAsset inputs) => 
			_inputs = inputs;
		
		public override void InstallBindings()
		{
			var copy = Object.Instantiate(_inputs);
			Container.BindInstance(copy).AsSingle();

			foreach (var actionMap in copy.actionMaps)
			{
				Container.BindInstance(actionMap).WithId(actionMap.name);
				
				if (actionMap.name == UnityConstants.Inputs.GameplayMap)
					actionMap.Disable();

				foreach (var action in actionMap.actions)
					Container.BindInstance(action).WithId($"{actionMap.name}/{action.name}");
			}
		}
	}
}