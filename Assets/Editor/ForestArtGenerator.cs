using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Genera sprites PNG reutilizables del bosque misterioso en Assets/Art/Forest.
    /// Limitación: siluetas procedurales de baja resolución, no arte final de producción.
    /// </summary>
    public static class ForestArtGenerator
    {
        public const string ArtFolder = "Assets/Art/Forest";

        public static class Paths
        {
            public const string White = ArtFolder + "/white_pixel.png";
            public const string SkyGradient = ArtFolder + "/sky_sunset_gradient.png";
            public const string MountainFar = ArtFolder + "/mountain_far.png";
            public const string MountainNear = ArtFolder + "/mountain_near.png";
            public const string Hill = ArtFolder + "/hill_roll.png";
            public const string TreePineA = ArtFolder + "/tree_pine_a.png";
            public const string TreePineB = ArtFolder + "/tree_pine_b.png";
            public const string TreeBroad = ArtFolder + "/tree_broad.png";
            public const string Bush = ArtFolder + "/bush.png";
            public const string Grass = ArtFolder + "/grass_tuft.png";
            public const string RockLarge = ArtFolder + "/rock_large.png";
            public const string RockSmall = ArtFolder + "/rock_small.png";
            public const string Ground = ArtFolder + "/ground_earth.png";
            public const string Platform = ArtFolder + "/platform_wood.png";
            public const string Rail = ArtFolder + "/rail_segment.png";
            public const string Station = ArtFolder + "/station_abandoned.png";
            public const string Dust = ArtFolder + "/dust_mote.png";
            public const string Cloud = ArtFolder + "/cloud_soft.png";
        }

        public static void EnsureAllSprites()
        {
            ProceduralSpriteUtil.EnsureFolder("Assets/Art");
            ProceduralSpriteUtil.EnsureFolder(ArtFolder);

            Write(Paths.White, Solid(8, 8, Color.white), 8f, FilterMode.Point);
            Write(Paths.SkyGradient, SkySunset(64, 256), 32f);
            Write(Paths.MountainFar, Mountain(256, 96, new Color(0.28f, 0.30f, 0.40f), true), 32f);
            Write(Paths.MountainNear, Mountain(256, 110, new Color(0.22f, 0.26f, 0.32f), false), 32f);
            Write(Paths.Hill, Hills(192, 64, new Color(0.20f, 0.28f, 0.24f)), 32f);
            Write(Paths.TreePineA, PineTree(64, 128, new Color(0.14f, 0.28f, 0.18f)), 32f, FilterMode.Point);
            Write(Paths.TreePineB, PineTree(56, 112, new Color(0.12f, 0.24f, 0.16f), taller: true), 32f, FilterMode.Point);
            Write(Paths.TreeBroad, BroadTree(80, 112, new Color(0.18f, 0.30f, 0.18f)), 32f, FilterMode.Point);
            Write(Paths.Bush, Bush(48, 32, new Color(0.16f, 0.32f, 0.18f)), 32f, FilterMode.Point);
            Write(Paths.Grass, GrassTuft(32, 24, new Color(0.25f, 0.42f, 0.22f)), 32f, FilterMode.Point);
            Write(Paths.RockLarge, Rock(64, 40, new Color(0.42f, 0.40f, 0.44f)), 32f, FilterMode.Point);
            Write(Paths.RockSmall, Rock(32, 22, new Color(0.38f, 0.36f, 0.40f)), 32f, FilterMode.Point);
            Write(Paths.Ground, Ground(128, 64), 32f, FilterMode.Point);
            Write(Paths.Platform, PlatformWood(96, 24), 32f, FilterMode.Point);
            Write(Paths.Rail, Rails(160, 24), 32f, FilterMode.Point);
            Write(Paths.Station, Station(160, 96), 32f, FilterMode.Point);
            Write(Paths.Dust, SoftDisc(16, new Color(1f, 0.92f, 0.75f, 0.65f)), 16f);
            Write(Paths.Cloud, Cloud(96, 40, new Color(1f, 0.78f, 0.62f, 0.35f)), 32f);

            AssetDatabaseRefresh();
        }

        static void AssetDatabaseRefresh()
        {
            UnityEditor.AssetDatabase.Refresh();
        }

        static void Write(string path, Texture2D tex, float ppu, FilterMode filter = FilterMode.Bilinear)
        {
            ProceduralSpriteUtil.WriteSprite(path, tex, ppu, filter);
        }

        static Texture2D Solid(int w, int h, Color c)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            ProceduralSpriteUtil.FillRect(tex, 0, 0, w, h, c);
            tex.Apply();
            return tex;
        }

        static Texture2D SkySunset(int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color c;
                if (t < 0.35f)
                {
                    c = Color.Lerp(new Color(0.35f, 0.22f, 0.38f), new Color(0.85f, 0.42f, 0.35f), t / 0.35f);
                }
                else if (t < 0.65f)
                {
                    c = Color.Lerp(new Color(0.85f, 0.42f, 0.35f), new Color(0.98f, 0.72f, 0.45f), (t - 0.35f) / 0.3f);
                }
                else
                {
                    c = Color.Lerp(new Color(0.98f, 0.72f, 0.45f), new Color(0.55f, 0.70f, 0.88f), (t - 0.65f) / 0.35f);
                }

                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }

            // Sol bajo.
            ProceduralSpriteUtil.FillCircle(tex, w / 2 + 8, (int)(h * 0.42f), 10, new Color(1f, 0.9f, 0.55f, 0.9f));
            tex.Apply();
            return tex;
        }

        static Texture2D Mountain(int w, int h, Color color, bool jagged)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            int peaks = jagged ? 5 : 3;
            for (int i = 0; i < peaks; i++)
            {
                float cx = w * (0.12f + i * (0.8f / peaks));
                float peakH = h * (jagged ? (0.55f + (i % 3) * 0.15f) : (0.65f + (i % 2) * 0.2f));
                float half = w * (jagged ? 0.16f : 0.22f);
                ProceduralSpriteUtil.FillTriangle(
                    tex,
                    new Vector2(cx - half, 0f),
                    new Vector2(cx + half, 0f),
                    new Vector2(cx, peakH),
                    color);
            }

            Color snow = Color.Lerp(color, Color.white, 0.35f);
            snow.a = 0.55f;
            for (int i = 0; i < peaks; i++)
            {
                float cx = w * (0.12f + i * (0.8f / peaks));
                float peakH = h * (jagged ? (0.55f + (i % 3) * 0.15f) : (0.65f + (i % 2) * 0.2f));
                ProceduralSpriteUtil.FillTriangle(
                    tex,
                    new Vector2(cx - 8f, peakH - 14f),
                    new Vector2(cx + 8f, peakH - 14f),
                    new Vector2(cx, peakH),
                    snow);
            }

            tex.Apply();
            return tex;
        }

        static Texture2D Hills(int w, int h, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            ProceduralSpriteUtil.FillEllipse(tex, w / 3, 8, w / 3, h - 6, color);
            ProceduralSpriteUtil.FillEllipse(tex, (2 * w) / 3, 4, w / 3 + 10, h - 4, Color.Lerp(color, Color.black, 0.12f));
            tex.Apply();
            return tex;
        }

        static Texture2D PineTree(int w, int h, Color foliage, bool taller = false)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color trunk = new Color(0.28f, 0.18f, 0.12f);
            int trunkW = Mathf.Max(4, w / 10);
            ProceduralSpriteUtil.FillRect(tex, w / 2 - trunkW / 2, 2, trunkW, h / 3, trunk);

            int layers = taller ? 4 : 3;
            for (int i = 0; i < layers; i++)
            {
                float t = i / (float)(layers - 1);
                float topY = h * (0.35f + t * 0.55f);
                float half = w * (0.42f - t * 0.22f);
                Color layerColor = Color.Lerp(foliage, Color.black, i * 0.08f);
                ProceduralSpriteUtil.FillTriangle(
                    tex,
                    new Vector2(w * 0.5f - half, topY - h * 0.22f),
                    new Vector2(w * 0.5f + half, topY - h * 0.22f),
                    new Vector2(w * 0.5f, topY),
                    layerColor);
            }

            tex.Apply();
            return tex;
        }

        static Texture2D BroadTree(int w, int h, Color foliage)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color trunk = new Color(0.30f, 0.20f, 0.14f);
            ProceduralSpriteUtil.FillRect(tex, w / 2 - 4, 2, 8, h / 2, trunk);
            ProceduralSpriteUtil.FillCircle(tex, w / 2, (int)(h * 0.62f), w / 3, foliage);
            ProceduralSpriteUtil.FillCircle(tex, w / 2 - 14, (int)(h * 0.52f), w / 4, Color.Lerp(foliage, Color.black, 0.1f));
            ProceduralSpriteUtil.FillCircle(tex, w / 2 + 14, (int)(h * 0.52f), w / 4, Color.Lerp(foliage, Color.black, 0.08f));
            ProceduralSpriteUtil.FillCircle(tex, w / 2, (int)(h * 0.78f), w / 5, Color.Lerp(foliage, Color.white, 0.05f));
            tex.Apply();
            return tex;
        }

        static Texture2D Bush(int w, int h, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            ProceduralSpriteUtil.FillEllipse(tex, w / 2, h / 2, w / 2 - 2, h / 2 - 2, color);
            ProceduralSpriteUtil.FillCircle(tex, w / 3, h / 2 + 2, h / 3, Color.Lerp(color, Color.black, 0.1f));
            ProceduralSpriteUtil.FillCircle(tex, (2 * w) / 3, h / 2 + 1, h / 3, Color.Lerp(color, Color.white, 0.05f));
            tex.Apply();
            return tex;
        }

        static Texture2D GrassTuft(int w, int h, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            for (int i = 0; i < 7; i++)
            {
                int x = 4 + i * 4;
                int tip = h - 2 - (i % 3) * 3;
                ProceduralSpriteUtil.FillTriangle(
                    tex,
                    new Vector2(x - 2, 1),
                    new Vector2(x + 2, 1),
                    new Vector2(x, tip),
                    Color.Lerp(color, Color.black, (i % 2) * 0.1f));
            }

            tex.Apply();
            return tex;
        }

        static Texture2D Rock(int w, int h, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            ProceduralSpriteUtil.FillEllipse(tex, w / 2, h / 2 - 1, w / 2 - 2, h / 2 - 3, color);
            ProceduralSpriteUtil.FillEllipse(tex, w / 2 - 4, h / 2, w / 3, h / 3, Color.Lerp(color, Color.black, 0.18f));
            tex.Apply();
            return tex;
        }

        static Texture2D Ground(int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color dirt = new Color(0.30f, 0.22f, 0.14f);
            Color soil = new Color(0.24f, 0.18f, 0.12f);
            Color grass = new Color(0.28f, 0.40f, 0.22f);
            ProceduralSpriteUtil.FillRect(tex, 0, 0, w, h, soil);
            ProceduralSpriteUtil.FillRect(tex, 0, h - 18, w, 18, dirt);
            ProceduralSpriteUtil.FillRect(tex, 0, h - 8, w, 8, grass);
            for (int x = 0; x < w; x += 6)
            {
                ProceduralSpriteUtil.Set(tex, x, h - 10, Color.Lerp(grass, Color.black, 0.2f));
                ProceduralSpriteUtil.Set(tex, x + 2, h - 12, new Color(0.18f, 0.14f, 0.1f));
            }

            tex.Apply();
            return tex;
        }

        static Texture2D PlatformWood(int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color plank = new Color(0.42f, 0.28f, 0.16f);
            Color edge = new Color(0.28f, 0.18f, 0.10f);
            Color moss = new Color(0.25f, 0.38f, 0.20f, 0.85f);
            ProceduralSpriteUtil.FillRect(tex, 0, 0, w, h, plank);
            ProceduralSpriteUtil.FillRect(tex, 0, h - 4, w, 4, moss);
            ProceduralSpriteUtil.FillRect(tex, 0, 0, w, 2, edge);
            for (int x = 8; x < w; x += 16)
            {
                ProceduralSpriteUtil.FillRect(tex, x, 1, 1, h - 2, Color.Lerp(plank, Color.black, 0.2f));
            }

            tex.Apply();
            return tex;
        }

        static Texture2D Rails(int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color tie = new Color(0.22f, 0.16f, 0.12f);
            Color rail = new Color(0.18f, 0.18f, 0.20f);
            for (int x = 2; x < w; x += 10)
            {
                ProceduralSpriteUtil.FillRect(tex, x, 4, 7, 3, tie);
            }

            ProceduralSpriteUtil.FillRect(tex, 0, 8, w, 2, rail);
            ProceduralSpriteUtil.FillRect(tex, 0, 14, w, 2, rail);
            tex.Apply();
            return tex;
        }

        static Texture2D Station(int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            Color wood = new Color(0.25f, 0.18f, 0.14f);
            Color roof = new Color(0.16f, 0.14f, 0.18f);
            Color dark = new Color(0.08f, 0.08f, 0.1f);
            Color warmWindow = new Color(1f, 0.72f, 0.35f, 0.85f);

            ProceduralSpriteUtil.FillRect(tex, 20, 8, 110, 48, wood);
            ProceduralSpriteUtil.FillTriangle(tex, new Vector2(14, 56), new Vector2(146, 56), new Vector2(80, 88), roof);
            ProceduralSpriteUtil.FillRect(tex, 28, 8, 18, 30, dark);
            ProceduralSpriteUtil.FillRect(tex, 55, 28, 14, 14, warmWindow);
            ProceduralSpriteUtil.FillRect(tex, 78, 28, 14, 14, warmWindow);
            ProceduralSpriteUtil.FillRect(tex, 101, 28, 14, 14, dark);
            ProceduralSpriteUtil.FillRect(tex, 118, 48, 10, 28, wood);
            ProceduralSpriteUtil.FillRect(tex, 116, 74, 14, 6, roof);
            // Andén
            ProceduralSpriteUtil.FillRect(tex, 8, 4, 144, 6, new Color(0.2f, 0.2f, 0.22f));
            // Poste de señal
            ProceduralSpriteUtil.FillRect(tex, 6, 10, 3, 50, dark);
            ProceduralSpriteUtil.FillRect(tex, 2, 52, 12, 6, new Color(0.55f, 0.18f, 0.14f));
            tex.Apply();
            return tex;
        }

        static Texture2D SoftDisc(int size, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(size, size);
            float r = size * 0.5f - 0.5f;
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy)) / r;
                    float a = Mathf.Clamp01(1f - d);
                    a *= a;
                    Color c = color;
                    c.a *= a;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D Cloud(int w, int h, Color color)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            ProceduralSpriteUtil.FillEllipse(tex, w / 2, h / 2, w / 2 - 4, h / 2 - 4, color);
            ProceduralSpriteUtil.FillCircle(tex, w / 3, h / 2 + 2, h / 2 - 2, color);
            ProceduralSpriteUtil.FillCircle(tex, (2 * w) / 3, h / 2, h / 2 - 1, color);
            tex.Apply();
            return tex;
        }
    }
}
