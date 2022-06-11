using DarkRift;
using KAG.Shared.Transform;

namespace KAG.Shared.Gameplay
{
	public struct PlayerMovementMessage : IDarkRiftSerializable
	{
		public ushort Id { get; set; }
		public Vector2 Input { get; set; }
		
		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(Input);
		}
		public void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			Input = evt.Reader.ReadSerializable<Vector2>();
		}
	}
}