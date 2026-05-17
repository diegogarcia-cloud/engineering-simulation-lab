using EngineeringSimulationLab.Simulations.Common;
using EngineeringSimulationLab.Simulations.Gravity;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringSimulationLab.UI
{
    public sealed class BodySpawnerPanel
    {
        private readonly GravitySimulationController simulation;
        private readonly InputField nameInput;
        private readonly InputField massInput;
        private readonly InputField visualRadiusInput;
        private readonly InputField velocityXInput;
        private readonly InputField velocityYInput;
        private readonly Toggle autoOrbitToggle;
        private readonly Dropdown massPresetDropdown;
        private readonly Text spawnPointText;

        public BodySpawnerPanel(
            GravitySimulationController simulation,
            InputField nameInput,
            InputField massInput,
            InputField visualRadiusInput,
            InputField velocityXInput,
            InputField velocityYInput,
            Toggle autoOrbitToggle,
            Dropdown massPresetDropdown,
            Text spawnPointText)
        {
            this.simulation = simulation;
            this.nameInput = nameInput;
            this.massInput = massInput;
            this.visualRadiusInput = visualRadiusInput;
            this.velocityXInput = velocityXInput;
            this.velocityYInput = velocityYInput;
            this.autoOrbitToggle = autoOrbitToggle;
            this.massPresetDropdown = massPresetDropdown;
            this.spawnPointText = spawnPointText;
        }

        public void ApplyMassPreset(int index)
        {
            double value = index switch
            {
                1 => 5.97237e24d,
                2 => 7.342e22d,
                3 => 1.0e12d,
                4 => 1.8982e27d,
                _ => ParseDouble(massInput.text, 5.97237e24d)
            };

            massInput.text = value.ToString("0.###e+0");
        }

        public void RefreshSpawnPoint()
        {
            if (spawnPointText != null && simulation != null)
            {
                spawnPointText.text = $"Spawn point: {UnitFormatter.Vector(simulation.PendingSpawnPositionMeters, "m")}";
            }
        }

        public void AddBody()
        {
            if (simulation == null)
            {
                return;
            }

            double mass = ParseDouble(massInput.text, 5.97237e24d);
            double visualRadius = ParseDouble(visualRadiusInput.text, 0.15d);
            Vector2d velocity = new Vector2d(ParseDouble(velocityXInput.text, 0d), ParseDouble(velocityYInput.text, 0d));
            Color color = Color.HSVToRGB(Mathf.Repeat(simulation.Bodies.Count * 0.17f, 1f), 0.72f, 1f);
            simulation.AddCustomBody(nameInput.text, mass, visualRadius, simulation.PendingSpawnPositionMeters, velocity, color, autoOrbitToggle.isOn);
        }

        private static double ParseDouble(string text, double fallback)
        {
            return double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value)
                ? value
                : fallback;
        }
    }
}
