using DarkRift;

namespace KAG.Shared.Transform
{
	public sealed class PositionComponent : Component
	{
		public Vector2 Value;

		protected override void Serialize(SerializeEvent evt) =>
			evt.Writer.Write(Value);
		protected override void Deserialize(DeserializeEvent evt) =>
			Value = evt.Reader.ReadSerializable<Vector2>();

		public override string ToString() => 
			$"{nameof(Value)}={Value}";
	}
}