using System;
using System.Collections.Generic;
using System.Linq;
using KAG.Shared.Extensions;
using KAG.Unity.Common.DataBindings;
using KAG.Unity.Common.Observables;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace KAG.Unity.Common
{
	public class CommandBehaviour : MonoBehaviour
	{
		#region Nested types

		private enum TargetMode
		{
			Injection,
			Behaviour
		}

		#endregion

		protected virtual Type SourceType => null;
		
		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnTargetModeChanged))]
		#endif
		[SerializeField]
		[LabelText("Mode")]
		private TargetMode _targetMode = TargetMode.Injection;
		
		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAllowedTargetAssemblyQualifiedTypeNames))]
		[OnValueChanged(nameof(OnTargetChanged))]
		#endif
		[SerializeField]
		[LabelText("Type")]
		[ShowIf(nameof(_targetMode), TargetMode.Injection)]
		private string _targetAssemblyQualifiedTypeName = string.Empty;

		#if UNITY_EDITOR
		[OnValueChanged(nameof(OnTargetChanged))]
		#endif
		[SerializeField]
		[LabelText("Instance")]
		[ShowIf(nameof(_targetMode), TargetMode.Behaviour)]
		private UnityEngine.Object _targetInstance = null;
		
		#if UNITY_EDITOR
		[ValueDropdown(nameof(GetAvailableDataBindingTargetBuilders))]
		#endif
		[SerializeField]
		[LabelText("Binding")]
		private DataBindingTargetBuilder _dataBindingTargetBuilder = DataBindingTargetBuilder.Default;

		protected IValueDataBindingTarget _dataBindingTarget;

		[Inject]
		public void Inject(DiContainer container)
		{
			try
			{
				_dataBindingTarget = GetDataBindingTarget(container);
			}
			catch(Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}

		public void Execute() => 
			_dataBindingTarget.Set(default);

		private IValueDataBindingTarget GetDataBindingTarget(DiContainer container)
		{
			var target = GetTarget(container);
			return _dataBindingTargetBuilder.Build(SourceType, target);
		}
		private object GetTarget(DiContainer container)
		{
			switch (_targetMode)
			{
				case TargetMode.Injection:
				{
					if (string.IsNullOrEmpty(_targetAssemblyQualifiedTypeName))
						throw new InvalidOperationException($"Data binding cannot be built with `{nameof(_targetMode)}={_targetMode}` as the serialized type is null.");

					if (!TryConvertTargetAssemblyQualifiedTypeName(out var targetType))
						throw new InvalidOperationException(
							$"Data binding cannot be built with `{nameof(_targetMode)}={_targetMode}` "
							+ $"as the serialized `{nameof(_targetAssemblyQualifiedTypeName)}={_targetAssemblyQualifiedTypeName}` is invalid.");

					return container.Resolve(targetType);
				}

				case TargetMode.Behaviour:
				{
					if (_targetInstance == null)
						throw new InvalidOperationException($"Data binding cannot be built with `{nameof(_targetMode)}={_targetMode}` as the serialized behaviour is unassigned.");
						
					return _targetInstance;
				}

				default:
					throw new ArgumentOutOfRangeException($"Data binding cannot be built as `{nameof(_targetMode)}={_targetMode}` isn't handled.");
			}
		}

		private bool TryGetTargetType(out Type type)
		{
			type = default;
			
			switch (_targetMode)
			{
				case TargetMode.Injection:
					return !string.IsNullOrEmpty(_targetAssemblyQualifiedTypeName) 
					       && TryConvertTargetAssemblyQualifiedTypeName(out type);

				case TargetMode.Behaviour:
				{
					if (_targetInstance == null)
						return false;

					type = _targetInstance.GetType();
					return true;
				}
			}

			return false;
		}
		private bool TryConvertTargetAssemblyQualifiedTypeName(out Type type)
		{
			try
			{
				type = Type.GetType(_targetAssemblyQualifiedTypeName);
			}
			catch
			{
				type = default;
				return false;
			}

			return type != null;
		}
		
		#if UNITY_EDITOR

		private IList<ValueDropdownItem<string>> GetAllowedTargetAssemblyQualifiedTypeNames() =>
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
		
		private void OnTargetModeChanged()
		{
			_targetAssemblyQualifiedTypeName = string.Empty;
			_targetInstance = null;
			
			OnTargetChanged();
		}
		private void OnTargetChanged() => 
			_dataBindingTargetBuilder = DataBindingTargetBuilder.Default;

		private IList<ValueDropdownItem<DataBindingTargetBuilder>> GetAvailableDataBindingTargetBuilders()
		{
			if (!TryGetTargetType(out var targetType))
				return new List<ValueDropdownItem<DataBindingTargetBuilder>>()
				{
					DataBindingConstants.DefaultDataBindingTargetBuilderDropdownItem
				};

			return SourceType != null 
				? DataBindingUtilities.Editor.GetAvailableDataBindingTargetBuilders(SourceType, targetType) 
				: DataBindingUtilities.Editor.GetAvailableDataBindingTargetBuilders(targetType);
		}

		#endif
	}
}