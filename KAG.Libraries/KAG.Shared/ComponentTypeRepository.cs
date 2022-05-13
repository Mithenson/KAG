using System;
using System.Collections.Generic;
using System.Linq;
using KAG.Shared.Extensions;

namespace KAG.Shared
{
	public sealed class ComponentTypeRepository
	{
		#region Nested types

		private sealed class TypeComparer : IComparer<Type>
		{
			public readonly static TypeComparer Default = new TypeComparer();
			
			public int Compare(Type lhs, Type rhs) => 
				string.Compare(lhs.FullName, rhs.FullName, StringComparison.Ordinal);
		}

		#endregion

		public IReadOnlyCollection<Type> ComponentTypes => _componentTypes;
		
		private Type[] _componentTypes;
		private Dictionary<Type, ushort> _componentTypeIds;
		private Dictionary<Type, HashSet<Type>> _componentTypeMappings;
		
		public ComponentTypeRepository()
		{
			_componentTypes = AppDomain.CurrentDomain.GetAssemblies()
			   .SelectMany(assembly => assembly.GetTypes())
			   .Where(type => !type.IsGenericType && !type.IsAbstract && typeof(Component).IsAssignableFrom(type))
			   .ToArray();
			
			Array.Sort(_componentTypes, TypeComparer.Default);
			
			_componentTypeIds = new Dictionary<Type, ushort>();
			_componentTypeMappings = new Dictionary<Type, HashSet<Type>>();
			ushort index = 0;
			
			foreach (var componentType in _componentTypes)
			{
				MapOut(componentType);
				
				_componentTypeIds.Add(componentType, index);
				index++;
			}
		}

		private void MapOut(Type componentType)
		{
			foreach (var interfaceType in componentType.GetInterfaces())
				Map(componentType, interfaceType);
			
			var current = componentType.BaseType;
			while (current != typeof(Component))
			{
				Map(current, componentType);
				current = current.BaseType;
			}
		}
		private void Map(Type componentType, Type implementedType)
		{
			if (!_componentTypeMappings.TryGetValue(implementedType, out var componentTypes))
			{
				componentTypes = new HashSet<Type>();
				_componentTypeMappings.Add(implementedType, componentTypes);
			}
			
			componentTypes.Add(componentType);
		}

		public void ValidateTypeAsValidComponentType(Type type)
		{
			ValidateTypeAsComponentType(type);
			ValidateComponentType(type);
		}
		public void ValidateTypeAsComponentType(Type type)
		{
			if (!typeof(Component).IsAssignableFrom(type))
				throw new InvalidOperationException($"The provided `type={type}` isn't a component type.");
		}
		public void ValidateComponentType(Type componentType)
		{
			if (componentType.IsAbstract)
				throw new InvalidOperationException($"The provided `{nameof(componentType)}={componentType}` isn't concrete. Abstract types aren't handled.");
			
			if (componentType.IsGenericType)
				throw new InvalidOperationException($"The provided `{nameof(componentType)}={componentType}{componentType.GenericityToString()}` is generic. This isn't allowed.");
		}
		
		public Type GetComponentType(ushort componentTypeId)
		{
			if (componentTypeId >= _componentTypes.Length)
				throw new ArgumentOutOfRangeException($"The given `{nameof(componentTypeId)}={componentTypeId}` is past the last `validValue={_componentTypes.Length - 1}`.");

			return _componentTypes[componentTypeId];
		}

		public ushort GetComponentTypeId<TComponent>() where TComponent : Component => 
			IMP_GetComponentTypeId(typeof(TComponent));
		public ushort GetComponentTypeId(Type componentType)
		{
			try
			{
				ValidateTypeAsComponentType(componentType);
			}
			catch (InvalidOperationException exception)
			{
				throw new InvalidOperationException($"The component type id request couldn't be fulfilled as the provided `type={componentType}` isn't valid", exception);
			}
			
			return IMP_GetComponentTypeId(componentType);
		}
		private ushort IMP_GetComponentTypeId(Type componentType)
		{
			try
			{
				ValidateComponentType(componentType);
			}
			catch (InvalidOperationException exception)
			{
				throw new InvalidOperationException($"The component type id request couldn't be fulfilled as the provided `type={componentType}` isn't valid", exception);
			}
			
			return _componentTypeIds[componentType];
		}

		public IEnumerable<Type> GetMappedComponentTypes(Type type)
		{
			if (!_componentTypeMappings.TryGetValue(type, out var mappedComponentTypes))
				throw new InvalidOperationException($"The `{nameof(type)}={type} isn't implemented by any concrete component type.");

			return mappedComponentTypes;
		}
	}
}