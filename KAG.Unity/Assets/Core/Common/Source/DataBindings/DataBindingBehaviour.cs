using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KAG.Shared.Extensions;
using KAG.Unity.Common.Observables;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Common.DataBindings
{
	public enum SourceBuildMode
	{
		Injection,
		Behaviour
	}
	
	[Serializable]
	public sealed class SourceBuilder
	{
		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnSourceModeChanged))]
		#endif
		[SerializeField]
		[LabelText("Mode")]
		private SourceBuildMode sourceBuildMode = SourceBuildMode.Injection;

		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAllowedSourceObservableAssemblyQualifiedTypeNames))]
		#endif
		[SerializeField]
		[LabelText("Type")]
		[ShowIf(nameof(sourceBuildMode), SourceBuildMode.Injection)]
		private string _sourceObservableAssemblyQualifiedTypeName = string.Empty;
		
		[SerializeField]
		[LabelText("Instance")]
		[ShowIf(nameof(sourceBuildMode), SourceBuildMode.Behaviour)]
		private ObservableBehaviour _sourceObservableInstance = null;
		
		public IObservable Build(DiContainer container)
		{
			switch (sourceBuildMode)
			{
				case SourceBuildMode.Injection:
				{
					if (string.IsNullOrEmpty(_sourceObservableAssemblyQualifiedTypeName))
						throw new InvalidOperationException($"Source cannot be built with `{nameof(sourceBuildMode)}={sourceBuildMode}` as the serialized type is null.");

					if (!TryConvertSourceObservableAssemblyQualifiedTypeName(out var sourceObservableType))
						throw new InvalidOperationException(
							$"Source cannot be built with `{nameof(sourceBuildMode)}={sourceBuildMode}` "
							+ $"as the serialized `{nameof(_sourceObservableAssemblyQualifiedTypeName)}={_sourceObservableAssemblyQualifiedTypeName}` is invalid.");

					return (IObservable)container.Resolve(sourceObservableType);
				}

				case SourceBuildMode.Behaviour:
				{
					if (_sourceObservableInstance == null)
						throw new InvalidOperationException($"Source cannot be built with `{nameof(sourceBuildMode)}={sourceBuildMode}` as the serialized behaviour is unassigned.");
						
					return _sourceObservableInstance;
				}
					
				default:
					throw new ArgumentOutOfRangeException($"Data binding cannot be built as `{nameof(sourceBuildMode)}={sourceBuildMode}` isn't handled.");
			}
		}
		
		public bool TryGetSourceObservableType(out Type type)
		{
			type = default;
			
			switch (sourceBuildMode)
			{
				case SourceBuildMode.Injection:
					return !string.IsNullOrEmpty(_sourceObservableAssemblyQualifiedTypeName) 
					       && TryConvertSourceObservableAssemblyQualifiedTypeName(out type);

				case SourceBuildMode.Behaviour:
				{
					if (_sourceObservableInstance == null)
						return false;

					type = _sourceObservableInstance.GetType();
					return true;
				}
			}

			return false;
		}
		public bool TryConvertSourceObservableAssemblyQualifiedTypeName(out Type type)
		{
			try
			{
				type = Type.GetType(_sourceObservableAssemblyQualifiedTypeName);
			}
			catch
			{
				type = default;
				return false;
			}

			return type != null;
		}
		
		#if UNITY_EDITOR
		
		private IList<ValueDropdownItem<string>> GetAllowedSourceObservableAssemblyQualifiedTypeNames() => 
			AppDomain.CurrentDomain
			   .GetAssemblies()
			   .Where(assembly => assembly.GetName().Name.StartsWith(Constants.RootAssemblyName))
			   .SelectMany(assembly => assembly.GetTypes())
			   .Where(type =>
				{
					return !type.IsAbstract
					       && typeof(IObservable).IsAssignableFrom(type)
					       && !typeof(UnityEngine.Object).IsAssignableFrom(type);
				})
			   .Select(type => new ValueDropdownItem<string>(type.Name.NicifyName(), type.AssemblyQualifiedName))
			   .ToArray();
		
		private void OnSourceModeChanged()
		{
			_sourceObservableAssemblyQualifiedTypeName = string.Empty;
			_sourceObservableInstance = null;
		}
		
		#endif
	}
	
	public sealed class DataBindingBehaviour : MonoBehaviour
	{
		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnSourceObservableChanged))]
		#endif
		[SerializeField]
		[FoldoutGroup("Source")]
		[InlineProperty]
		[HideLabel]
		private SourceBuilder _sourceBuilder = new SourceBuilder();
	
		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAvailableSourcePropertyNames))]
		#endif
		[SerializeField]
		[FoldoutGroup("Source")]
		[LabelText("Property")]
		private string _sourcePropertyName = string.Empty;

		#if UNITY_EDITOR
		[ShowIf(nameof(HasValidSourcePropertyType))]
		#endif
		[SerializeReference]
		private IDataBindingConverter[] _converters = new IDataBindingConverter[0];
		
		#if UNITY_EDITOR
		[FoldoutGroup("Target", VisibleIf = nameof(HasValidSourcePropertyType))]
		[OnValueChanged(nameof(OnTargetInstanceChanged))]
		#endif
		[SerializeField]
		[LabelText("Instance")]
		private UnityEngine.Object _targetInstance = null;

		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAvailableDataBindingTargetBuilders))]
		#endif
		[SerializeField]
		[FoldoutGroup("Target")]
		[LabelText("Binding")]
		private DataBindingTargetBuilder _dataBindingTargetBuilder = DataBindingTargetBuilder.Default;

		private ValueDataBinding _value;

		[Inject]
		public void Inject(DiContainer container)
		{
			try
			{
				var source = GetDataBindingSource(container);
				var target = GetDataBindingTarget(source.GetProperty().PropertyType);

				_value = TryGetDataBindingConverter(out var converter) 
					? new ValueDataBinding(source, converter, target, enabled) 
					: new ValueDataBinding(source, target, enabled);
			}
			catch(Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}

		private void OnEnable() =>
			_value?.SetActive(enabled);

		private void OnDisable() =>
			_value?.SetActive(enabled);
		
		public ObservableDataBindingSource GetDataBindingSource(DiContainer container)
		{
			var sourceObservable = _sourceBuilder.Build(container);
			return new ObservableDataBindingSource(sourceObservable, new PropertyIdentifier(_sourcePropertyName));
		}
		
		private bool TryGetDataBindingConverter(out IDataBindingConverter converter)
		{
			if (_converters.Length == 0)
			{
				converter = default;
				return false;
			}

			if (_converters.Length == 1)
			{
				converter = _converters[0];
				return true;
			}

			converter = new DataBindingDecoratedConverter(_converters[0], GetChildConverter(1));
			return true;
		}
		private IDataBindingConverter GetChildConverter(int index)
		{
			if (index == _converters.Length - 1)
				return _converters[_converters.Length - 1];

			return new DataBindingDecoratedConverter(_converters[index], GetChildConverter(index + 1));
		}
		
		public IValueDataBindingTarget GetDataBindingTarget(Type sourcePropertyType)
		{
			if (_targetInstance == null)
				throw new InvalidOperationException("Data binding cannot be built as there is no target instance assigned.");

			return _dataBindingTargetBuilder.Build(sourcePropertyType, _targetInstance);
		}

		#if UNITY_EDITOR
		
		private void OnSourceObservableChanged() => 
			_sourcePropertyName = string.Empty;
		
		private IList<ValueDropdownItem<string>> GetAvailableSourcePropertyNames()
		{
			if (!_sourceBuilder.TryGetSourceObservableType(out var sourceObservableType))
				return Array.Empty<ValueDropdownItem<string>>();

			return IMP_GetAvailableSourcePropertyNames(sourceObservableType);
		}
		private IList<ValueDropdownItem<string>> IMP_GetAvailableSourcePropertyNames(Type type) => 
			type.GetProperties(DataBindingConstants.PropertySearchFlags)
			   .Where(property => property.DeclaringType == type)
			   .Select(property => new ValueDropdownItem<string>(property.Name.NicifyName(), property.Name))
			   .ToArray();
		
		private bool HasValidSourcePropertyType() => 
			TryGetSourcePropertyType(out _);
		private bool TryGetSourcePropertyType(out Type type)
		{
			type = default;
			
			if (!_sourceBuilder.TryGetSourceObservableType(out var sourceObservableType)
				|| string.IsNullOrEmpty(_sourcePropertyName))
				return false;

			PropertyInfo sourceProperty;
			try
			{
				sourceProperty = _sourcePropertyName.ToPropertyForDataBindingTarget(sourceObservableType);
			}
			catch
			{
				return false;
			}

			if (sourceProperty == null)
				return false;

			type = sourceProperty.PropertyType;
			return true;
		}

		private void OnTargetInstanceChanged() => 
			_dataBindingTargetBuilder = DataBindingTargetBuilder.Default;

		private IList<ValueDropdownItem<DataBindingTargetBuilder>> GetAvailableDataBindingTargetBuilders()
		{
			if (!TryGetSourcePropertyType(out var sourcePropertyType)
			    || _targetInstance == null)
				return new ValueDropdownItem<DataBindingTargetBuilder>[]
				{
					DataBindingConstants.DefaultDataBindingTargetBuilderDropdownItem
				};

			var targetInstanceType = _targetInstance.GetType();
			return DataBindingUtilities.Editor.GetAvailableDataBindingTargetBuilders(sourcePropertyType, targetInstanceType);
		}

		#endif
	}
}