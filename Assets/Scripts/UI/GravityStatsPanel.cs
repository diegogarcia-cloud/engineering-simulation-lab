using EngineeringSimulationLab.Simulations.Common;
using EngineeringSimulationLab.Simulations.Gravity;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringSimulationLab.UI
{
    public sealed class GravityStatsPanel
    {
        private const int LabelColumnWidth = 20;
        private const int NumberColumnWidth = 13;
        private const string LabelColor = "#9FB3C8";
        private const string UnitColor = "#7B95B5";

        private readonly Text titleText;
        private readonly Text statsText;
        private bool fontApplied;

        public GravityStatsPanel(Text titleText, Text statsText)
        {
            this.titleText = titleText;
            this.statsText = statsText;
        }

        public void Refresh(GravitySimulationController simulation)
        {
            if (statsText == null || simulation == null)
            {
                return;
            }

            EnsureMonospaceFont();

            GravityBody body = simulation.SelectedBody ?? simulation.FindBodyByName("Earth");
            if (body == null)
            {
                if (titleText != null)
                {
                    titleText.text = "SELECTED BODY";
                }
                statsText.text = "No body selected";
                return;
            }

            if (titleText != null)
            {
                titleText.text = $"SELECTED BODY: {body.Name.ToUpperInvariant()}";
            }

            double distance = body.DistanceFromOriginMeters;
            double period = distance > 0d
                ? 2d * System.Math.PI * System.Math.Sqrt(System.Math.Pow(distance, 3d) / (GravityPhysics.G * SolarSystemPreset.SunMassKg))
                : 0d;

            var distSplit = UnitFormatter.DistanceSplit(distance);
            var speedSplit = UnitFormatter.SpeedSplit(body.SpeedMetersPerSecond);
            var accelSplit = UnitFormatter.AccelerationSplit(body.AccelerationMetersPerSecondSquared.Magnitude);
            var forceSplit = UnitFormatter.ForceSplit(body.ForceNewtons.Magnitude);
            var massSplit = UnitFormatter.MassSplit(body.MassKg);
            var radiusSplit = UnitFormatter.DistanceSplit(body.PhysicalRadiusMeters);
            var periodSplit = UnitFormatter.TimeSplit(period);

            statsText.text =
                Row("Distance to Sun", distSplit.number, distSplit.unit) +
                Row("Speed", speedSplit.number, speedSplit.unit) +
                RowVector("Velocity vector", body.VelocityMetersPerSecond, "m/s") +
                Row("|Acceleration|", accelSplit.number, accelSplit.unit) +
                RowVector("Acceleration vec", body.AccelerationMetersPerSecondSquared, "m/s²") +
                Row("|Gravity force|", forceSplit.number, forceSplit.unit) +
                Row("Mass", massSplit.number, massSplit.unit) +
                Row("Radius", radiusSplit.number, radiusSplit.unit) +
                Row("Orbital period", periodSplit.number, periodSplit.unit) +
                RowVector("Position vector", body.PositionMeters, "m");
        }

        private void EnsureMonospaceFont()
        {
            if (fontApplied || statsText == null)
            {
                return;
            }

            Font mono = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Courier New", "Menlo", "Liberation Mono" }, 12);
            if (mono != null)
            {
                statsText.font = mono;
                statsText.fontSize = 12;
                statsText.lineSpacing = 1.08f;
            }
            fontApplied = true;
        }

        private static string Row(string label, string number, string unit)
        {
            string paddedLabel = label.PadRight(LabelColumnWidth);
            string paddedNumber = number.PadLeft(NumberColumnWidth);
            return $"<color={LabelColor}>{paddedLabel}</color>{paddedNumber} <color={UnitColor}>{unit}</color>\n";
        }

        private static string RowVector(string label, Vector2d vector, string unit)
        {
            string paddedLabel = label.PadRight(LabelColumnWidth);
            string value = $"({vector.X,11:+0.000e+0;-0.000e+0;0.000e+0}, {vector.Y,11:+0.000e+0;-0.000e+0;0.000e+0})";
            return $"<color={LabelColor}>{paddedLabel}</color>{value} <color={UnitColor}>{unit}</color>\n";
        }
    }
}
