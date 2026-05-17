using System.Globalization;

namespace EngineeringSimulationLab.Simulations.Common
{
    public static class UnitFormatter
    {
        public const double AstronomicalUnitMeters = 149_597_870_700d;
        private const double DaySeconds = 86_400d;
        private const double YearSeconds = 365.25d * DaySeconds;

        public static string Distance(double meters)
        {
            double abs = System.Math.Abs(meters);
            if (abs >= 0.05d * AstronomicalUnitMeters)
            {
                return $"{meters / AstronomicalUnitMeters:0.###} AU ({meters:0.###e+0} m)";
            }

            if (abs >= 1_000d)
            {
                return $"{meters / 1_000d:0.###} km ({meters:0.###e+0} m)";
            }

            return $"{meters:0.###} m";
        }

        public static string Speed(double metersPerSecond)
        {
            double abs = System.Math.Abs(metersPerSecond);
            if (abs >= 1_000d)
            {
                return $"{metersPerSecond / 1_000d:0.###} km/s ({metersPerSecond:0.###e+0} m/s)";
            }

            return $"{metersPerSecond:0.###} m/s";
        }

        public static string Acceleration(double metersPerSecondSquared)
        {
            return $"{metersPerSecondSquared:0.###e+0} m/s^2";
        }

        public static string Force(double newtons)
        {
            return $"{newtons:0.###e+0} N";
        }

        public static string Mass(double kilograms)
        {
            return $"{kilograms:0.###e+0} kg";
        }

        public static string Time(double seconds)
        {
            double abs = System.Math.Abs(seconds);
            if (abs >= YearSeconds)
            {
                return $"{seconds / YearSeconds:0.###} years";
            }

            if (abs >= DaySeconds)
            {
                return $"{seconds / DaySeconds:0.###} days";
            }

            if (abs >= 3_600d)
            {
                return $"{seconds / 3_600d:0.###} hours";
            }

            if (abs >= 60d)
            {
                return $"{seconds / 60d:0.###} minutes";
            }

            return $"{seconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds";
        }

        public static string Vector(Vector2d vector, string unit)
        {
            return $"({vector.X:0.###e+0}, {vector.Y:0.###e+0}) {unit}";
        }

        public static (string number, string unit) DistanceSplit(double meters)
        {
            double abs = System.Math.Abs(meters);
            if (abs >= 0.05d * AstronomicalUnitMeters)
            {
                return ($"{meters / AstronomicalUnitMeters:0.000}", "AU");
            }
            if (abs >= 1_000d)
            {
                return ($"{meters / 1_000d:0.###}", "km");
            }
            return ($"{meters:0.###}", "m");
        }

        public static (string number, string unit) SpeedSplit(double metersPerSecond)
        {
            double abs = System.Math.Abs(metersPerSecond);
            if (abs >= 1_000d)
            {
                return ($"{metersPerSecond / 1_000d:0.000}", "km/s");
            }
            return ($"{metersPerSecond:0.000}", "m/s");
        }

        public static (string number, string unit) AccelerationSplit(double metersPerSecondSquared)
        {
            return ($"{metersPerSecondSquared:0.000e+0}", "m/s²");
        }

        public static (string number, string unit) ForceSplit(double newtons)
        {
            return ($"{newtons:0.000e+0}", "N");
        }

        public static (string number, string unit) MassSplit(double kilograms)
        {
            return ($"{kilograms:0.000e+0}", "kg");
        }

        public static (string number, string unit) TimeSplit(double seconds)
        {
            double abs = System.Math.Abs(seconds);
            if (abs >= YearSeconds)
            {
                return ($"{seconds / YearSeconds:0.###}", "years");
            }
            if (abs >= DaySeconds)
            {
                return ($"{seconds / DaySeconds:0.###}", "days");
            }
            if (abs >= 3_600d)
            {
                return ($"{seconds / 3_600d:0.###}", "hours");
            }
            if (abs >= 60d)
            {
                return ($"{seconds / 60d:0.###}", "min");
            }
            return ($"{seconds.ToString("0.###", CultureInfo.InvariantCulture)}", "s");
        }
    }
}
