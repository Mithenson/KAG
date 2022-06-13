using DarkRift;

namespace KAG.Shared.Gameplay
{
	public struct PlayerRotationUpdateMessage : IDarkRiftSerializable
	{
		public ushort ClientId { get; set; }
		public float Radians { get; set; }
		
		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(ClientId);
			evt.Writer.Write(Radians);
		}
		public void Deserialize(DeserializeEvent evt)
		{
			ClientId = evt.Reader.ReadUInt16();
			Radians = evt.Reader.ReadSingle();
		}
	}
}