namespace EngineeringSimulationLab.Simulations.Common
{
    public static class SimulationMath2D
    {
        public const double GravitationalConstant = 6.67430e-11d;
        public const double AstronomicalUnitMeters = UnitFormatter.AstronomicalUnitMeters;

        public static double CircularOrbitSpeed(double centralMassKg, double radiusMeters)
        {
            return System.Math.Sqrt(System.Math.Max(0d, GravitationalConstant * centralMassKg / System.Math.Max(radiusMeters, 1d)));
        }
    }
}
