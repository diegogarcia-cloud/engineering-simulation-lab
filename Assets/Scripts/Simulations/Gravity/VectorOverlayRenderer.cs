using EngineeringSimulationLab.Simulations.Common;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class VectorOverlayRenderer : MonoBehaviour
    {
        private static readonly Color VelocityColor = new Color(0.2f, 0.85f, 1f, 1f);
        private static readonly Color AccelerationColor = new Color(1f, 0.55f, 0.18f, 1f);

        private LineRenderer velocityLine;
        private LineRenderer accelerationLine;
        private SpriteRenderer velocityArrowhead;
        private SpriteRenderer accelerationArrowhead;
        private Sprite arrowheadSprite;

        public void Initialize()
        {
            arrowheadSprite = CreateArrowheadSprite();
            velocityLine = CreateLine("Velocity Vector", VelocityColor);
            accelerationLine = CreateLine("Acceleration Vector", AccelerationColor);
            velocityArrowhead = CreateArrowhead("Velocity Arrowhead", VelocityColor);
            accelerationArrowhead = CreateArrowhead("Acceleration Arrowhead", AccelerationColor);
        }

        public void Refresh(GravitySimulationController simulation)
        {
            if (simulation == null || !simulation.ShowVectors || simulation.SelectedBody == null || simulation.SelectedBody.IsFixed)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            GravityBody body = simulation.SelectedBody;
            Vector3 start = simulation.MetersToUnity(body.PositionMeters);
            Vector2d velocityDirection = body.VelocityMetersPerSecond.Normalized;
            Vector2d accelerationDirection = body.AccelerationMetersPerSecondSquared.Normalized;
            float zoomBoost = Camera.main != null
                ? Mathf.Clamp(Camera.main.orthographicSize / 24f, 1f, 5f)
                : 1f;
            float velocityLength = Mathf.Clamp((float)(body.SpeedMetersPerSecond / 30_000d), 0.6f, 4.0f) * simulation.VectorScale * zoomBoost;
            float accelerationLength = Mathf.Clamp((float)(body.AccelerationMetersPerSecondSquared.Magnitude / 0.006d), 0.5f, 3.5f) * simulation.VectorScale * zoomBoost;

            Vector3 velocityEnd = start + new Vector3((float)velocityDirection.X, (float)velocityDirection.Y, 0f) * velocityLength;
            Vector3 accelerationEnd = start + new Vector3((float)accelerationDirection.X, (float)accelerationDirection.Y, 0f) * accelerationLength;

            SetLine(velocityLine, start, velocityEnd);
            SetLine(accelerationLine, start, accelerationEnd);

            PositionArrowhead(velocityArrowhead, velocityEnd, new Vector2((float)velocityDirection.X, (float)velocityDirection.Y), velocityLength);
            PositionArrowhead(accelerationArrowhead, accelerationEnd, new Vector2((float)accelerationDirection.X, (float)accelerationDirection.Y), accelerationLength);
        }

        private void SetVisible(bool visible)
        {
            if (velocityLine != null)
            {
                velocityLine.enabled = visible;
            }
            if (accelerationLine != null)
            {
                accelerationLine.enabled = visible;
            }
            if (velocityArrowhead != null)
            {
                velocityArrowhead.enabled = visible;
            }
            if (accelerationArrowhead != null)
            {
                accelerationArrowhead.enabled = visible;
            }
        }

        private static void SetLine(LineRenderer line, Vector3 start, Vector3 end)
        {
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }

        private static void PositionArrowhead(SpriteRenderer arrowhead, Vector3 position, Vector2 direction, float lineLength)
        {
            if (arrowhead == null)
            {
                return;
            }

            arrowhead.transform.position = position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowhead.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            float size = Mathf.Clamp(lineLength * 0.22f, 0.6f, 2.4f);
            arrowhead.transform.localScale = new Vector3(size, size, 1f);
        }

        private LineRenderer CreateLine(string objectName, Color color)
        {
            GameObject lineObject = new GameObject(objectName);
            lineObject.transform.SetParent(transform);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.widthMultiplier = 0.22f;
            line.positionCount = 0;
            line.sortingOrder = 8;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.85f);
            line.material = new Material(Shader.Find("Sprites/Default"));
            return line;
        }

        private SpriteRenderer CreateArrowhead(string objectName, Color color)
        {
            GameObject head = new GameObject(objectName);
            head.transform.SetParent(transform);
            SpriteRenderer renderer = head.AddComponent<SpriteRenderer>();
            renderer.sprite = arrowheadSprite;
            renderer.color = color;
            renderer.sortingOrder = 9;
            return renderer;
        }

        private static Sprite CreateArrowheadSprite()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated Arrowhead";
            texture.wrapMode = TextureWrapMode.Clamp;

            // Triangle pointing up (+y), pivot at base-center.
            float halfBase = size * 0.42f;
            float baseY = size * 0.05f;
            float tipY = size * 0.95f;

            for (int y = 0; y < size; y++)
            {
                float t = Mathf.InverseLerp(baseY, tipY, y);
                if (t < 0f || t > 1f)
                {
                    for (int x = 0; x < size; x++)
                    {
                        texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                    }
                    continue;
                }

                float halfWidth = Mathf.Lerp(halfBase, 0f, t);
                float centerX = size * 0.5f;
                for (int x = 0; x < size; x++)
                {
                    float distance = Mathf.Abs(x - centerX);
                    float falloff = Mathf.Clamp01(halfWidth - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, falloff));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.05f), size);
        }
    }
}
