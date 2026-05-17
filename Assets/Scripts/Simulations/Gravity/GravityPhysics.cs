using System.Collections.Generic;
using EngineeringSimulationLab.Simulations.Common;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public static class GravityPhysics
    {
        public const double G = SimulationMath2D.GravitationalConstant;

        public static Vector2d AccelerationFrom(GravityBody attractor, Vector2d samplePositionMeters)
        {
            Vector2d offset = attractor.PositionMeters - samplePositionMeters;
            double distanceSquared = System.Math.Max(offset.SqrMagnitude, 1d);
            return offset.Normalized * (G * attractor.MassKg / distanceSquared);
        }

        public static Vector2d ForceOn(GravityBody body, Vector2d accelerationMetersPerSecondSquared)
        {
            return accelerationMetersPerSecondSquared * body.MassKg;
        }

        public static void StepSunOnly(IReadOnlyList<GravityBody> bodies, GravityBody sun, double deltaSeconds)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                GravityBody body = bodies[i];
                if (body.IsFixed)
                {
                    body.PositionMeters = Vector2d.Zero;
                    body.VelocityMetersPerSecond = Vector2d.Zero;
                    body.AccelerationMetersPerSecondSquared = Vector2d.Zero;
                    body.ForceNewtons = Vector2d.Zero;
                    continue;
                }

                Vector2d oldAcceleration = AccelerationFrom(sun, body.PositionMeters);
                Vector2d nextPosition = body.PositionMeters
                    + body.VelocityMetersPerSecond * deltaSeconds
                    + oldAcceleration * (0.5d * deltaSeconds * deltaSeconds);
                Vector2d nextAcceleration = AccelerationFrom(sun, nextPosition);

                body.PositionMeters = nextPosition;
                body.VelocityMetersPerSecond += (oldAcceleration + nextAcceleration) * (0.5d * deltaSeconds);
                body.AccelerationMetersPerSecondSquared = nextAcceleration;
                body.ForceNewtons = ForceOn(body, nextAcceleration);
            }
        }

        public static double CircularOrbitSpeed(double centralMassKg, double radiusMeters)
        {
            return SimulationMath2D.CircularOrbitSpeed(centralMassKg, radiusMeters);
        }
    }
}
