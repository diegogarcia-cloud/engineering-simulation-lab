using System.Collections.Generic;
using EngineeringSimulationLab.Core;
using EngineeringSimulationLab.Simulations.Common;
using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class GravitySimulationController : MonoBehaviour, ISimulationDemo
    {
        public const double DisplayMetersPerUnity = UnitFormatter.AstronomicalUnitMeters / 8d;
        public const double DefaultTimeScaleSecondsPerSecond = 86_400d * 20d;
        public const double RunawayDistanceMeters = 9e12d;

        private readonly List<GravityBody> bodies = new List<GravityBody>();
        private readonly List<GameObject> spawnedObjects = new List<GameObject>();

        private Sprite bodySprite;
        private Sprite glowSprite;
        private Sprite saturnRingSprite;
        private readonly Dictionary<string, Sprite> planetSprites = new Dictionary<string, Sprite>();
        private bool initialized;
        private bool paused;
        private GravityBody sun;
        private GravityBody selectedBody;
        private double simulationTimeSeconds;
        private double timeScaleSecondsPerSecond = DefaultTimeScaleSecondsPerSecond;
        private bool showTrails = true;
        private bool showVectors = true;
        private int trailLength = 720;
        private float vectorScale = 1f;
        private Vector2d pendingSpawnPositionMeters = new Vector2d(UnitFormatter.AstronomicalUnitMeters * 1.25d, 0d);
        private VectorOverlayRenderer vectorOverlay;
        private SpaceBackdropRenderer backdrop;
        private Sprite ringSprite;

        public string DemoName => "Gravity Simulator";
        public IReadOnlyList<GravityBody> Bodies => bodies;
        public GravityBody SelectedBody => selectedBody;
        public GravityBody Sun => sun;
        public double SimulationTimeSeconds => simulationTimeSeconds;
        public double TimeScaleSecondsPerSecond => timeScaleSecondsPerSecond;
        public bool ShowTrails => showTrails;
        public bool ShowVectors => showVectors;
        public int TrailLength => trailLength;
        public float VectorScale => vectorScale;
        public Vector2d PendingSpawnPositionMeters => pendingSpawnPositionMeters;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            bodySprite = CreateBodySprite();
            ringSprite = CreateRingSprite();
            glowSprite = CreateGlowSprite();
            saturnRingSprite = PlanetSpriteFactory.CreateSaturnRings();
            backdrop = gameObject.AddComponent<SpaceBackdropRenderer>();
            backdrop.Initialize();
            vectorOverlay = gameObject.AddComponent<VectorOverlayRenderer>();
            vectorOverlay.Initialize();
            PostFxRig.EnsureBloom(transform);
            ResetSimulation();
        }

        public void TickSimulation(float dt)
        {
            if (paused || bodies.Count == 0)
            {
                return;
            }

            double scaledSeconds = System.Math.Max(0d, dt) * timeScaleSecondsPerSecond;
            StepRealSeconds(scaledSeconds);
        }

        public void StepOneTick()
        {
            StepRealSeconds(3_600d);
        }

        public void ResetSimulation()
        {
            ClearBodies();
            simulationTimeSeconds = 0d;

            IReadOnlyList<GravityBody> preset = SolarSystemPreset.CreateBodies();
            for (int i = 0; i < preset.Count; i++)
            {
                AddBody(preset[i]);
            }

            sun = bodies[0];
            SelectBody(FindBodyByName("Earth") ?? bodies[1]);
            UpdateAllBodyViews();
        }

        public void SetPaused(bool isPaused)
        {
            paused = isPaused;
        }

        public void SetTimeScaleSecondsPerSecond(double value)
        {
            timeScaleSecondsPerSecond = System.Math.Max(1d, value);
        }

        public void SetShowTrails(bool visible)
        {
            showTrails = visible;
            foreach (GravityBody body in bodies)
            {
                body.Trail?.SetVisible(visible);
            }
        }

        public void SetTrailLength(float value)
        {
            trailLength = Mathf.Clamp(Mathf.RoundToInt(value), 60, 2400);
            foreach (GravityBody body in bodies)
            {
                body.Trail?.SetMaxPoints(trailLength);
            }
        }

        public void SetShowVectors(bool visible)
        {
            showVectors = visible;
        }

        public void SetVectorScale(float value)
        {
            vectorScale = Mathf.Clamp(value, 0.1f, 8f);
        }

        public void SelectBody(GravityBody body)
        {
            selectedBody = body ?? FindBodyByName("Earth") ?? (bodies.Count > 0 ? bodies[0] : null);
            RefreshSelectionHighlight();
        }

        public GravityBody FindBodyByName(string bodyName)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                if (bodies[i].Name == bodyName)
                {
                    return bodies[i];
                }
            }

            return null;
        }

        public GravityBody GetBodyAtWorldPosition(Vector2 worldPosition, float pickRadiusUnity)
        {
            GravityBody closest = null;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < bodies.Count; i++)
            {
                GravityBody body = bodies[i];
                if (body.ViewTransform == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(worldPosition, body.ViewTransform.position);
                float radius = Mathf.Max((float)body.VisualRadiusUnity, pickRadiusUnity);
                if (distance <= radius && distance < closestDistance)
                {
                    closest = body;
                    closestDistance = distance;
                }
            }

            return closest;
        }

        public void SetPendingSpawnWorldPosition(Vector2 worldPosition)
        {
            pendingSpawnPositionMeters = UnityToMeters(worldPosition);
        }

        public GravityBody AddCustomBody(string bodyName, double massKg, double visualRadiusUnity, Vector2d positionMeters, Vector2d manualVelocityMetersPerSecond, Color color, bool autoOrbitSun)
        {
            string resolvedName = string.IsNullOrWhiteSpace(bodyName) ? $"Custom Body {bodies.Count}" : bodyName.Trim();
            Vector2d velocity = manualVelocityMetersPerSecond;
            if (autoOrbitSun && positionMeters.Magnitude > 1d)
            {
                Vector2d tangent = new Vector2d(-positionMeters.Y, positionMeters.X).Normalized;
                velocity = tangent * GravityPhysics.CircularOrbitSpeed(SolarSystemPreset.SunMassKg, positionMeters.Magnitude);
            }

            GravityBody body = new GravityBody(
                resolvedName,
                System.Math.Max(1d, massKg),
                0d,
                System.Math.Max(0.04d, visualRadiusUnity),
                positionMeters,
                velocity,
                color);
            AddBody(body);
            SelectBody(body);
            return body;
        }

        public Vector3 MetersToUnity(Vector2d positionMeters)
        {
            return new Vector3((float)(positionMeters.X / DisplayMetersPerUnity), (float)(positionMeters.Y / DisplayMetersPerUnity), 0f);
        }

        public Vector2d UnityToMeters(Vector2 worldPosition)
        {
            return new Vector2d(worldPosition.x * DisplayMetersPerUnity, worldPosition.y * DisplayMetersPerUnity);
        }

        private void StepRealSeconds(double seconds)
        {
            if (sun == null || seconds <= 0d)
            {
                return;
            }

            const double maxStepSeconds = 3_600d;
            int subSteps = Mathf.Clamp(Mathf.CeilToInt((float)(seconds / maxStepSeconds)), 1, 720);
            double step = seconds / subSteps;
            for (int i = 0; i < subSteps; i++)
            {
                GravityPhysics.StepSunOnly(bodies, sun, step);
                simulationTimeSeconds += step;
            }

            CullRunawayBodies();
            UpdateAllBodyViews();
            vectorOverlay?.Refresh(this);
        }

        private void CullRunawayBodies()
        {
            bool selectionLost = false;
            for (int i = bodies.Count - 1; i >= 0; i--)
            {
                GravityBody body = bodies[i];
                if (body.IsFixed)
                {
                    continue;
                }

                double x = body.PositionMeters.X;
                double y = body.PositionMeters.Y;
                bool nonFinite = double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y);
                if (!nonFinite && body.DistanceFromOriginMeters <= RunawayDistanceMeters)
                {
                    continue;
                }

                if (body == selectedBody)
                {
                    selectionLost = true;
                }

                if (body.ViewTransform != null)
                {
                    spawnedObjects.Remove(body.ViewTransform.gameObject);
                    DestroyGeneratedObject(body.ViewTransform.gameObject);
                }
                if (body.Trail != null)
                {
                    spawnedObjects.Remove(body.Trail.gameObject);
                    DestroyGeneratedObject(body.Trail.gameObject);
                }
                if (body.OrbitRing != null)
                {
                    spawnedObjects.Remove(body.OrbitRing.gameObject);
                    DestroyGeneratedObject(body.OrbitRing.gameObject);
                }

                body.ViewTransform = null;
                body.Renderer = null;
                body.SelectionRing = null;
                body.Trail = null;
                body.OrbitRing = null;
                bodies.RemoveAt(i);
            }

            if (selectionLost)
            {
                GravityBody fallback = FindBodyByName("Earth");
                if (fallback == null)
                {
                    for (int i = 0; i < bodies.Count; i++)
                    {
                        if (!bodies[i].IsFixed)
                        {
                            fallback = bodies[i];
                            break;
                        }
                    }
                }
                SelectBody(fallback);
            }
        }

        private void AddBody(GravityBody body)
        {
            bodies.Add(body);

            GameObject view = new GameObject(body.Name);
            view.transform.SetParent(transform);
            SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
            renderer.sprite = ResolveSprite(body);
            renderer.color = renderer.sprite == bodySprite ? body.Color : Color.white;
            renderer.sortingOrder = body.IsFixed ? 2 : 1;

            if (body.IsFixed)
            {
                AddCoronaLayer(view.transform, "Corona Inner", 2.0f, new Color(2.6f * body.Color.r, 2.2f * body.Color.g, 1.4f * body.Color.b, 0.85f), 0);
                AddCoronaLayer(view.transform, "Corona Mid", 3.4f, new Color(2.2f * body.Color.r, 1.7f * body.Color.g, 0.9f * body.Color.b, 0.55f), -1);
                AddCoronaLayer(view.transform, "Corona Outer", 5.2f, new Color(1.8f * body.Color.r, 1.2f * body.Color.g, 0.6f * body.Color.b, 0.32f), -2);
                AddCoronaLayer(view.transform, "Corona Halo", 8.4f, new Color(1.4f * body.Color.r, 0.9f * body.Color.g, 0.45f * body.Color.b, 0.18f), -3);
                renderer.color = new Color(2.0f * body.Color.r, 1.7f * body.Color.g, 1.0f * body.Color.b, 1f);
            }

            if (body.Name == "Saturn")
            {
                GameObject rings = new GameObject("Saturn Rings");
                rings.transform.SetParent(view.transform, false);
                rings.transform.localPosition = Vector3.zero;
                rings.transform.localScale = new Vector3(2.6f, 2.6f, 1f);
                rings.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
                SpriteRenderer ringsRenderer = rings.AddComponent<SpriteRenderer>();
                ringsRenderer.sprite = saturnRingSprite;
                ringsRenderer.sortingOrder = (body.IsFixed ? 2 : 1) + 1;
            }

            GameObject ringObject = new GameObject(body.Name + " Selection Ring");
            ringObject.transform.SetParent(view.transform);
            ringObject.transform.localPosition = Vector3.zero;
            SpriteRenderer ringRenderer = ringObject.AddComponent<SpriteRenderer>();
            ringRenderer.sprite = ringSprite;
            ringRenderer.color = new Color(0.2f, 0.72f, 1f, 0.95f);
            ringRenderer.sortingOrder = 9;
            ringObject.SetActive(false);

            GameObject trailObject = new GameObject(body.Name + " Trail");
            trailObject.transform.SetParent(transform);
            OrbitTrailRenderer trail = trailObject.AddComponent<OrbitTrailRenderer>();
            trail.Configure(body.Color, trailLength);
            trail.SetVisible(showTrails && !body.IsFixed);

            OrbitRingRenderer orbitRing = null;
            GameObject orbitRingObject = null;
            if (!body.IsFixed)
            {
                orbitRingObject = new GameObject(body.Name + " Orbit Ring");
                orbitRingObject.transform.SetParent(transform);
                orbitRingObject.AddComponent<LineRenderer>();
                orbitRing = orbitRingObject.AddComponent<OrbitRingRenderer>();
                orbitRing.Configure(body.Color);
                double centralMassKg = SolarSystemPreset.SunMassKg;
                if (OrbitRingRenderer.TryComputeEllipseElements(
                        body.PositionMeters,
                        body.VelocityMetersPerSecond,
                        centralMassKg,
                        out double aMeters,
                        out double e,
                        out double argOfPerihelion))
                {
                    double aUnity = aMeters / DisplayMetersPerUnity;
                    orbitRing.SetEllipse(aUnity, e, argOfPerihelion);
                }
                else
                {
                    orbitRing.SetVisible(false);
                }
            }

            body.ViewTransform = view.transform;
            body.Renderer = renderer;
            body.SelectionRing = ringObject.transform;
            body.Trail = trail;
            body.OrbitRing = orbitRing;
            spawnedObjects.Add(view);
            spawnedObjects.Add(trailObject);
            if (orbitRingObject != null)
            {
                spawnedObjects.Add(orbitRingObject);
            }
            UpdateBodyView(body);
        }

        private void UpdateAllBodyViews()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                UpdateBodyView(bodies[i]);
            }
        }

        private void UpdateBodyView(GravityBody body)
        {
            if (body.ViewTransform == null)
            {
                return;
            }

            Vector3 unityPosition = MetersToUnity(body.PositionMeters);
            body.ViewTransform.position = unityPosition;
            float visualZoomBoost = 1f;
            if (Camera.main != null)
            {
                visualZoomBoost = Mathf.Clamp(Camera.main.orthographicSize / 36f, 1f, 4.5f);
            }

            float baseMultiplier = body.IsFixed ? 2f : 6f;
            float diameter = (float)body.VisualRadiusUnity * baseMultiplier * visualZoomBoost;
            body.ViewTransform.localScale = new Vector3(diameter, diameter, 1f);
            if (body.SelectionRing != null)
            {
                float ringScale = 1.65f;
                body.SelectionRing.localScale = new Vector3(ringScale, ringScale, 1f);
            }
            if (!body.IsFixed)
            {
                body.Trail?.Record(unityPosition);
            }

            vectorOverlay?.Refresh(this);
        }

        private void RefreshSelectionHighlight()
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                GravityBody body = bodies[i];
                if (body.Renderer == null)
                {
                    continue;
                }

                if (!body.IsFixed)
                {
                    bool usesGenericDisc = body.Renderer.sprite == bodySprite;
                    if (usesGenericDisc)
                    {
                        body.Renderer.color = body == selectedBody ? Color.Lerp(body.Color, Color.white, 0.38f) : body.Color;
                    }
                    else
                    {
                        body.Renderer.color = body == selectedBody ? new Color(1.18f, 1.18f, 1.18f, 1f) : Color.white;
                    }
                }

                body.Renderer.sortingOrder = body == selectedBody ? 5 : (body.IsFixed ? 2 : 1);
                if (body.SelectionRing != null)
                {
                    body.SelectionRing.gameObject.SetActive(body == selectedBody);
                }
            }
        }

        private void ClearBodies()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                if (spawnedObjects[i] != null)
                {
                    DestroyGeneratedObject(spawnedObjects[i]);
                }
            }

            spawnedObjects.Clear();
            bodies.Clear();
            selectedBody = null;
            sun = null;
        }

        private static void DestroyGeneratedObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private Sprite ResolveSprite(GravityBody body)
        {
            if (body.IsFixed)
            {
                return bodySprite;
            }

            if (!planetSprites.TryGetValue(body.Name, out Sprite sprite))
            {
                sprite = PlanetSpriteFactory.CreateForPlanet(body.Name, body.Color);
                if (sprite != null)
                {
                    planetSprites[body.Name] = sprite;
                }
                else
                {
                    sprite = bodySprite;
                }
            }

            return sprite;
        }

        private void AddCoronaLayer(Transform parent, string layerName, float scaleRelativeToBody, Color hdrColor, int sortingOrderDelta)
        {
            GameObject layer = new GameObject(layerName);
            layer.transform.SetParent(parent, false);
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localScale = new Vector3(scaleRelativeToBody, scaleRelativeToBody, 1f);
            SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
            renderer.sprite = glowSprite;
            renderer.color = hdrColor;
            renderer.sortingOrder = sortingOrderDelta;
        }

        private static Sprite CreateGlowSprite()
        {
            const int size = 192;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated Sun Glow";
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxRadius = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / maxRadius;
                    float falloff = Mathf.Clamp01(1f - distance);
                    float alpha = falloff * falloff * falloff;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateBodySprite()
        {
            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated SI Gravity Body Disc";
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float edge = Mathf.Clamp01(radius - distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateRingSprite()
        {
            const int size = 96;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated Selection Ring";
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 38f) / 3.2f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, ring));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
