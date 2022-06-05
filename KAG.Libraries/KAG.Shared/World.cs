using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using KAG.Shared.Prototype;

namespace KAG.Shared
{
	public sealed class World
	{
		public IEnumerable<Entity> Entities => _entities.Values;

		private readonly IEntityPool _entityPool;
		private readonly PrototypeRepository _prototypeRepository;
		
		private Dictionary<ushort, Entity> _entities;
		private Queue<ushort> _idsAvailableForRecycling;

		public World(IEntityPool entityPool, PrototypeRepository prototypeRepository)
		{
			_entityPool = entityPool;
			_prototypeRepository = prototypeRepository;

			_entities = new Dictionary<ushort, Entity>();
			_idsAvailableForRecycling = new Queue<ushort>();
		}

		public Entity CreateEntity(Identity identity)
		{
			if (!_prototypeRepository.TryGetPrototypeEntity(identity, out var prototypeEntity))
				throw new InvalidOperationException($"No prototype exists for `{nameof(identity)}={identity}`.");

			return CloneEntity(prototypeEntity);
		}

		public Entity CreateEntity()
		{
			var id = GetNextEntityId();
			return IMP_CreateEntity(id);
		}
		public Entity CreateEntity(ushort id)
		{
			if (_entities.ContainsKey(id))
				throw new InvalidOperationException($"An entity with `{nameof(id)}={id}` already exists.");

			return IMP_CreateEntity(id);
		}
		private Entity IMP_CreateEntity(ushort id)
		{
			var entity =  _entityPool.Acquire(id);
			
			_entities.Add(id, entity);
			return entity;
		}
		
		private ushort GetNextEntityId()
		{
			var id = (ushort)_entities.Count;
			if (_idsAvailableForRecycling.Count > 0)
				id = _idsAvailableForRecycling.Dequeue();

			return id;
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

		public void Clear()
		{
			var entities = _entities.Values.ToArray();
			for (var i = 0; i < entities.Length; i++)
				Destroy(entities[i]);
		}
		
		public void Destroy(Entity entity)
		{
			_entities.Remove(entity.Id);
			_idsAvailableForRecycling.Enqueue(entity.Id);
			
			_entityPool.Return(entity);
		}
	}
}