using DarkRift;
using KAG.Shared.Transform;

namespace KAG.Shared.Gameplay
{
	public sealed class MovementComponent : Component
	{
		public float Speed;

		private PositionComponent _position;
		
		public override void OnAddedToEntity() =>
			_position = Owner.GetComponent<PositionComponent>();

		public void Move(Vector2 input) => 
			Move(input, out _);
		public void Move(Vector2 input, out Vector2 updatedPosition)
		{
			_position.Value += input.Normalized * Speed * 0.02f;
			updatedPosition = _position.Value;
		}

		protected override void Serialize(SerializeEvent evt) =>
			evt.Writer.Write(Speed);
		protected override void Deserialize(DeserializeEvent evt) =>
			Speed = evt.Reader.ReadSingle();

		public override string ToString() => 
			$"{nameof(Speed)}={Speed}";
	}
}