using DarkRift;
using Newtonsoft.Json.Serialization;

namespace KAG.Shared.Prototype
{
	public sealed class IdentityComponent : Component
	{
		public Identity Value;

		protected override void Serialize(SerializeEvent evt) => 
			evt.Writer.Write((uint)Value);
		protected override void Deserialize(DeserializeEvent evt) =>
			Value = (Identity)evt.Reader.ReadUInt32();
		
		public override string ToString() => 
			$"{nameof(Value)}={Value}";
	}
}