using DarkRift;
using KAG.Shared.Utilities;

namespace KAG.Shared.Transform
{
	public sealed class RotationComponent : Component
	{
		public float Radians;
		
		protected override void Serialize(SerializeEvent evt) =>
			evt.Writer.Write(Radians);
		protected override void Deserialize(DeserializeEvent evt) => 
			Radians = evt.Reader.ReadSingle();

		public override string ToString() => 
			$"{nameof(Radians)}={Radians * ExtMath.Rad2Deg}";
	}
}