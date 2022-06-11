using System;
using System.Collections.Generic;
using System.Linq;
using DarkRift;
using KAG.Shared.Prototype;

namespace KAG.Shared
{
	public class World
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
		
		public Entity CreateEntity(DarkRiftReader reader)
		{
			var id = reader.ReadUInt16();
			
			var entity = BYPASS_CreateEntity(id);
			reader.ReadSerializableInto(ref entity);

			OnEntityCreated(entity);
			return entity;
		}

		public Entity CreateEntity()
		{
			var entity = BYPASS_CreateEntity();
			
			OnEntityCreated(entity);
			return entity;
		}
		private Entity BYPASS_CreateEntity()
		{
			var id = GetNextEntityId();
			return IMP_CreateEntity(id);
		}
			
		public Entity CreateEntity(ushort id)
		{
			var entity = BYPASS_CreateEntity(id);
			
			OnEntityCreated(entity);
			return entity;
		}
		private Entity BYPASS_CreateEntity(ushort id)
		{
			if (_entities.ContainsKey(id))
				throw new InvalidOperationException($"An entity with `{nameof(id)}={id}` already exists.");

			return IMP_CreateEntity(id);
		}
		
		private Entity IMP_CreateEntity(ushort id)
		{
			var entity = _entityPool.Acquire(id);
			
			_entities.Add(id, entity);
			return entity;
		}
		protected virtual void OnEntityCreated(Entity entity) { }
		
		private ushort GetNextEntityId()
		{
			var id = (ushort)_entities.Count;
			if (_idsAvailableForRecycling.Count > 0)
				id = _idsAvailableForRecycling.Dequeue();

			return id;
		}

		public Entity CloneEntity(Entity entity)
		{
			var clone = BYPASS_CreateEntity();

			using var writer = DarkRiftWriter.Create();
			writer.Write(entity);

			using var reader = DarkRiftReader.CreateFromArray(writer.ToArray(), 0, writer.Length);
			reader.ReadUInt16(); // Discard the id
			reader.ReadSerializableInto(ref clone);

			
			
			OnEntityCreated(clone);
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
			
			OnEntityDestroyed(entity);
		} 
		protected virtual void OnEntityDestroyed(Entity entity) { }
	}
}