using EngineeringSimulationLab.Simulations.Common;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class OrbitRingRenderer : MonoBehaviour
    {
        private const int SegmentCount = 192;

        private LineRenderer line;

        public void Configure(Color planetColor)
        {
            EnsureLine();
            line.useWorldSpace = true;
            line.loop = true;
            line.widthMultiplier = 0.018f;
            line.sortingOrder = -4;
            line.material = new Material(Shader.Find("Sprites/Default"));
            Color faded = new Color(planetColor.r, planetColor.g, planetColor.b, 0.22f);
            line.startColor = faded;
            line.endColor = faded;
            line.positionCount = 0;
        }

        public void SetEllipse(double semiMajorAxisUnity, double eccentricity, double argumentOfPerihelionRadians)
        {
            EnsureLine();
            if (double.IsNaN(semiMajorAxisUnity) || double.IsInfinity(semiMajorAxisUnity) || semiMajorAxisUnity <= 0d)
            {
                line.positionCount = 0;
                return;
            }

            double a = semiMajorAxisUnity;
            double e = System.Math.Min(0.95d, System.Math.Max(0d, eccentricity));
            double b = a * System.Math.Sqrt(System.Math.Max(0d, 1d - e * e));
            double focusOffset = a * e;
            double cosw = System.Math.Cos(argumentOfPerihelionRadians);
            double sinw = System.Math.Sin(argumentOfPerihelionRadians);

            line.positionCount = SegmentCount;
            for (int i = 0; i < SegmentCount; i++)
            {
                double t = i * 2d * System.Math.PI / SegmentCount;
                double localX = a * System.Math.Cos(t) - focusOffset;
                double localY = b * System.Math.Sin(t);
                double worldX = localX * cosw - localY * sinw;
                double worldY = localX * sinw + localY * cosw;
                line.SetPosition(i, new Vector3((float)worldX, (float)worldY, 0f));
            }
        }

        public void SetVisible(bool visible)
        {
            EnsureLine();
            line.enabled = visible;
        }

        public static bool TryComputeEllipseElements(
            Vector2d positionMeters,
            Vector2d velocityMetersPerSecond,
            double centralMassKg,
            out double semiMajorAxisMeters,
            out double eccentricity,
            out double argumentOfPerihelionRadians)
        {
            double mu = SimulationMath2D.GravitationalConstant * centralMassKg;
            double r = positionMeters.Magnitude;
            double v2 = velocityMetersPerSecond.SqrMagnitude;

            semiMajorAxisMeters = 0d;
            eccentricity = 0d;
            argumentOfPerihelionRadians = 0d;

            if (r <= 0d || mu <= 0d)
            {
                return false;
            }

            double energy = 0.5d * v2 - mu / r;
            if (energy >= 0d)
            {
                return false;
            }

            semiMajorAxisMeters = -mu / (2d * energy);

            double rDotV = positionMeters.X * velocityMetersPerSecond.X + positionMeters.Y * velocityMetersPerSecond.Y;
            double scaleA = v2 - mu / r;
            double evX = (scaleA * positionMeters.X - rDotV * velocityMetersPerSecond.X) / mu;
            double evY = (scaleA * positionMeters.Y - rDotV * velocityMetersPerSecond.Y) / mu;
            eccentricity = System.Math.Sqrt(evX * evX + evY * evY);

            if (eccentricity > 1e-6d)
            {
                argumentOfPerihelionRadians = System.Math.Atan2(evY, evX);
            }

            return true;
        }

        private void EnsureLine()
        {
            if (line == null)
            {
                line = GetComponent<LineRenderer>();
            }
        }
    }
}
