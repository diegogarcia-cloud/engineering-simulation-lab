using System.Collections.Generic;
using EngineeringSimulationLab.Simulations.Gravity;
using UnityEngine;
using UnityEngine.UI;

namespace EngineeringSimulationLab.UI
{
    public sealed class GravityBodyLabelOverlay
    {
        private readonly GravitySimulationController simulation;
        private readonly Camera camera;
        private readonly RectTransform root;
        private readonly Dictionary<GravityBody, Text> labels = new Dictionary<GravityBody, Text>();
        private bool visible = true;

        public GravityBodyLabelOverlay(GravitySimulationController simulation, Camera camera, RectTransform root)
        {
            this.simulation = simulation;
            this.camera = camera;
            this.root = root;
        }

        public void SetVisible(bool isVisible)
        {
            visible = isVisible;
            foreach (Text label in labels.Values)
            {
                if (label != null)
                {
                    label.gameObject.SetActive(visible);
                }
            }
        }

        public void Refresh()
        {
            if (simulation == null || camera == null || root == null)
            {
                return;
            }

            PruneDeadLabels();
            EnsureLabels();
            foreach (KeyValuePair<GravityBody, Text> pair in labels)
            {
                GravityBody body = pair.Key;
                Text label = pair.Value;
                if (body.ViewTransform == null || label == null)
                {
                    continue;
                }

                Vector3 screen = camera.WorldToScreenPoint(body.ViewTransform.position);
                bool inUiColumn = screen.x < 520f || screen.x > Screen.width - 620f;
                bool inChrome = screen.y < 96f || screen.y > Screen.height - 86f;
                bool onScreen = visible && screen.z > 0f && !inUiColumn && !inChrome && screen.x > 0f && screen.y > 0f && screen.x < Screen.width && screen.y < Screen.height;
                label.gameObject.SetActive(onScreen);
                if (!onScreen)
                {
                    continue;
                }

                bool selected = body == simulation.SelectedBody;
                label.text = selected ? body.Name.ToUpperInvariant() : body.Name;
                label.fontSize = selected ? 14 : 12;
                label.color = selected ? new Color(0.28f, 0.76f, 1f) : new Color(0.86f, 0.91f, 0.98f);
                label.transform.position = screen + new Vector3(12f, 10f, 0f);
            }
        }

        private void PruneDeadLabels()
        {
            List<GravityBody> dead = null;
            foreach (GravityBody body in labels.Keys)
            {
                bool alive = false;
                for (int i = 0; i < simulation.Bodies.Count; i++)
                {
                    if (simulation.Bodies[i] == body)
                    {
                        alive = true;
                        break;
                    }
                }

                if (!alive)
                {
                    (dead ??= new List<GravityBody>()).Add(body);
                }
            }

            if (dead == null)
            {
                return;
            }

            for (int i = 0; i < dead.Count; i++)
            {
                if (labels.TryGetValue(dead[i], out Text label) && label != null)
                {
                    Object.Destroy(label.gameObject);
                }
                labels.Remove(dead[i]);
            }
        }

        private void EnsureLabels()
        {
            for (int i = 0; i < simulation.Bodies.Count; i++)
            {
                GravityBody body = simulation.Bodies[i];
                if (labels.ContainsKey(body))
                {
                    continue;
                }

                GameObject labelObject = new GameObject(body.Name + " Label");
                labelObject.transform.SetParent(root, false);
                Text label = labelObject.AddComponent<Text>();
                label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label.fontSize = body.IsFixed ? 15 : 13;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleLeft;
                label.raycastTarget = false;
                RectTransform rect = label.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(120f, 24f);
                labels.Add(body, label);
            }
        }
    }
}
