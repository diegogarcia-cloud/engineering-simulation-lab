using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public sealed class SpaceBackdropRenderer : MonoBehaviour
    {
        [SerializeField] private int distantStarCount = 1600;
        [SerializeField] private int nearStarCount = 420;
        [SerializeField] private float distantFieldRadius = 220f;
        [SerializeField] private float nearFieldRadius = 140f;
        [SerializeField] private float distantParallaxLock = 0.94f;
        [SerializeField] private float nearParallaxLock = 0.58f;

        private Sprite starSprite;
        private Transform distantLayer;
        private Transform nearLayer;
        private Camera targetCamera;
        private bool initialized;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            starSprite = CreateStarSprite();
            Random.InitState(60715);

            distantLayer = CreateLayer("Distant Stars");
            nearLayer = CreateLayer("Near Stars");

            PopulateLayer(distantLayer, distantStarCount, distantFieldRadius, 0.018f, 0.055f, 0.28f, 0.72f, -22, true);
            PopulateLayer(nearLayer, nearStarCount, nearFieldRadius, 0.06f, 0.16f, 0.65f, 1f, -20, false);

            targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
            if (targetCamera == null)
            {
                return;
            }

            Vector3 cam = targetCamera.transform.position;
            if (distantLayer != null)
            {
                distantLayer.position = new Vector3(cam.x * distantParallaxLock, cam.y * distantParallaxLock, distantLayer.position.z);
            }
            if (nearLayer != null)
            {
                nearLayer.position = new Vector3(cam.x * nearParallaxLock, cam.y * nearParallaxLock, nearLayer.position.z);
            }
        }

        private Transform CreateLayer(string layerName)
        {
            GameObject layer = new GameObject(layerName);
            layer.transform.SetParent(transform);
            layer.transform.position = new Vector3(0f, 0f, 12f);
            return layer.transform;
        }

        private void PopulateLayer(Transform parent, int count, float fieldRadius, float sizeMin, float sizeMax, float alphaMin, float alphaMax, int sortingOrder, bool tightSize)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject star = new GameObject("Star");
                star.transform.SetParent(parent);
                Vector2 point = Random.insideUnitCircle * fieldRadius;
                star.transform.localPosition = new Vector3(point.x, point.y, 0f);

                float sizeRoll = Random.value;
                float size = tightSize
                    ? Mathf.Lerp(sizeMin, sizeMax, Mathf.Pow(sizeRoll, 2.5f))
                    : Mathf.Lerp(sizeMin, sizeMax, Mathf.Pow(sizeRoll, 1.5f));
                star.transform.localScale = new Vector3(size, size, 1f);

                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = starSprite;
                float warmth = Random.Range(0.78f, 1f);
                float blue = Random.Range(0.85f, 1f);
                float red = Random.Range(0.62f, 0.95f);
                float green = Mathf.Lerp(red, blue, Random.value);
                float alpha = Mathf.Lerp(alphaMin, alphaMax, Mathf.Pow(Random.value, 1.4f));
                renderer.color = new Color(red, green * warmth, blue, alpha);
                renderer.sortingOrder = sortingOrder;
            }
        }

        private static Sprite CreateStarSprite()
        {
            const int size = 24;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Generated Background Star";
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxR = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / maxR;
                    float coreAlpha = Mathf.Clamp01(1f - distance);
                    float glow = Mathf.Pow(coreAlpha, 2.4f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, glow));
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
