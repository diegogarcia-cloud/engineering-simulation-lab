using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Common
{
    public readonly struct Vector2d
    {
        public static readonly Vector2d Zero = new Vector2d(0d, 0d);

        public Vector2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
        public double SqrMagnitude => X * X + Y * Y;
        public double Magnitude => System.Math.Sqrt(SqrMagnitude);

        public Vector2d Normalized
        {
            get
            {
                double magnitude = Magnitude;
                return magnitude > 0d ? this / magnitude : Zero;
            }
        }

        public Vector2 ToVector2()
        {
            return new Vector2((float)X, (float)Y);
        }

        public override string ToString()
        {
            return $"({X:0.###e+0}, {Y:0.###e+0})";
        }

        public static Vector2d FromUnity(Vector2 value)
        {
            return new Vector2d(value.x, value.y);
        }

        public static Vector2d operator +(Vector2d a, Vector2d b) => new Vector2d(a.X + b.X, a.Y + b.Y);
        public static Vector2d operator -(Vector2d a, Vector2d b) => new Vector2d(a.X - b.X, a.Y - b.Y);
        public static Vector2d operator -(Vector2d value) => new Vector2d(-value.X, -value.Y);
        public static Vector2d operator *(Vector2d value, double scalar) => new Vector2d(value.X * scalar, value.Y * scalar);
        public static Vector2d operator *(double scalar, Vector2d value) => value * scalar;
        public static Vector2d operator /(Vector2d value, double scalar) => new Vector2d(value.X / scalar, value.Y / scalar);
    }
}
