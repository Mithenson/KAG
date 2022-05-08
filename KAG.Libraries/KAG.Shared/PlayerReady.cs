using DarkRift;

namespace KAG.Shared
{
	public class PlayerReady : IDarkRiftSerializable
	{
		public ushort Id { get; set; }
		public bool Value { get; set; }

		public PlayerReady() { }
		public PlayerReady(ushort id, bool value)
		{
			Id = id;
			Value = value;
		}
		
		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(Value);
			
		}
		public void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			Value = evt.Reader.ReadBoolean();
		}
	}
}