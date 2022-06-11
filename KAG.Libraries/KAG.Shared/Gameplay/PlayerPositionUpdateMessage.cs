using DarkRift;
using KAG.Shared.Transform;

namespace KAG.Shared.Gameplay
{
	public struct PlayerPositionUpdateMessage : IDarkRiftSerializable
	{
		public ushort ClientId { get; set; }
		public ushort Id { get; set; }
		public Vector2 Position { get; set; }
		
		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(Position);
		}
		public void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			Position = evt.Reader.ReadSerializable<Vector2>();
		}
	}
}