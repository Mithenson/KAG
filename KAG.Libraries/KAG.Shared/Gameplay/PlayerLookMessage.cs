using DarkRift;
using KAG.Shared.Transform;

namespace KAG.Shared.Gameplay
{
	public struct PlayerLookMessage : IDarkRiftSerializable
	{
		public float Radians { get; set; }
		
		public void Serialize(SerializeEvent evt) =>
			evt.Writer.Write(Radians);
		public void Deserialize(DeserializeEvent evt) => 
			Radians = evt.Reader.ReadSingle();
	}
}