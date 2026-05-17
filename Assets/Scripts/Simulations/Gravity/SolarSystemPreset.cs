using System.Collections.Generic;
using EngineeringSimulationLab.Simulations.Common;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public static class SolarSystemPreset
    {
        public const double SunMassKg = 1.98847e30d;
        public const double SunRadiusMeters = 695_700_000d;

        public static IReadOnlyList<GravityBody> CreateBodies()
        {
            List<GravityBody> bodies = new List<GravityBody>
            {
                new GravityBody("Sun", SunMassKg, SunRadiusMeters, 0.9d, Vector2d.Zero, Vector2d.Zero, new Color(1f, 0.77f, 0.22f), true)
            };

            AddPlanet(bodies, "Mercury", 3.3011e23d, 2_439_700d, 0.387098d, 0.16d, 225d, new Color(0.68f, 0.64f, 0.58f));
            AddPlanet(bodies, "Venus", 4.8675e24d, 6_051_800d, 0.723332d, 0.22d, 155d, new Color(0.95f, 0.72f, 0.38f));
            AddPlanet(bodies, "Earth", 5.97237e24d, 6_371_000d, 1.000000d, 0.24d, 285d, new Color(0.28f, 0.58f, 1f));
            AddPlanet(bodies, "Mars", 6.4171e23d, 3_389_500d, 1.523679d, 0.19d, 25d, new Color(0.95f, 0.36f, 0.22f));
            AddPlanet(bodies, "Jupiter", 1.8982e27d, 69_911_000d, 5.2044d, 0.48d, 335d, new Color(0.92f, 0.68f, 0.48f));
            AddPlanet(bodies, "Saturn", 5.6834e26d, 58_232_000d, 9.5826d, 0.43d, 195d, new Color(0.93f, 0.82f, 0.52f));
            AddPlanet(bodies, "Uranus", 8.6810e25d, 25_362_000d, 19.2184d, 0.34d, 65d, new Color(0.49f, 0.9f, 0.92f));
            AddPlanet(bodies, "Neptune", 1.02413e26d, 24_622_000d, 30.11d, 0.34d, 118d, new Color(0.32f, 0.48f, 1f));

            return bodies;
        }

        private static void AddPlanet(List<GravityBody> bodies, string name, double massKg, double physicalRadiusMeters, double semiMajorAxisAu, double visualRadiusUnity, double angleDegrees, Color color)
        {
            double radiusMeters = semiMajorAxisAu * UnitFormatter.AstronomicalUnitMeters;
            double speed = GravityPhysics.CircularOrbitSpeed(SunMassKg, radiusMeters);
            double angle = angleDegrees * System.Math.PI / 180d;
            Vector2d radial = new Vector2d(System.Math.Cos(angle), System.Math.Sin(angle));
            Vector2d tangent = new Vector2d(-radial.Y, radial.X);
            bodies.Add(new GravityBody(
                name,
                massKg,
                physicalRadiusMeters,
                visualRadiusUnity,
                radial * radiusMeters,
                tangent * speed,
                color));
        }
    }
}
