using System.Collections.Generic;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class OrbitTrailRenderer : MonoBehaviour
    {
        [SerializeField] private int maxPoints = 720;
        [SerializeField] private float minDistanceUnity = 0.025f;

        private readonly List<Vector3> points = new List<Vector3>();
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.widthMultiplier = 0.025f;
            lineRenderer.positionCount = 0;
            lineRenderer.sortingOrder = -3;
        }

        public void Configure(Color color, int trailLength)
        {
            maxPoints = Mathf.Max(2, trailLength);
            EnsureLineRenderer();
            Color start = new Color(color.r, color.g, color.b, 0.38f);
            Color end = new Color(color.r, color.g, color.b, 0.02f);
            lineRenderer.startColor = start;
            lineRenderer.endColor = end;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        public void SetVisible(bool visible)
        {
            EnsureLineRenderer();
            lineRenderer.enabled = visible;
        }

        public void SetMaxPoints(int trailLength)
        {
            maxPoints = Mathf.Max(2, trailLength);
            while (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }

            ApplyPoints();
        }

        public void Record(Vector3 position)
        {
            EnsureLineRenderer();
            if (points.Count > 0 && Vector3.Distance(points[points.Count - 1], position) < minDistanceUnity)
            {
                return;
            }

            points.Add(position);
            while (points.Count > maxPoints)
            {
                points.RemoveAt(0);
            }

            ApplyPoints();
        }

        public void Clear()
        {
            points.Clear();
            ApplyPoints();
        }

        private void ApplyPoints()
        {
            if (lineRenderer == null)
            {
                return;
            }

            lineRenderer.positionCount = points.Count;
            if (points.Count > 0)
            {
                lineRenderer.SetPositions(points.ToArray());
            }
        }

        private void EnsureLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }
        }
    }
}
