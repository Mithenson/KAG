using System;
using UnityEngine;

namespace KAG.Unity.Common.DataBindings
{
	[Serializable]
	public struct DataBindingTargetBuilder
	{
		public readonly static DataBindingTargetBuilder Default = new DataBindingTargetBuilder(DataBindingTargetBuildMode.Property, string.Empty);
			
		[SerializeField]
		[HideInInspector]
		private DataBindingTargetBuildMode _mode;
			
		[SerializeField]
		[HideInInspector]
		private string _name;

		public DataBindingTargetBuilder(DataBindingTargetBuildMode mode, string name)
		{
			_mode = mode;
			_name = name;
		}

		public IValueDataBindingTarget Build(Type sourceType, object instance)
		{
			switch (_mode)
			{
				case DataBindingTargetBuildMode.Property:
					return _name.ToReflectedPropertyDataBindingTarget(instance);

				case DataBindingTargetBuildMode.Method:
				{
					if (sourceType == null)
						throw new InvalidOperationException($"Cannot build a data binding target for `{nameof(_mode)}={_mode}` when the provided source type is null");
					
					return _name.ToReflectedMethodDataBindingTarget(sourceType, instance);
				}
			
				case DataBindingTargetBuildMode.ParameterlessMethod:
					return _name.ToReflectedParameterlessMethodDataBindingTarget(instance);

				default:
					throw new ArgumentOutOfRangeException($"Data binding cannot be built as `{nameof(_mode)}={_mode}` isn't handled.");
			}
		}
	}
}