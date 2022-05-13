using DarkRift;

namespace KAG.Shared.Network
{
	public sealed class PlayerIdentificationMessage : IDarkRiftSerializable
	{
		public string Name { get; set; }

		public void Serialize(SerializeEvent evt) => 
			evt.Writer.Write(Name);
		public void Deserialize(DeserializeEvent evt) => 
			Name = evt.Reader.ReadString();
	}
}