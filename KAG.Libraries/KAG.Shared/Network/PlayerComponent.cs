using DarkRift;

namespace KAG.Shared.Network
{
	public sealed class PlayerComponent : Component
	{
		public ushort Id;
		public string Name;

		protected override void Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(Id);
			evt.Writer.Write(Name ?? string.Empty);
		}
		protected override void Deserialize(DeserializeEvent evt)
		{
			Id = evt.Reader.ReadUInt16();
			Name = evt.Reader.ReadString();
		}
		
		public override string ToString() => 
			$"{nameof(Id)}={Id}, {nameof(Name)}={Name}";
	}
}