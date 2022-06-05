using DarkRift;

namespace KAG.Shared
{
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
}