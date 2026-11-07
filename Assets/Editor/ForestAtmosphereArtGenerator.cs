using System.IO;
using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Paleta "Memoria desvanecida": dos fríos dominantes, niebla y un único acento cálido.
    /// </summary>
    public static class AtmospherePalette
    {
        public static readonly Color Slate = Hex("1B2733");
        public static readonly Color GreenGray = Hex("3A4A4C");
        public static readonly Color Fog = Hex("7C8A8A");
        public static readonly Color Horizon = Hex("B9B3A0");
        public static readonly Color Warm = Hex("E8A84A");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            c.a = 1f;
            return c;
        }

        public static Color WithAlpha(Color c, float a)
        {
            c.a = a;
            return c;
        }
    }

    /// <summary>
    /// Genera los sprites de la escena ForestAtmosphereDemo en Assets/Art/ForestAtmosphere.
    /// Siluetas con degradados y ruido (no pixel art plano). Los sprites de gameplay respetan
    /// las mismas dimensiones y PPU que los de ForestDemo para no alterar los colliders.
    /// No son arte final: sirven para tono, profundidad y legibilidad.
    /// </summary>
    public static class ForestAtmosphereArtGenerator
    {
        public const string ArtFolder = "Assets/Art/ForestAtmosphere";

        public static class Paths
        {
            public const string Sky = ArtFolder + "/sky_cold_gradient.png";
            public const string HorizonGlow = ArtFolder + "/horizon_glow.png";
            public const string FogBand = ArtFolder + "/fog_band.png";
            public const string RidgeFar = ArtFolder + "/ridge_far.png";
            public const string RidgeNear = ArtFolder + "/ridge_near.png";
            public const string Hills = ArtFolder + "/hills_soft.png";
            public const string Rails = ArtFolder + "/rails_silhouette.png";
            public const string StationBody = ArtFolder + "/station_silhouette.png";
            public const string StationWindows = ArtFolder + "/station_windows.png";
            public const string PineA = ArtFolder + "/tree_pine_a.png";
            public const string PineB = ArtFolder + "/tree_pine_b.png";
            public const string Broad = ArtFolder + "/tree_broad.png";
            public const string TrunkTall = ArtFolder + "/trunk_tall.png";
            public const string Bush = ArtFolder + "/bush_soft.png";
            public const string Grass = ArtFolder + "/grass_wisp.png";
            public const string RockDecor = ArtFolder + "/rock_decor_haze.png";
            public const string Ground = ArtFolder + "/ground_atmos.png";
            public const string Shelf = ArtFolder + "/shelf_atmos.png";
            public const string Platform = ArtFolder + "/platform_atmos.png";
            public const string PlatformShadow = ArtFolder + "/platform_shadow_soft.png";
            public const string Crate = ArtFolder + "/crate_atmos.png";
            public const string RockSolid = ArtFolder + "/rock_solid_atmos.png";
            public const string Mote = ArtFolder + "/mote_soft.png";
            public const string GlowSoft = ArtFolder + "/glow_soft.png";
            public const string Vignette = ArtFolder + "/vignette_soft.png";
        }

        public static void EnsureAllSprites()
        {
            ProceduralSpriteUtil.EnsureFolder("Assets/Art");
            ProceduralSpriteUtil.EnsureFolder(ArtFolder);

            // Fondo y atmósfera (se tiñen en el builder: las texturas son blancas con alfa).
            Write(Paths.Sky, SkyGradient(16, 384), 16f);
            Write(Paths.HorizonGlow, RadialGlow(256, 128, new Color(1f, 0.93f, 0.78f, 1f), 0.9f), 32f);
            Write(Paths.FogBand, FogBand(256, 64), 32f);
            Write(Paths.RidgeFar, Ridge(256, 96, seed: 11, baseHeight: 0.32f, amplitude: 0.55f, freq: 0.011f, fadeBase: 0.35f), 32f);
            Write(Paths.RidgeNear, Ridge(256, 110, seed: 29, baseHeight: 0.28f, amplitude: 0.5f, freq: 0.016f, fadeBase: 0.3f), 32f);
            Write(Paths.Hills, Ridge(192, 64, seed: 53, baseHeight: 0.38f, amplitude: 0.42f, freq: 0.02f, fadeBase: 0.08f), 32f);
            Write(Paths.Rails, Rails(160, 24), 32f, FilterMode.Bilinear);
            Write(Paths.StationBody, StationBody(160, 96), 32f);
            Write(Paths.StationWindows, StationWindows(160, 96), 32f);
            Write(Paths.PineA, Pine(64, 128, taller: false, seed: 3), 32f);
            Write(Paths.PineB, Pine(56, 112, taller: true, seed: 7), 32f);
            Write(Paths.Broad, BroadTree(80, 112, seed: 5), 32f);
            Write(Paths.TrunkTall, TrunkTall(24, 192), 32f);
            Write(Paths.Bush, SoftCluster(48, 32, seed: 2), 32f);
            Write(Paths.Grass, GrassWisp(32, 24), 32f);
            Write(Paths.RockDecor, SoftCluster(48, 28, seed: 9, flatBottom: true), 32f);

            // Gameplay: mismas dimensiones/PPU que ForestDemo (ground 128x64, shelf 128x64,
            // platform 96x32, crate 48x48@48, rock 64x48) para conservar los colliders.
            Write(Paths.Ground, Ground(128, 64), 32f);
            Write(Paths.Shelf, Shelf(128, 64), 32f);
            Write(Paths.Platform, PlatformPlanks(96, 32), 32f);
            Write(Paths.PlatformShadow, SoftShadow(96, 16), 32f);
            Write(Paths.Crate, Crate(48, 48), 48f);
            Write(Paths.RockSolid, RockSolid(64, 48), 32f);

            // Partículas, luz y viñeta.
            Write(Paths.Mote, RadialGlow(32, 32, Color.white, 2.2f), 32f);
            Write(Paths.GlowSoft, RadialGlow(64, 64, AtmospherePalette.Warm, 1.6f), 32f);
            Write(Paths.Vignette, Vignette(128, 128), 8f);

            AssetDatabase.Refresh();
        }

        static void Write(string path, Texture2D tex, float ppu, FilterMode filter = FilterMode.Bilinear)
        {
            ProceduralSpriteUtil.WriteSprite(path, tex, ppu, filter);
        }

        // ------------------------------------------------------------------
        // Utilidades de ruido y composición
        // ------------------------------------------------------------------

        static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0x7fffffff) / (float)int.MaxValue;
            }
        }

        static float ValueNoise(float x, float y, int seed)
        {
            int xi = Mathf.FloorToInt(x);
            int yi = Mathf.FloorToInt(y);
            float tx = x - xi;
            float ty = y - yi;
            tx = tx * tx * (3f - 2f * tx);
            ty = ty * ty * (3f - 2f * ty);
            float a = Hash(xi, yi, seed);
            float b = Hash(xi + 1, yi, seed);
            float c = Hash(xi, yi + 1, seed);
            float d = Hash(xi + 1, yi + 1, seed);
            return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
        }

        static float Fbm(float x, float y, int seed, int octaves = 4)
        {
            float v = 0f;
            float amp = 0.5f;
            float freq = 1f;
            for (int i = 0; i < octaves; i++)
            {
                v += amp * ValueNoise(x * freq, y * freq, seed + i * 17);
                freq *= 2f;
                amp *= 0.5f;
            }

            return v;
        }

        static Color Over(Color dst, Color src)
        {
            float outA = src.a + dst.a * (1f - src.a);
            if (outA <= 0.0001f)
            {
                return Color.clear;
            }

            Color o = (src * src.a + dst * dst.a * (1f - src.a)) / outA;
            o.a = outA;
            return o;
        }

        static void Put(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            px[y * w + x] = Over(px[y * w + x], c);
        }

        static Color[] NewBuffer(int w, int h)
        {
            var px = new Color[w * h];
            for (int i = 0; i < px.Length; i++)
            {
                px[i] = Color.clear;
            }

            return px;
        }

        static Texture2D ToTexture(Color[] px, int w, int h)
        {
            var tex = ProceduralSpriteUtil.CreateTexture(w, h);
            tex.SetPixels(px);
            tex.Apply();
            return tex;
        }

        static Color SampleGradient(float t, params (float t, Color c)[] stops)
        {
            if (t <= stops[0].t)
            {
                return stops[0].c;
            }

            for (int i = 1; i < stops.Length; i++)
            {
                if (t <= stops[i].t)
                {
                    float k = Mathf.InverseLerp(stops[i - 1].t, stops[i].t, t);
                    k = k * k * (3f - 2f * k);
                    return Color.Lerp(stops[i - 1].c, stops[i].c, k);
                }
            }

            return stops[stops.Length - 1].c;
        }

        static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        // ------------------------------------------------------------------
        // Cielo, niebla, relieve lejano
        // ------------------------------------------------------------------

        /// <summary>Degradado vertical frío. El horizonte pálido está hacia t=0.35 (y mundo ≈ 1).</summary>
        static Texture2D SkyGradient(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color fog = AtmospherePalette.Fog;
            Color green = AtmospherePalette.GreenGray;
            Color slate = AtmospherePalette.Slate;
            Color horizon = AtmospherePalette.Horizon;
            Color high = Color.Lerp(green, fog, 0.25f);
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                Color c = SampleGradient(
                    t,
                    (0.00f, green),
                    (0.20f, fog),
                    (0.35f, horizon),
                    (0.47f, Color.Lerp(fog, horizon, 0.35f)),
                    (0.62f, high),
                    (0.80f, green),
                    (1.00f, slate));
                for (int x = 0; x < w; x++)
                {
                    float dither = (Hash(x, y, 91) - 0.5f) * 0.02f;
                    var d = new Color(c.r + dither, c.g + dither, c.b + dither, 1f);
                    px[y * w + x] = d;
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D RadialGlow(int w, int h, Color color, float power)
        {
            var px = NewBuffer(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f) / w * 2f - 1f;
                    float ny = (y + 0.5f) / h * 2f - 1f;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), power);
                    var c = color;
                    c.a = a;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D FogBand(int w, int h)
        {
            var px = NewBuffer(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float ex = (x / (float)(w - 1) - 0.5f) * 2f;
                    float ey = (y / (float)(h - 1) - 0.5f) * 2f;
                    float shape = Mathf.Pow(Mathf.Clamp01(1f - ey * ey), 2f) * Mathf.Clamp01(1.4f - Mathf.Pow(Mathf.Abs(ex), 4f));
                    float n = Mathf.Lerp(0.35f, 1f, Fbm(x / 36f, y / 14f, 41, 4));
                    px[y * w + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(shape * n));
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Cordillera suave por mapa de alturas; la base se funde con la niebla.</summary>
        static Texture2D Ridge(int w, int h, int seed, float baseHeight, float amplitude, float freq, float fadeBase)
        {
            var px = NewBuffer(w, h);
            for (int x = 0; x < w; x++)
            {
                float n = Fbm(x * freq, seed * 0.37f, seed, 5);
                float height = h * Mathf.Clamp01(baseHeight + amplitude * (n - 0.25f));
                for (int y = 0; y < h; y++)
                {
                    float edge = Mathf.Clamp01(height - y);
                    if (edge <= 0f)
                    {
                        continue;
                    }

                    float rel = y / Mathf.Max(1f, height);
                    float baseFade = Mathf.SmoothStep(fadeBase, 1f, Mathf.Clamp01(rel / 0.5f));
                    float shade = 0.88f + 0.12f * Fbm(x / 20f, y / 12f, seed + 5, 3);
                    float topLight = Mathf.Clamp01(1f - (height - y) / 7f) * 0.08f;
                    float v = Mathf.Clamp01(shade + topLight);
                    px[y * w + x] = new Color(v, v, v, edge * baseFade);
                }
            }

            return ToTexture(px, w, h);
        }

        // ------------------------------------------------------------------
        // Árboles y estructuras (siluetas blancas teñibles)
        // ------------------------------------------------------------------

        static Texture2D Pine(int w, int h, bool taller, int seed)
        {
            var px = NewBuffer(w, h);
            float cx = w * 0.5f;
            float foliageStart = h * 0.16f;
            int tiers = taller ? 7 : 5;
            int trunkW = Mathf.Max(3, w / 14);
            for (int y = 0; y < h; y++)
            {
                float ty = Mathf.InverseLerp(foliageStart, h - 2f, y);
                for (int x = 0; x < w; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - cx);
                    float alpha = 0f;
                    if (y < foliageStart + 6f && dx < trunkW * 0.5f + (y < 4 ? 1.5f : 0f))
                    {
                        alpha = 1f;
                    }

                    if (ty >= 0f && ty <= 1f)
                    {
                        // Cada piso es un triángulo: ancho abajo, afilado arriba, y el siguiente vuelve a ensancharse.
                        float rp = Mathf.Repeat(ty * tiers, 1f);
                        float saw = 1f - 0.5f * rp;
                        float hw = w * 0.47f * Mathf.Pow(1f - ty, 0.8f) * saw;
                        hw += (Hash(y, seed, 5) - 0.5f) * 2.6f;
                        alpha = Mathf.Max(alpha, Mathf.Clamp01(hw - dx + 0.5f));
                    }

                    if (alpha <= 0f)
                    {
                        continue;
                    }

                    float v = Mathf.Lerp(0.78f, 1f, y / (float)h) * (0.94f + 0.06f * Hash(x, y, seed));
                    px[y * w + x] = new Color(v, v, v, alpha);
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D BroadTree(int w, int h, int seed)
        {
            var px = NewBuffer(w, h);
            float cx = w * 0.5f;
            int trunkTop = (int)(h * 0.5f);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float alpha = 0f;
                    float dx = Mathf.Abs(x + 0.5f - cx);
                    float trunkHalf = Mathf.Lerp(4.5f, 3f, y / (float)trunkTop);
                    if (y < trunkTop && dx < trunkHalf)
                    {
                        alpha = 1f;
                    }

                    // Copa irregular: mezcla de elipses + ruido como umbral.
                    float ex = (x + 0.5f - cx) / (w * 0.46f);
                    float ey = (y + 0.5f - h * 0.66f) / (h * 0.30f);
                    float d = Mathf.Sqrt(ex * ex + ey * ey);
                    // Copa con racimos definidos (umbral nítido) en vez de nube difusa.
                    float n = Fbm(x / 6f, y / 6f, seed, 3);
                    float canopy = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.56f, (1.0f - d) * 0.9f + n * 0.55f));
                    alpha = Mathf.Max(alpha, canopy);
                    if (alpha <= 0.01f)
                    {
                        continue;
                    }

                    float v = Mathf.Lerp(0.74f, 1f, y / (float)h) * (0.92f + 0.08f * n);
                    px[y * w + x] = new Color(v, v, v, alpha);
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Tronco alto sin copa visible (escala y vacío; evoca bosque sin fin).</summary>
        static Texture2D TrunkTall(int w, int h)
        {
            var px = NewBuffer(w, h);
            float cx = w * 0.5f;
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                float bend = Mathf.Sin(t * 3.1f) * 1.2f;
                float half = Mathf.Lerp(w * 0.34f, w * 0.20f, t) + (Hash(y, 3, 13) - 0.5f) * 0.6f;
                for (int x = 0; x < w; x++)
                {
                    float dx = x + 0.5f - (cx + bend);
                    float a = Mathf.Clamp01(half - Mathf.Abs(dx) + 0.5f);
                    // Ramas cortas.
                    foreach (float by in new[] { 0.46f, 0.64f, 0.82f })
                    {
                        float dy = Mathf.Abs(t - by) * h;
                        if (dy < 2.2f && Mathf.Abs(dx) < half + 6f && Mathf.Abs(dx) > half - 1f)
                        {
                            float side = (by > 0.6f) ? -1f : 1f;
                            if (dx * side > 0f)
                            {
                                a = Mathf.Max(a, 0.9f);
                            }
                        }
                    }

                    if (a <= 0f)
                    {
                        continue;
                    }

                    float v = Mathf.Lerp(0.82f, 1f, (dx / half + 1f) * 0.5f) * (0.9f + 0.1f * Hash(x, y, 4));
                    px[y * w + x] = new Color(v, v, v, a);
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D Rails(int w, int h)
        {
            var px = NewBuffer(w, h);
            for (int x = 2; x < w; x += 10)
            {
                for (int y = 4; y < 7; y++)
                {
                    for (int dx = 0; dx < 7; dx++)
                    {
                        Put(px, w, h, x + dx, y, new Color(0.8f, 0.8f, 0.8f, 0.9f));
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                Put(px, w, h, x, 8, Color.white);
                Put(px, w, h, x, 9, Color.white);
                Put(px, w, h, x, 14, Color.white);
                Put(px, w, h, x, 15, Color.white);
            }

            return ToTexture(px, w, h);
        }

        static Texture2D StationBody(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color solid = Color.white;
            // Cuerpo.
            for (int y = 8; y < 56; y++)
            {
                for (int x = 20; x < 130; x++)
                {
                    Put(px, w, h, x, y, solid);
                }
            }

            // Tejado triangular.
            var roof = new[] { new Vector2(14, 56), new Vector2(146, 56), new Vector2(80, 90) };
            for (int y = 56; y < 92; y++)
            {
                for (int x = 10; x < 150; x++)
                {
                    if (PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), roof))
                    {
                        Put(px, w, h, x, y, solid);
                    }
                }
            }

            // Chimenea, andén y poste.
            for (int y = 48; y < 80; y++)
            {
                for (int x = 118; x < 128; x++)
                {
                    Put(px, w, h, x, y, solid);
                }
            }

            for (int y = 3; y < 9; y++)
            {
                for (int x = 6; x < 154; x++)
                {
                    Put(px, w, h, x, y, new Color(0.9f, 0.9f, 0.9f, 1f));
                }
            }

            for (int y = 10; y < 60; y++)
            {
                for (int x = 6; x < 9; x++)
                {
                    Put(px, w, h, x, y, solid);
                }
            }

            // Huecos: puerta y ventanas (se ve el cielo detrás).
            ClearRect(px, w, h, 28, 8, 18, 30);
            ClearRect(px, w, h, 55, 28, 14, 14);
            ClearRect(px, w, h, 78, 28, 14, 14);
            ClearRect(px, w, h, 101, 28, 14, 14);
            // Marco ligeramente más oscuro alrededor de la puerta.
            return ToTexture(px, w, h);
        }

        static void ClearRect(Color[] px, int w, int h, int x0, int y0, int rw, int rh)
        {
            for (int y = y0; y < y0 + rh; y++)
            {
                for (int x = x0; x < x0 + rw; x++)
                {
                    if (x >= 0 && y >= 0 && x < w && y < h)
                    {
                        px[y * w + x] = Color.clear;
                    }
                }
            }
        }

        /// <summary>Dos ventanas con luz cálida, mismas dimensiones que StationBody.</summary>
        static Texture2D StationWindows(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color warm = AtmospherePalette.Warm;
            int[] xs = { 55, 78 };
            foreach (int x0 in xs)
            {
                for (int y = 28; y < 42; y++)
                {
                    for (int x = x0; x < x0 + 14; x++)
                    {
                        float g = 0.75f + 0.25f * ((y - 28) / 13f);
                        px[y * w + x] = new Color(warm.r * g, warm.g * g, warm.b * g, 0.95f);
                    }
                }

                // Halo suave alrededor del cristal.
                for (int y = 22; y < 50; y++)
                {
                    for (int x = x0 - 6; x < x0 + 20; x++)
                    {
                        if (x < 0 || x >= w)
                        {
                            continue;
                        }

                        if (y >= 28 && y < 42 && x >= x0 && x < x0 + 14)
                        {
                            continue;
                        }

                        float dx = Mathf.Max(0f, Mathf.Max(x0 - x, x - (x0 + 13)));
                        float dy = Mathf.Max(0f, Mathf.Max(28 - y, y - 41));
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        float a = Mathf.Clamp01(1f - d / 7f);
                        a *= a * 0.35f;
                        if (a > 0.005f)
                        {
                            Put(px, w, h, x, y, new Color(warm.r, warm.g, warm.b, a));
                        }
                    }
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D SoftCluster(int w, int h, int seed, bool flatBottom = false)
        {
            var px = NewBuffer(w, h);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float ex = (x + 0.5f) / w * 2f - 1f;
                    float ey = (y + 0.5f) / h * 2f - 1f;
                    float cluster = 0f;
                    cluster = Mathf.Max(cluster, 1f - Mathf.Sqrt((ex + 0.35f) * (ex + 0.35f) * 1.1f + (ey + 0.1f) * (ey + 0.1f) * 1.8f));
                    cluster = Mathf.Max(cluster, 1f - Mathf.Sqrt((ex - 0.30f) * (ex - 0.30f) * 1.2f + (ey + 0.05f) * (ey + 0.05f) * 1.6f));
                    cluster = Mathf.Max(cluster, 1f - Mathf.Sqrt(ex * ex * 0.7f + (ey - 0.25f) * (ey - 0.25f) * 2.6f));
                    float n = Fbm(x / 7f, y / 7f, seed, 3);
                    float a = Mathf.Clamp01(cluster * 2.2f + (n - 0.5f) * 0.8f);
                    if (flatBottom && y < 3)
                    {
                        a *= y / 3f;
                    }

                    if (a <= 0.01f)
                    {
                        continue;
                    }

                    float v = Mathf.Lerp(0.8f, 1f, y / (float)h);
                    px[y * w + x] = new Color(v, v, v, a);
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D GrassWisp(int w, int h)
        {
            var px = NewBuffer(w, h);
            for (int i = 0; i < 9; i++)
            {
                float baseX = 3f + i * (w - 6f) / 8f;
                float tip = h * (0.55f + 0.4f * Hash(i, 1, 21));
                float lean = (Hash(i, 2, 21) - 0.5f) * 8f;
                for (int y = 0; y < (int)tip; y++)
                {
                    float t = y / tip;
                    float x = baseX + lean * t * t;
                    float half = Mathf.Lerp(1.1f, 0.15f, t);
                    for (int px0 = Mathf.FloorToInt(x - 2f); px0 <= Mathf.CeilToInt(x + 2f); px0++)
                    {
                        float a = Mathf.Clamp01(half - Mathf.Abs(px0 + 0.5f - x) + 0.5f);
                        if (a > 0f)
                        {
                            Put(px, w, h, px0, y, new Color(1f, 1f, 1f, a * 0.9f));
                        }
                    }
                }
            }

            return ToTexture(px, w, h);
        }

        // ------------------------------------------------------------------
        // Superficies de gameplay (mismas dimensiones que ForestDemo)
        // ------------------------------------------------------------------

        /// <summary>
        /// Suelo principal: se estira en horizontal, por eso solo lleva bandas horizontales.
        /// Borde superior pálido (pisable) + capa de musgo apagado + estratos fríos.
        /// </summary>
        static Texture2D Ground(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color rim = AtmospherePalette.Hex("C3C7B4");
            Color lip = AtmospherePalette.Hex("8C9A92");
            Color moss = AtmospherePalette.Hex("56685F");
            Color bodyTop = AtmospherePalette.Hex("3F5052");
            Color bodyBottom = AtmospherePalette.Slate;
            for (int y = 0; y < h; y++)
            {
                int d = h - 1 - y;
                for (int x = 0; x < w; x++)
                {
                    float n = Fbm(x / 22f, y / 3.5f, 61, 3) - 0.5f;
                    Color c;
                    if (d < 2)
                    {
                        c = rim;
                    }
                    else if (d < 4)
                    {
                        c = lip;
                    }
                    else if (d < 14)
                    {
                        float k = (d - 4f) / 10f;
                        c = Color.Lerp(moss, Color.Lerp(moss, bodyTop, 0.8f), k);
                        c += new Color(n, n, n, 0f) * 0.10f;
                    }
                    else
                    {
                        float k = Mathf.Clamp01((d - 14f) / (h - 16f));
                        c = Color.Lerp(bodyTop, bodyBottom, Mathf.SmoothStep(0f, 1f, k));
                        float strata = (d % 11 == 0) ? -0.05f : 0f;
                        c += new Color(n + strata, n + strata, n + strata, 0f) * 0.12f;
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>RaisedShelf: peldaño de piedra más claro que el suelo, con labio y repisa visibles.</summary>
        static Texture2D Shelf(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color rim = AtmospherePalette.Hex("D2D3C0");
            Color topFace = AtmospherePalette.Hex("6F7D70");
            Color stoneTop = AtmospherePalette.Hex("56686A");
            Color stoneBottom = AtmospherePalette.Hex("27363B");
            Color ledgeDark = AtmospherePalette.Hex("1E2B30");
            Color ledgeLight = AtmospherePalette.Hex("8DA0A0");
            for (int y = 0; y < h; y++)
            {
                int d = h - 1 - y;
                for (int x = 0; x < w; x++)
                {
                    float n = Fbm(x / 18f, y / 3f, 77, 3) - 0.5f;
                    Color c;
                    if (d < 3)
                    {
                        c = rim;
                    }
                    else if (d < 15)
                    {
                        c = Color.Lerp(topFace, stoneTop, (d - 3f) / 12f);
                    }
                    else if (d == 15)
                    {
                        c = ledgeDark;
                    }
                    else if (d == 16)
                    {
                        c = ledgeLight;
                    }
                    else
                    {
                        float k = Mathf.Clamp01((d - 17f) / (h - 19f));
                        c = Color.Lerp(stoneTop, stoneBottom, k);
                    }

                    c += new Color(n, n, n, 0f) * 0.12f;
                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Pasarela de tablones envejecidos: tapa pálida, juntas, sombra inferior irregular.</summary>
        static Texture2D PlatformPlanks(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color rim = AtmospherePalette.Hex("C0C5B2");
            Color topFace = AtmospherePalette.Hex("8A8A74");
            Color plank = AtmospherePalette.Hex("5E6256");
            Color plankDark = AtmospherePalette.Hex("3F4842");
            Color underside = AtmospherePalette.Hex("1F2A2E");
            Color seam = AtmospherePalette.Hex("1B2428");
            for (int y = 0; y < h; y++)
            {
                int d = h - 1 - y;
                for (int x = 0; x < w; x++)
                {
                    float n = Fbm(x / 6f, y / 2.5f, 17, 3) - 0.5f;
                    Color c;
                    if (d < 2)
                    {
                        c = rim;
                    }
                    else if (d < 6)
                    {
                        c = Color.Lerp(topFace, plank, (d - 2f) / 4f);
                    }
                    else if (d < h - 6)
                    {
                        c = Color.Lerp(plank, plankDark, (d - 6f) / (h - 12f));
                    }
                    else
                    {
                        c = underside;
                    }

                    c += new Color(n, n, n, 0f) * 0.14f;
                    if ((x % 16) == 0 && d >= 3 && d < h - 3)
                    {
                        c = Color.Lerp(c, seam, 0.8f);
                    }

                    if (x < 2 || x >= w - 2)
                    {
                        c = Color.Lerp(c, seam, 0.55f);
                    }

                    c.a = 1f;
                    // Borde inferior irregular (madera podrida) dentro de los dos últimos píxeles.
                    if (y < 2 && Hash(x, y, 33) > 0.55f)
                    {
                        c.a = 0.55f;
                    }

                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D SoftShadow(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color shadow = AtmospherePalette.Hex("0E161B");
            for (int y = 0; y < h; y++)
            {
                float t = 1f - y / (float)(h - 1);
                for (int x = 0; x < w; x++)
                {
                    float edge = Mathf.Clamp01(Mathf.Min(x, w - 1 - x) / (w * 0.22f));
                    var c = shadow;
                    c.a = 0.55f * t * t * edge;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Caja de madera gris-parda desaturada: marco oscuro, listones, tapa clara y diagonal.</summary>
        static Texture2D Crate(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color frame = AtmospherePalette.Hex("23272A");
            Color wood = AtmospherePalette.Hex("6D6452");
            Color woodDark = AtmospherePalette.Hex("4B473F");
            Color topLight = AtmospherePalette.Hex("A39B84");
            Color gap = AtmospherePalette.Hex("2B2D2B");
            Color brace = AtmospherePalette.Hex("857B64");
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float n = Fbm(x / 5f, y / 2f, 23, 3) - 0.5f;
                    Color c = Color.Lerp(wood, woodDark, 1f - y / (float)(h - 1));
                    c += new Color(n, n, n, 0f) * 0.12f;
                    int edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                    if (edge < 3)
                    {
                        c = frame;
                    }
                    else if (y > h - 7)
                    {
                        c = Color.Lerp(topLight, wood, (h - 1 - y - 3f) / 3f);
                    }
                    else
                    {
                        // Listones horizontales con junta oscura.
                        if ((y - 3) % 11 == 0)
                        {
                            c = gap;
                        }

                        // Diagonal de refuerzo.
                        if (Mathf.Abs((x - 3) - (y - 3)) < 2 && x > 3 && x < w - 4 && y > 3 && y < h - 8)
                        {
                            c = Color.Lerp(c, brace, 0.8f);
                        }
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Roca sólida facetada con arista oscura: bloque macizo, claramente "obstáculo".</summary>
        static Texture2D RockSolid(int w, int h)
        {
            var px = NewBuffer(w, h);
            var poly = new[]
            {
                new Vector2(3f, 1f), new Vector2(1f, 16f), new Vector2(6f, 33f), new Vector2(15f, 44f),
                new Vector2(34f, 46.5f), new Vector2(51f, 41f), new Vector2(61f, 27f), new Vector2(62.5f, 10f),
                new Vector2(58f, 1f)
            };
            Color baseCol = AtmospherePalette.Hex("65726F");
            Color litCol = AtmospherePalette.Hex("93A09A");
            Color shadeCol = AtmospherePalette.Hex("3A4849");
            Color edgeCol = AtmospherePalette.Hex("162024");
            Color moss = AtmospherePalette.Hex("56705A");
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    if (!PointInPolygon(p, poly))
                    {
                        continue;
                    }

                    // Distancia aproximada al borde por sondeo de vecinos.
                    float edgeDist = 6f;
                    for (int r = 1; r <= 3; r++)
                    {
                        if (!PointInPolygon(p + new Vector2(r, 0), poly) || !PointInPolygon(p - new Vector2(r, 0), poly) ||
                            !PointInPolygon(p + new Vector2(0, r), poly) || !PointInPolygon(p - new Vector2(0, r), poly))
                        {
                            edgeDist = r;
                            break;
                        }
                    }

                    float lx = x / (float)w;
                    float ly = y / (float)h;
                    float light = Mathf.Clamp01(ly * 1.2f - lx * 0.35f + 0.1f);
                    // Facetas: cambio brusco de tono según líneas diagonales.
                    float facet = Mathf.Repeat((x * 0.55f + y * 0.9f) / 14f, 1f) < 0.5f ? 0.07f : -0.05f;
                    Color c = Color.Lerp(shadeCol, baseCol, Mathf.Clamp01(light + facet));
                    if (ly > 0.72f)
                    {
                        c = Color.Lerp(c, litCol, (ly - 0.72f) / 0.28f * 0.9f);
                    }

                    float n = Fbm(x / 6f, y / 6f, 31, 3) - 0.5f;
                    c += new Color(n, n, n, 0f) * 0.16f;
                    if (ly > 0.82f && lx > 0.2f && lx < 0.7f && n > -0.05f)
                    {
                        c = Color.Lerp(c, moss, 0.5f);
                    }

                    if (edgeDist <= 1f)
                    {
                        c = edgeCol;
                    }
                    else if (edgeDist <= 2f)
                    {
                        c = Color.Lerp(c, edgeCol, 0.55f);
                    }

                    // Grieta.
                    if (Mathf.Abs(x - (w * 0.45f + (y - h * 0.3f) * 0.35f)) < 0.9f && y > h * 0.2f && y < h * 0.62f)
                    {
                        c = Color.Lerp(c, edgeCol, 0.8f);
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        static Texture2D Vignette(int w, int h)
        {
            var px = NewBuffer(w, h);
            Color slate = AtmospherePalette.Hex("0E161D");
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f) / w * 2f - 1f;
                    float ny = (y + 0.5f) / h * 2f - 1f;
                    float r = Mathf.Sqrt(nx * nx + ny * ny);
                    float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 1.25f, r));
                    var c = slate;
                    c.a = a;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }
    }
}
