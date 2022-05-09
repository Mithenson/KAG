using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DarkRift;
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
			if (!componentType.IsAbstract)
				throw new InvalidOperationException($"The provided `{nameof(componentType)}={componentType}` isn't concrete. Abstract types aren't handled.");
			
			if (!componentType.IsGenericType)
				throw new InvalidOperationException($"The provided `{nameof(componentType)}={componentType}` is generic. This isn't allowed.");
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

	public interface IComponentFactory
	{
		Component CreateComponent(Type componentType);
		TComponent CreateComponent<TComponent>() where TComponent : Component;
	}
	
	public sealed class Entity : IEquatable<Entity>, IDarkRiftSerializable
	{
		public readonly ushort Id;

		internal ComponentTypeRepository ComponentTypeRepository => _componentTypeRepository;

		private readonly ComponentTypeRepository _componentTypeRepository;
		private readonly IComponentFactory _componentFactory;
		
		private Dictionary<Type, Component> _components;
		
		public Entity(ushort id, ComponentTypeRepository componentTypeRepository, IComponentFactory componentFactory)
		{
			Id = id;
			
			_componentTypeRepository = componentTypeRepository;
			_componentFactory = componentFactory;

			_components = new Dictionary<Type, Component>();
		}

		public void AddComponent(Component component)
		{
			var type = component.GetType();
			if (_components.ContainsKey(type))
				throw new InvalidOperationException($"The `{nameof(Entity)}={this.ToShortString()}` already has a component of `{nameof(type)}={type}`.");
			
			_components.Add(type, component);
			
			component.Owner = this;
			component.OnAddedToEntity();
		}

		public bool HasComponent<TComponent>() where TComponent : Component => 
			IMP_HasComponent(typeof(TComponent));
		public bool HasComponent(Type componentType)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_HasComponent(componentType);
		}
		private bool IMP_HasComponent(Type componentType)
		{
			_componentTypeRepository.ValidateComponentType(componentType);
			return IMP_UNCHECKED_HasComponent(componentType);
		}
		private bool IMP_UNCHECKED_HasComponent(Type componentType) =>
			_components.ContainsKey(componentType);

		public bool HasAnyComponentImplementing<T>() => 
			HasAnyComponentImplementing(typeof(T));
		public bool HasAnyComponentImplementing(Type type)
		{
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_HasComponent(componentType))
					return true;
			}

			return false;
		}

		public TComponent GetComponent<TComponent>() where TComponent : Component => 
			(TComponent)IMP_GetComponent(typeof(TComponent));
		public Component GetComponent(Type componentType)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_GetComponent(componentType);
		}
		private Component IMP_GetComponent(Type componentType)
		{
			_componentTypeRepository.ValidateComponentType(componentType);
			return IMP_UNCHECKED_GetComponent(componentType);
		}
		private Component IMP_UNCHECKED_GetComponent(Type componentType)
		{
			if (!_components.TryGetValue(componentType, out var component))
				throw new InvalidOperationException($"The `{nameof(Entity)}={this.ToShortString()}` doesn't have a component of `type={componentType}`.");

			return component;
		}

		public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : Component
		{
			if (!IMP_TryGetComponent(typeof(TComponent), out var uncastedComponent))
			{
				component = default;
				return false;
			}

			component = (TComponent)uncastedComponent;
			return true;
		}
		public bool TryGetComponent(Type componentType, out Component component)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_TryGetComponent(componentType, out component);
		}
		private bool IMP_TryGetComponent(Type componentType, out Component component)
		{
			_componentTypeRepository.ValidateComponentType(componentType);
			return IMP_UNCHECKED_TryGetComponent(componentType, out component);
		}
		private bool IMP_UNCHECKED_TryGetComponent(Type componentType, out Component component) => 
			_components.TryGetValue(componentType, out component);

		public T GetFirstComponentImplementing<T>() where T : class => 
			GetFirstComponentImplementing(typeof(T)) as T;
		public Component GetFirstComponentImplementing(Type type)
		{
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out var component))
					return component;
			}

			throw new InvalidOperationException($"The `{nameof(Entity)}={this.ToShortString()}` has no component implementing the `{nameof(type)}={type}`");
		}
		
		public bool TryGetFirstComponentImplementing<T>(out T component) where T : class
		{
			if (!TryGetFirstComponentImplementing(typeof(T), out var uncastedComponent))
			{
				component = default;
				return false;
			}

			component = uncastedComponent as T;
			return true;
		}
		public bool TryGetFirstComponentImplementing(Type type, out Component component)
		{
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out component))
					return true;
			}

			component = default;
			return false;
		}

		public void GetComponentsImplementing<T>(IList<T> components) where T : class
		{
			components.Clear();
			
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(typeof(T));
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out var component))
					components.Add(component as T);
			}
		}
		public void GetComponentsImplementing(Type type, IList<Component> components)
		{
			components.Clear();
			
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out var component))
					components.Add(component);
			}
		}
		
		public T[] GetComponentsImplementing<T>() where T : class
		{
			var components = new List<T>();
			
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(typeof(T));
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out var component))
					components.Add(component as T);
			}

			return components.ToArray();
		}
		public Component[] GetComponentsImplementing(Type type)
		{
			var components = new List<Component>();
			
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_TryGetComponent(componentType, out var component))
					components.Add(component);
			}

			return components.ToArray();
		}

		public bool RemoveComponent<TComponent>() where TComponent : Component =>
			IMP_RemoveComponent(typeof(TComponent));
		public bool RemoveComponent(Type componentType)
		{
			_componentTypeRepository.ValidateTypeAsComponentType(componentType);
			return IMP_RemoveComponent(componentType);
		}
		private bool IMP_RemoveComponent(Type componentType)
		{
			_componentTypeRepository.ValidateComponentType(componentType);
			return IMP_UNCHECKED_RemoveComponent(componentType);
		}
		private bool IMP_UNCHECKED_RemoveComponent(Type componentType)
		{
			if (!IMP_UNCHECKED_TryGetComponent(componentType, out var component))
				return false;

			component.OnRemovedFromEntity();
			component.Owner = null;

			return _components.Remove(componentType);
		}

		public int RemoveAllComponentsImplementing<T>() => 
			RemoveAllComponentsImplementing(typeof(T));
		public int RemoveAllComponentsImplementing(Type type)
		{
			var removalCount = 0;
			
			var mappedComponentTypes = _componentTypeRepository.GetMappedComponentTypes(type);
			foreach (var componentType in mappedComponentTypes)
			{
				if (IMP_UNCHECKED_RemoveComponent(componentType))
					removalCount++;
			}

			return removalCount;
		}
		
		public static bool operator ==(Entity lhs, Entity rhs) => 
			Equals(lhs, rhs);
		public static bool operator !=(Entity hs, Entity rhs) => 
			!Equals(hs, rhs);

		public override bool Equals(object obj) => 
			obj is Entity other && Equals(other);
		public bool Equals(Entity other) =>
			Id == other.Id;
		
		public override int GetHashCode() => 
			Id.GetHashCode();
		
		void IDarkRiftSerializable.Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			foreach (var component in _components.Values)
			{ 
				evt.Writer.Write(_componentTypeRepository.GetComponentTypeId(component.GetType()));
				evt.Writer.Write(component);
			}
		}
		void IDarkRiftSerializable.Deserialize(DeserializeEvent evt)
		{
			while (evt.Reader.Position < evt.Reader.Length)
			{
				var componentType = _componentTypeRepository.GetComponentType(evt.Reader.ReadUInt16());
				if (!IMP_UNCHECKED_TryGetComponent(componentType, out var component))
				{
					component = _componentFactory.CreateComponent(componentType);
					AddComponent(component);
				}
				
				evt.Reader.ReadSerializableInto(ref component);
			}	
		}

		public string ToShortString() => 
			Id.ToString();
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"{nameof(Id)}={Id}");
			
			builder.AppendLine("Components=[");
			foreach (var componentType in _components.Keys)
				builder.AppendLine($"	{componentType.Name}");

			builder.AppendLine("]");
			return builder.ToString();
		}
	}

	public abstract class Component : IDarkRiftSerializable
	{
		public Entity Owner { get; internal set; }

		protected ComponentTypeRepository ComponentTypeRepository => Owner.ComponentTypeRepository;

		public virtual void OnAddedToEntity() { }
		public virtual void OnRemovedFromEntity() { }
		
		void IDarkRiftSerializable.Serialize(SerializeEvent evt) => 
			Serialize(evt);
		protected virtual void Serialize(SerializeEvent evt) { }

		void IDarkRiftSerializable.Deserialize(DeserializeEvent evt) =>
			Deserialize(evt);
		protected virtual void Deserialize(DeserializeEvent evt) { }
	}

	public class Player : IDarkRiftSerializable
	{
		public ushort Id { get; set; }
		public string PlayerName { get; set; }
		
		public Player() { }
		public Player(ushort id, string playerName)
		{
			Id = id;
			PlayerName = playerName;
		}

		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(PlayerName);
			
		}
		public void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			PlayerName = evt.Reader.ReadString();
		}
	}
}