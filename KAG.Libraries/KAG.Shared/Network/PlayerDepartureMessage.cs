using DarkRift;

namespace KAG.Shared.Network
{
	public struct PlayerDepartureMessage : IDarkRiftSerializable
	{
		public ushort Id { get; set; }
		
		public void Serialize(SerializeEvent evt) => 
			evt.Writer.Write(Id);
		public void Deserialize(DeserializeEvent evt) => 
			Id = evt.Reader.ReadUInt16();
	}
}