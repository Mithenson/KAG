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

		private void OnDestroy() => 
			_value?.Dispose();

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
			if (_converters.Length > 0)
			{
				var lastConverter = _converters[_converters.Length - 1];
				
				type = lastConverter?.OutputType;
				if (type != null)
					return true;
			}
			
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