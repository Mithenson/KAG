using System.Collections.Generic;
using DarkRift;

namespace KAG.Shared
{
	public sealed class World
	{
		public IEnumerable<Entity> Entities => _entities.Values;

		private readonly IEntityPool _entityPool;
		
		private Dictionary<ushort, Entity> _entities;
		private Queue<ushort> _idsAvailableForRecycling;

		public World(IEntityPool entityPool)
		{
			_entityPool = entityPool;

			_entities = new Dictionary<ushort, Entity>();
			_idsAvailableForRecycling = new Queue<ushort>();
		}

		public Entity CreateEntity()
		{
			var id = GetNextEntityId();
			var entity =  _entityPool.Acquire(id);
			
			_entities.Add(id, entity);
			return entity;
		}

		public Entity CloneEntity(Entity entity)
		{
			var clone = CreateEntity();

			using var writer = DarkRiftWriter.Create();
			writer.Write(entity);

			using var reader = DarkRiftReader.CreateFromArray(writer.ToArray(), 0, writer.Length);
			reader.ReadUInt16(); // Discard the id
			reader.ReadSerializableInto(ref clone);

			return clone;
		}

		private ushort GetNextEntityId()
		{
			var id = (ushort)_entities.Count;
			if (_idsAvailableForRecycling.Count > 0)
				id = _idsAvailableForRecycling.Dequeue();

			return id;
		}

		public void Destroy(Entity entity)
		{
			_entities.Remove(entity.Id);
			_idsAvailableForRecycling.Enqueue(entity.Id);
			
			_entityPool.Return(entity);
		}
	}
}