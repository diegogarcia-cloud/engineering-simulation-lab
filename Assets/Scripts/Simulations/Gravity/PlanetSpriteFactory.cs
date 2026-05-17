using UnityEngine;

namespace EngineeringSimulationLab.Simulations.Gravity
{
    public static class PlanetSpriteFactory
    {
        private const int DefaultSize = 128;

        public static Sprite CreateForPlanet(string planetName, Color baseColor)
        {
            switch (planetName)
            {
                case "Earth":
                    return CreateEarth(DefaultSize);
                case "Mars":
                    return CreateMars(DefaultSize);
                case "Jupiter":
                    return CreateJupiter(DefaultSize);
                case "Saturn":
                    return CreateSaturn(DefaultSize);
                case "Mercury":
                    return CreateMercury(DefaultSize);
                case "Venus":
                    return CreateVenus(DefaultSize);
                case "Uranus":
                    return CreateUranus(DefaultSize);
                case "Neptune":
                    return CreateNeptune(DefaultSize);
                default:
                    return CreateGeneric(DefaultSize, baseColor);
            }
        }

        public static Sprite CreateSaturnRings()
        {
            const int width = 320;
            const int height = 120;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = "Saturn Rings";
            texture.wrapMode = TextureWrapMode.Clamp;

            float centerX = (width - 1) * 0.5f;
            float centerY = (height - 1) * 0.5f;
            float maxRadiusX = width * 0.5f;
            float maxRadiusY = height * 0.5f;
            float innerNormalized = 0.42f;
            float outerNormalized = 0.98f;
            Color outer = new Color(0.94f, 0.86f, 0.62f, 1f);
            Color mid = new Color(0.78f, 0.66f, 0.42f, 1f);
            Color inner = new Color(0.58f, 0.46f, 0.28f, 1f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x - centerX) / maxRadiusX;
                    float ny = (y - centerY) / maxRadiusY;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    if (r < innerNormalized || r > outerNormalized)
                    {
                        texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                        continue;
                    }

                    float band = Mathf.InverseLerp(innerNormalized, outerNormalized, r);
                    Color tint = band < 0.5f ? Color.Lerp(inner, mid, band * 2f) : Color.Lerp(mid, outer, (band - 0.5f) * 2f);
                    float ridge = 0.7f + 0.3f * Mathf.Sin(band * Mathf.PI * 8f);
                    tint.r *= ridge;
                    tint.g *= ridge;
                    tint.b *= ridge;

                    float edgeFade = Mathf.SmoothStep(0f, 1f, Mathf.Min((r - innerNormalized) * 10f, (outerNormalized - r) * 8f));
                    tint.a = Mathf.Clamp01(edgeFade * 0.95f);
                    texture.SetPixel(x, y, tint);
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), width);
        }

        private static Sprite CreateGeneric(int size, Color baseColor)
        {
            Texture2D texture = NewTexture(size, "Generic Planet");
            FillSphereGradient(texture, size, baseColor, 0.65f, 1.1f);
            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateEarth(int size)
        {
            Color ocean = new Color(0.18f, 0.42f, 0.85f);
            Color deep = new Color(0.08f, 0.22f, 0.55f);
            Color land = new Color(0.32f, 0.62f, 0.32f);
            Color landDark = new Color(0.22f, 0.45f, 0.22f);
            Color clouds = new Color(0.96f, 0.97f, 0.99f);
            Texture2D texture = NewTexture(size, "Earth");
            FillSphereGradient(texture, size, ocean, 0.55f, 1.05f);

            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    float n = Mathf.PerlinNoise(x * 0.045f, y * 0.045f);
                    float n2 = Mathf.PerlinNoise(x * 0.11f + 31f, y * 0.11f + 17f);
                    Color current = texture.GetPixel(x, y);
                    if (n > 0.58f)
                    {
                        Color landTint = Color.Lerp(landDark, land, n2);
                        current = Color.Lerp(current, landTint, 0.92f);
                    }
                    else if (n > 0.42f)
                    {
                        current = Color.Lerp(current, deep, 0.35f);
                    }

                    float cloudMask = Mathf.PerlinNoise(x * 0.05f + 91f, y * 0.05f + 73f);
                    if (cloudMask > 0.72f)
                    {
                        current = Color.Lerp(current, clouds, (cloudMask - 0.72f) * 2.6f);
                    }

                    texture.SetPixel(x, y, current);
                }
            }

            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateMars(int size)
        {
            Color rust = new Color(0.78f, 0.36f, 0.22f);
            Color rustDark = new Color(0.46f, 0.2f, 0.13f);
            Color ice = new Color(0.92f, 0.88f, 0.84f);
            Texture2D texture = NewTexture(size, "Mars");
            FillSphereGradient(texture, size, rust, 0.5f, 1.05f);

            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    float n = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);
                    Color current = texture.GetPixel(x, y);
                    current = Color.Lerp(current, rustDark, (n - 0.4f) * 0.8f);

                    float capDistance = Mathf.Abs(y - c) / maxR;
                    if (capDistance > 0.82f)
                    {
                        float capAlpha = (capDistance - 0.82f) * 5f;
                        current = Color.Lerp(current, ice, Mathf.Clamp01(capAlpha));
                    }

                    texture.SetPixel(x, y, current);
                }
            }

            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateJupiter(int size)
        {
            Color baseTan = new Color(0.86f, 0.74f, 0.55f);
            Color band1 = new Color(0.78f, 0.6f, 0.34f);
            Color band2 = new Color(0.95f, 0.86f, 0.7f);
            Color spot = new Color(0.78f, 0.32f, 0.22f);
            Texture2D texture = NewTexture(size, "Jupiter");
            FillSphereGradient(texture, size, baseTan, 0.6f, 1.05f);

            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                float yNorm = (y - c) / maxR;
                float wave = Mathf.Sin(yNorm * 9f);
                float wave2 = Mathf.Sin(yNorm * 22f + 1.3f) * 0.4f;
                Color bandTint = wave + wave2 > 0f ? band2 : band1;
                float strength = Mathf.Abs(wave + wave2 * 0.4f) * 0.55f;
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    float swirl = Mathf.PerlinNoise(x * 0.08f, y * 0.04f);
                    Color current = texture.GetPixel(x, y);
                    Color mixed = Color.Lerp(current, bandTint, strength + swirl * 0.18f);

                    float spotDX = (x - c) / maxR;
                    float spotDY = (y - c - maxR * 0.18f) / maxR;
                    float spotR = Mathf.Sqrt(spotDX * spotDX * 1.8f + spotDY * spotDY * 4f);
                    if (spotR < 0.22f)
                    {
                        mixed = Color.Lerp(mixed, spot, (0.22f - spotR) * 4f);
                    }

                    texture.SetPixel(x, y, mixed);
                }
            }

            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateSaturn(int size)
        {
            Color baseCream = new Color(0.96f, 0.86f, 0.6f);
            Color bandDark = new Color(0.78f, 0.68f, 0.42f);
            Texture2D texture = NewTexture(size, "Saturn");
            FillSphereGradient(texture, size, baseCream, 0.62f, 1.06f);

            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                float yNorm = (y - c) / maxR;
                float wave = Mathf.Sin(yNorm * 7f) * 0.5f + Mathf.Sin(yNorm * 14f + 0.7f) * 0.2f;
                float strength = Mathf.Clamp01(Mathf.Abs(wave) * 0.6f);
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    Color current = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, Color.Lerp(current, bandDark, strength));
                }
            }

            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateMercury(int size)
        {
            Color gray = new Color(0.7f, 0.65f, 0.58f);
            Texture2D texture = NewTexture(size, "Mercury");
            FillSphereGradient(texture, size, gray, 0.55f, 1.05f);
            CratersOverlay(texture, size, 0.18f);
            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateVenus(int size)
        {
            Color cream = new Color(0.97f, 0.78f, 0.4f);
            Texture2D texture = NewTexture(size, "Venus");
            FillSphereGradient(texture, size, cream, 0.65f, 1.04f);
            CloudSwirl(texture, size, new Color(0.9f, 0.7f, 0.32f), 0.55f);
            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateUranus(int size)
        {
            Color paleCyan = new Color(0.55f, 0.92f, 0.94f);
            Texture2D texture = NewTexture(size, "Uranus");
            FillSphereGradient(texture, size, paleCyan, 0.7f, 1.04f);
            CloudSwirl(texture, size, new Color(0.4f, 0.78f, 0.84f), 0.25f);
            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static Sprite CreateNeptune(int size)
        {
            Color deepBlue = new Color(0.32f, 0.48f, 1f);
            Texture2D texture = NewTexture(size, "Neptune");
            FillSphereGradient(texture, size, deepBlue, 0.55f, 1.05f);
            CloudSwirl(texture, size, new Color(0.16f, 0.28f, 0.78f), 0.35f);
            texture.Apply();
            return BakeSprite(texture, size);
        }

        private static void FillSphereGradient(Texture2D texture, int size, Color baseColor, float darkenAtEdge, float highlightBoost)
        {
            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            Vector2 light = new Vector2(c - size * 0.18f, c + size * 0.16f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
                        continue;
                    }

                    float edge = 1f - (d / maxR);
                    float shade = Mathf.Lerp(darkenAtEdge, highlightBoost, edge);
                    float lightDist = Vector2.Distance(new Vector2(x, y), light) / maxR;
                    float highlight = Mathf.Clamp01(1f - lightDist) * 0.25f;
                    Color tint = new Color(
                        Mathf.Clamp01(baseColor.r * shade + highlight),
                        Mathf.Clamp01(baseColor.g * shade + highlight),
                        Mathf.Clamp01(baseColor.b * shade + highlight),
                        1f);
                    float aaEdge = Mathf.Clamp01((maxR - d) * 1.5f);
                    tint.a *= aaEdge;
                    texture.SetPixel(x, y, tint);
                }
            }
        }

        private static void CratersOverlay(Texture2D texture, int size, float intensity)
        {
            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    float n = Mathf.PerlinNoise(x * 0.18f, y * 0.18f);
                    float n2 = Mathf.PerlinNoise(x * 0.45f + 9f, y * 0.45f + 5f);
                    Color current = texture.GetPixel(x, y);
                    float crater = Mathf.Clamp01(((n + n2) * 0.5f - 0.55f) * 3f);
                    Color dark = new Color(current.r * 0.55f, current.g * 0.55f, current.b * 0.55f, current.a);
                    texture.SetPixel(x, y, Color.Lerp(current, dark, crater * intensity));
                }
            }
        }

        private static void CloudSwirl(Texture2D texture, int size, Color tint, float intensity)
        {
            float c = (size - 1) * 0.5f;
            float maxR = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - c;
                    float dy = y - c;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d > maxR)
                    {
                        continue;
                    }

                    float n = Mathf.PerlinNoise(x * 0.06f + 22f, y * 0.02f + 11f);
                    Color current = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, Color.Lerp(current, tint, n * intensity));
                }
            }
        }

        private static Texture2D NewTexture(int size, string name)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = name;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        private static Sprite BakeSprite(Texture2D texture, int size)
        {
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
