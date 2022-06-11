using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkRift;

namespace KAG.Shared
{
	public sealed class Entity : IEquatable<Entity>, IDarkRiftSerializable
	{
		public ushort Id;

		internal World World => _world;
		internal ComponentTypeRepository ComponentTypeRepository => _componentTypeRepository;

		private readonly World _world;
		private readonly ComponentTypeRepository _componentTypeRepository;
		private readonly IComponentPool _componentPool;
		
		private Dictionary<Type, Component> _components;
		
		public Entity(World world, ComponentTypeRepository componentTypeRepository, IComponentPool componentPool)
		{
			_world = world;
			_componentTypeRepository = componentTypeRepository;
			_componentPool = componentPool;

			_components = new Dictionary<Type, Component>();
		}
		internal Entity(ComponentTypeRepository componentTypeRepository)
		{
			_componentTypeRepository = componentTypeRepository;
			
			_components = new Dictionary<Type, Component>();
		}

		public TComponent AddComponent<TComponent>() where TComponent : Component
		{
			var component = _componentPool.Acquire<TComponent>();
			IMP_AddComponent(component);

			return component;
		}
		public void AddComponent(Component component)
		{
			if (component.Owner != null)
				throw new InvalidOperationException($"The `{nameof(component)}={component}` is already assigned to `{nameof(Entity)}={component.Owner.ToShortString()}`.");
			
			IMP_AddComponent(component);
		}
		private void IMP_AddComponent(Component component)
		{
			var type = component.GetType();
			if (_components.ContainsKey(type))
				throw new InvalidOperationException($"The `{nameof(Entity)}={this.ToShortString()}` already has a component of `{nameof(type)}={type}`.");

			BYPASS_AddComponent(type, component);
			component.OnAddedToEntity();
		}
		internal void BYPASS_AddComponent(Component component) => 
			BYPASS_AddComponent(component.GetType(), component);
		internal void BYPASS_AddComponent(Type type, Component component)
		{
			_components.Add(type, component);
			component.Owner = this;
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

		public bool HasAnyComponents() => 
			_components.Count > 0;

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

		public void GetAllComponents(IList<Component> components)
		{
			components.Clear();
			
			foreach (var component in _components.Values)
				components.Add(component);
		}
		public Component[] GetAllComponents() =>
			_components.Values.ToArray();
		
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
			
			IMP_DIRECT_RemoveComponent(componentType, component);
			return true;
		}
		private void IMP_DIRECT_RemoveComponent(Type componentType, Component component)
		{
			component.OnRemovedFromEntity();
			component.Owner = null;

			_components.Remove(componentType);
			_componentPool.Return(component);
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

		public int RemoveAllComponents()
		{
			var removalCount = _components.Count;

			var kvps = _components.ToArray();
			foreach (var kvp in kvps)
				IMP_DIRECT_RemoveComponent(kvp.Key, kvp.Value);
				
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
			evt.Writer.Write((short)_components.Count);
			foreach (var component in _components.Values)
			{ 
				evt.Writer.Write(_componentTypeRepository.GetComponentTypeId(component.GetType()));
				evt.Writer.Write(component);
			}
		}
		void IDarkRiftSerializable.Deserialize(DeserializeEvent evt)
		{
			var addedComponents = new List<Component>();
			
			var count = evt.Reader.ReadUInt16();
			for (var i = 0; i < count; i++)
			{
				var componentType = _componentTypeRepository.GetComponentType(evt.Reader.ReadUInt16());
				if (!IMP_UNCHECKED_TryGetComponent(componentType, out var component))
				{
					component = _componentPool.Acquire(componentType);
					BYPASS_AddComponent(component);
					
					addedComponents.Add(component);
				}
				
				evt.Reader.ReadSerializableInto(ref component);
			}

			foreach (var addedComponent in addedComponents)
				addedComponent.OnAddedToEntity();
		}

		public string ToShortString() => 
			Id.ToString();
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.AppendLine($"{nameof(Id)}={Id}");
			
			builder.AppendLine("Components=[");
			foreach (var kvp in _components)
				builder.AppendLine($"	{kvp.Key.Name}=({kvp.Value})");

			builder.AppendLine("]");
			return builder.ToString();
		}
	}
}