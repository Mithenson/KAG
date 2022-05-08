using DarkRift;

namespace KAG.Shared
{
	public class Player : IDarkRiftSerializable
	{
		public ushort Id { get; set; }
		public string PlayerName { get; set; }
		
		public Player() { }
		public Player(ushort id, string playerName)
		{
			Id = id;
			PlayerName = playerName;
		}

		public void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(PlayerName);
			
		}
		public void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			PlayerName = evt.Reader.ReadString();
		}
	}
}