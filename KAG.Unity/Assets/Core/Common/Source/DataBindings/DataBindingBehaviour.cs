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
	public sealed class DataBindingBehaviour : MonoBehaviour
	{
		#region Nested types

		private enum SourceMode
		{
			Injection,
			Behaviour
		}

		#endregion
		
		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnSourceModeChanged))]
		#endif
		[SerializeField]
		[FoldoutGroup("Source")]
		[LabelText("Mode")]
		private SourceMode _sourceMode = SourceMode.Injection;

		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAllowedSourceObservableAssemblyQualifiedTypeNames))]
		[OnValueChanged(nameof(OnSourceObservableChanged))]
		#endif
		[SerializeField]
		[FoldoutGroup("Source")]
		[LabelText("Type")]
		[ShowIf(nameof(_sourceMode), SourceMode.Injection)]
		private string _sourceObservableAssemblyQualifiedTypeName = string.Empty;

		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnSourceObservableChanged))]
		#endif
		[SerializeField]
		[FoldoutGroup("Source")]
		[LabelText("Instance")]
		[ShowIf(nameof(_sourceMode), SourceMode.Behaviour)]
		private ObservableBehaviour _sourceObservableInstance = null;

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

		private DataBinding _value;

		[Inject]
		public void Inject(DiContainer container)
		{
			try
			{
				var source = GetDataBindingSource(container);
				var target = GetDataBindingTarget(source.GetProperty().PropertyType);

				_value = TryGetDataBindingConverter(out var converter) 
					? new DataBinding(source, converter, target, enabled) 
					: new DataBinding(source, target, enabled);
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
			switch (_sourceMode)
			{
				case SourceMode.Injection:
				{
					if (string.IsNullOrEmpty(_sourceObservableAssemblyQualifiedTypeName))
						throw new InvalidOperationException($"Data binding cannot be built with `{nameof(_sourceMode)}={_sourceMode}` as the serialized type is null.");

					if (!TryConvertSourceObservableAssemblyQualifiedTypeName(out var sourceObservableType))
						throw new InvalidOperationException(
							$"Data binding cannot be built with `{nameof(_sourceMode)}={_sourceMode}` "
							+ $"as the serialized `{nameof(_sourceObservableAssemblyQualifiedTypeName)}={_sourceObservableAssemblyQualifiedTypeName}` is invalid.");

					var observable = (IObservable)container.Resolve(sourceObservableType);
					return new ObservableDataBindingSource(observable, new PropertyIdentifier(_sourcePropertyName));
				}

				case SourceMode.Behaviour:
				{
					if (_sourceObservableInstance == null)
						throw new InvalidOperationException($"Data binding cannot be built with `{nameof(_sourceMode)}={_sourceMode}` as the serialized behaviour is unassigned.");
						
					return new ObservableDataBindingSource(_sourceObservableInstance, new PropertyIdentifier(_sourcePropertyName));
				}
					
				default:
					throw new ArgumentOutOfRangeException($"Data binding cannot be built as `{nameof(_sourceMode)}={_sourceMode}` isn't handled.");
			}
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
		
		public IDataBindingTarget GetDataBindingTarget(Type sourcePropertyType)
		{
			if (_targetInstance == null)
				throw new InvalidOperationException("Data binding cannot be built as there is no target instance assigned.");

			return _dataBindingTargetBuilder.Build(sourcePropertyType, _targetInstance);
		}

		private bool TryGetSourceObservableType(out Type type)
		{
			type = default;
			
			switch (_sourceMode)
			{
				case SourceMode.Injection:
					return !string.IsNullOrEmpty(_sourceObservableAssemblyQualifiedTypeName) 
					       && TryConvertSourceObservableAssemblyQualifiedTypeName(out type);

				case SourceMode.Behaviour:
				{
					if (_sourceObservableInstance == null)
						return false;

					type = _sourceObservableInstance.GetType();
					return true;
				}
			}

			return false;
		}
		private bool TryConvertSourceObservableAssemblyQualifiedTypeName(out Type type)
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

		private IList<ValueDropdownItem<string>> GetAvailableSourcePropertyNames()
		{
			if (!TryGetSourceObservableType(out var sourceObservableType))
				return Array.Empty<ValueDropdownItem<string>>();

			return IMP_GetAvailableSourcePropertyNames(sourceObservableType);
		}
		private IList<ValueDropdownItem<string>> IMP_GetAvailableSourcePropertyNames(Type type) => 
			type.GetProperties(DataBindingConstants.PropertySearchFlags)
			   .Where(property => property.DeclaringType == type)
			   .Select(property => new ValueDropdownItem<string>(property.Name.NicifyName(), property.Name))
			   .ToArray();

		private void OnSourceModeChanged()
		{
			_sourceObservableAssemblyQualifiedTypeName = string.Empty;
			_sourceObservableInstance = null;
			
			OnSourceObservableChanged();
		}
		private void OnSourceObservableChanged() => 
			_sourcePropertyName = string.Empty;

		private bool HasValidSourcePropertyType() => 
			TryGetSourcePropertyType(out _);
		private bool TryGetSourcePropertyType(out Type type)
		{
			type = default;
			
			if (!TryGetSourceObservableType(out var sourceObservableType)
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