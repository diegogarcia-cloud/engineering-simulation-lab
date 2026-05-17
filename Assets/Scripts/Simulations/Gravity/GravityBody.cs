using EngineeringSimulationLab.Simulations.Common;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class GravityBody
    {
        public GravityBody(
            string name,
            double massKg,
            double physicalRadiusMeters,
            double visualRadiusUnity,
            Vector2d positionMeters,
            Vector2d velocityMetersPerSecond,
            Color color,
            bool isFixed = false)
        {
            Name = name;
            MassKg = massKg;
            PhysicalRadiusMeters = physicalRadiusMeters;
            VisualRadiusUnity = visualRadiusUnity;
            PositionMeters = positionMeters;
            VelocityMetersPerSecond = velocityMetersPerSecond;
            Color = color;
            IsFixed = isFixed;
        }

        public string Name { get; }
        public double MassKg { get; }
        public double PhysicalRadiusMeters { get; }
        public double VisualRadiusUnity { get; }
        public Vector2d PositionMeters { get; set; }
        public Vector2d VelocityMetersPerSecond { get; set; }
        public Vector2d AccelerationMetersPerSecondSquared { get; set; }
        public Vector2d ForceNewtons { get; set; }
        public Color Color { get; }
        public bool IsFixed { get; }
        public Transform ViewTransform { get; set; }
        public SpriteRenderer Renderer { get; set; }
        public Transform SelectionRing { get; set; }
        public OrbitTrailRenderer Trail { get; set; }
        public OrbitRingRenderer OrbitRing { get; set; }

        public double SpeedMetersPerSecond => VelocityMetersPerSecond.Magnitude;
        public double DistanceFromOriginMeters => PositionMeters.Magnitude;
    }
}
