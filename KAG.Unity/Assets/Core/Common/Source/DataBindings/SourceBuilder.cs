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
}