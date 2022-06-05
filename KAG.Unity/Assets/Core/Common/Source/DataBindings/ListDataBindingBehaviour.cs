using System;
using System.Collections.Generic;
using System.Linq;
using KAG.Shared.Extensions;
using KAG.Unity.Common.Observables;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Common.DataBindings
{
	public sealed class ListDataBindingBehaviour : MonoBehaviour
	{
		#region Nested types

		private sealed class BindingInstaller : Installer
		{
			private object _binding;
			
			public BindingInstaller(object binding) => 
				_binding = binding;

			public override void InstallBindings() =>
				Container.BindInterfacesAndSelfTo(_binding.GetType()).FromInstance(_binding).AsSingle();
		}

		#endregion
		
		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnSourceObservableChanged))]
		#endif
		[SerializeField]
		[InlineProperty]
		[HideLabel]
		private SourceBuilder _sourceBuilder = new SourceBuilder();

		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAvailableSourceListNames))]
		#endif
		[SerializeField]
		[LabelText("List")]
		private string _sourceListName = string.Empty;

		[SerializeField]
		[AssetsOnly]
		private GameObject _prefab;

		private DiContainer _container;
		private ObservableList<GameObject> _targets;
		private ListDataBinding _value;
		
		[Inject]
		public void Inject(DiContainer container)
		{
			_container = container;

			try
			{
				var observable = _sourceBuilder.Build(container);
				var listProperty = _sourceListName.ToPropertyForDataBindingTarget(observable);
				var observableList = (IObservableList)listProperty.GetValue(observable);

				var converter = new LambdaDataBindingConverter<object, GameObject>(InstantiateTarget);

				_targets = new ObservableList<GameObject>();
				_targets.OnElementRemoved += OnTargetRemoved;

				_value = new ListDataBinding(observableList, converter, _targets, enabled);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
		
		private void OnEnable() =>
			_value?.SetActive(enabled);

		private void OnDisable() =>
			_value?.SetActive(enabled);

		private void OnDestroy()
		{
			_targets.OnElementRemoved -= OnTargetRemoved;
			_value?.Dispose();
		}

		private GameObject InstantiateTarget(object source)
		{
			var context = _container.InstantiatePrefabForComponent<GameObjectContext>(_prefab);
			context.AddNormalInstaller(new BindingInstaller(source));
			context.Run();

			return context.gameObject;
		}

		private void OnTargetRemoved(object _, ListElementRemovedEventArgs args) =>
			Destroy((GameObject)args.Item);

		#if UNITY_EDITOR
		
		private void OnSourceObservableChanged() => 
			_sourceListName = string.Empty;
		
		private IList<ValueDropdownItem<string>> GetAvailableSourceListNames()
		{
			if (!_sourceBuilder.TryGetSourceObservableType(out var sourceObservableType))
				return Array.Empty<ValueDropdownItem<string>>();

			return IMP_GetAvailableSourceListNames(sourceObservableType);
		}
		private IList<ValueDropdownItem<string>> IMP_GetAvailableSourceListNames(Type type) => 
			type.GetProperties(DataBindingConstants.PropertySearchFlags)
			   .Where(property => property.DeclaringType == type && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(ObservableList<>))
			   .Select(property => new ValueDropdownItem<string>(property.Name.NicifyName(), property.Name))
			   .ToArray();

		#endif
	}
}