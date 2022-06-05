using System;
using DarkRift;
using KAG.Shared.Utilities;

namespace KAG.Shared.Transform
{
	public struct Vector2 : IDarkRiftSerializable, IEquatable<Vector2>
	{
		private const double Epsilon = 9.99999943962493E-11;

		public static readonly Vector2 Zero = new Vector2(0.0f, 0.0f);
		public static readonly Vector2 One = new Vector2(1.0f, 1.0f);
		public static readonly Vector2 Right = new Vector2(1.0f, 0.0f);
		public static readonly Vector2 Left = new Vector2(-1.0f, 0.0f);
		public static readonly Vector2 Up = new Vector2(0.0f, 1.0f);
		public static readonly Vector2 Down = new Vector2(0.0f, -1.0f);

		public float SqrMagnitude => (float)Math.Sqrt(Math.Pow(X, 2.0d) + Math.Pow(Y, 2.0d));
		public float Magnitude => (float)(Math.Pow(X, 2.0d) + Math.Pow(Y, 2.0d));
		
		public float X;
		public float Y;

		public Vector2(float x, float y)
		{
			X = x;
			Y = y;
		}
		
		public static Vector2 Lerp(Vector2 lhs, Vector2 rhs, float ratio)
		{
			ratio = ExtMath.Clamp01(ratio);
			return LerpUnclamped(lhs, rhs, ratio);
		}
		public static Vector2 LerpUnclamped(Vector2 lhs, Vector2 rhs, float ratio) =>
			new Vector2(ExtMath.LerpUnclamped(lhs.X, rhs.X, ratio), ExtMath.LerpUnclamped(lhs.X, rhs.X, ratio));

		public float Distance(Vector2 lhs, Vector2 rhs) => 
			(rhs - lhs).Magnitude;
		public float DistanceSqr(Vector2 lhs, Vector2 rhs) => 
			(rhs - lhs).SqrMagnitude;
		
		public static Vector2 operator +(Vector2 lhs, Vector2 rhs) => 
			new Vector2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		
		public static Vector2 operator -(Vector2 lhs, Vector2 rhs) => 
			new Vector2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		
		public static Vector2 operator *(Vector2 lhs, Vector2 rhs) => 
			new Vector2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		
		public static Vector2 operator /(Vector2 lhs, Vector2 rhs) => 
			new Vector2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		
		public static Vector2 operator -(Vector2 value) => 
			new Vector2(-value.X, -value.Y);
		
		public static Vector2 operator *(Vector2 lhs, float rhs) => 
			new Vector2(lhs.X * rhs, lhs.Y * rhs);
		
		public static Vector2 operator *(float lhs, Vector2 rhs) => 
			new Vector2(lhs * rhs.X, lhs * rhs.Y);
		
		public static Vector2 operator /(Vector2 lhs, float rhs) => 
			new Vector2(lhs.X / rhs, lhs.Y / rhs);
		
		public static bool operator ==(Vector2 lhs, Vector2 rhs) => lhs.Equals(rhs);
		public static bool operator !=(Vector2 lhs, Vector2 rhs) => !lhs.Equals(rhs);

		public override bool Equals(object obj) => 
			obj is Vector2 other && Equals(other);
		public bool Equals(Vector2 other)
		{
			var xDiff = other.X - X;
			var yDiff = other.Y - Y;

			return Math.Pow(xDiff, 2.0d) + Math.Pow(yDiff, 2.0d) < Epsilon;
		}
		
		void IDarkRiftSerializable.Serialize(SerializeEvent evt)
		{
			evt.Writer.Write(X);
			evt.Writer.Write(Y);
		}
		void IDarkRiftSerializable.Deserialize(DeserializeEvent evt)
		{
			X = evt.Reader.ReadSingle();
			Y = evt.Reader.ReadSingle();
		}
		
		public override int GetHashCode()
		{
			unchecked
			{
				return (X.GetHashCode() * 397) ^ Y.GetHashCode();
			}
		}

		public override string ToString() => 
			$"({X}, {Y})";
	}
}