using UnityEngine;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Arte procedural del bosque de la primera prueba ("Memoria desvanecida"): azules desaturados,
    /// verde grisáceo, sombras frías y toques otoñales discretos. Provisional: pensado para sustituirse
    /// por arte de producción manteniendo capas, escalas y factores de parallax.
    /// </summary>
    public static class DemoEnvironmentArt
    {
        public const string Folder = "Assets/Art/Demo";

        public static string P(string name) => $"{Folder}/{name}.png";

        public static void Generate()
        {
            GenerateSky();
            GenerateSunShaft();
            GenerateFog();

            Ridge("ridge_far", 11, 0.55f, 55f, new Color(0.56f, 0.68f, 0.74f), new Color(0.42f, 0.55f, 0.63f), 70, 0.9f);
            Ridge("ridge_near", 23, 0.50f, 46f, new Color(0.34f, 0.47f, 0.53f), new Color(0.24f, 0.36f, 0.43f), 90, 1.25f);

            Pine("tree_pine_a", 160, 480, 101, 1.0f);
            Pine("tree_pine_b", 136, 440, 202, 0.82f);
            Pine("tree_pine_c", 176, 420, 303, 1.15f);
            Broadleaf("tree_broad_a", 224, 420, 404, 0.0f);
            Broadleaf("tree_broad_b", 208, 400, 505, 0.55f);
            ForegroundTrunk("trunk_fg_a", 112, 540, 606);
            ForegroundTrunk("trunk_fg_b", 96, 540, 707);

            Rock("rock_a", 168, 110, 31, true);
            Rock("rock_b", 120, 84, 47, true);
            Rock("rock_c", 88, 60, 53, false);
            FallenLog("log_fallen", 176, 32, 61);
            Crate("crate_wood", 44, 44, 17);
            Fern("fern", 112, 96, 71);
            GrassTuft("grass_tuft", 72, 56, 81);
            Bush("bush", 160, 96, 91);
            PromptKey("prompt_key_e");
            GenerateTerrain();
        }

        // ------------------------------------------------------------------
        // Cielo, luz, niebla
        // ------------------------------------------------------------------

        static void GenerateSky()
        {
            var img = new DemoImg(4, 512);
            Color top = new Color(0.13f, 0.20f, 0.29f);
            Color mid = new Color(0.30f, 0.43f, 0.51f);
            Color horizon = new Color(0.66f, 0.74f, 0.70f);
            for (int y = 0; y < img.H; y++)
            {
                float t = y / (float)(img.H - 1); // 0 abajo (horizonte) -> 1 arriba
                Color c = t < 0.45f
                    ? Color.Lerp(horizon, mid, t / 0.45f)
                    : Color.Lerp(mid, top, (t - 0.45f) / 0.55f);
                for (int x = 0; x < img.W; x++)
                {
                    img.Set(x, y, c);
                }
            }

            img.Save(P("sky"), 32f, FilterMode.Bilinear);
        }

        static void GenerateSunShaft()
        {
            var img = new DemoImg(96, 384);
            for (int y = 0; y < img.H; y++)
            {
                float v = y / (float)(img.H - 1);
                float fade = Mathf.SmoothStep(0f, 1f, v) * (1f - Mathf.SmoothStep(0.75f, 1f, v) * 0.0f);
                for (int x = 0; x < img.W; x++)
                {
                    float u = (x + 0.5f) / img.W * 2f - 1f;
                    float bell = Mathf.Pow(Mathf.Clamp01(1f - Mathf.Abs(u)), 1.6f);
                    float stripe = 0.75f + 0.25f * Mathf.PerlinNoise(x * 0.09f, 3.1f);
                    img.Set(x, y, new Color(1f, 0.93f, 0.74f, bell * fade * stripe));
                }
            }

            img.Save(P("sun_shaft"), 32f, FilterMode.Bilinear);
        }

        static void GenerateFog()
        {
            var img = new DemoImg(256, 96);
            for (int y = 0; y < img.H; y++)
            {
                for (int x = 0; x < img.W; x++)
                {
                    float nx = (x + 0.5f) / img.W * 2f - 1f;
                    float ny = (y + 0.5f) / img.H * 2f - 1f;
                    float d = Mathf.Clamp01(1f - Mathf.Sqrt(nx * nx * 0.55f + ny * ny));
                    float n = 0.55f + 0.45f * Mathf.PerlinNoise(x * 0.025f + 4f, y * 0.06f);
                    img.Set(x, y, new Color(0.80f, 0.88f, 0.92f, d * d * n * 0.65f));
                }
            }

            img.Save(P("fog"), 32f, FilterMode.Bilinear);
        }

        // ------------------------------------------------------------------
        // Crestas lejanas (teselables en horizontal)
        // ------------------------------------------------------------------

        static void Ridge(string name, int seed, float baseFrac, float amp, Color top, Color bottom, int trees, float treeScale)
        {
            const int w = 1024;
            const int h = 256;
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            int[] freqs = { 1, 2, 3, 5, 7, 11, 17 };
            float[] amps = { 1f, 0.6f, 0.45f, 0.25f, 0.18f, 0.1f, 0.06f };
            float[] phases = new float[freqs.Length];
            for (int i = 0; i < phases.Length; i++)
            {
                phases[i] = (float)rng.NextDouble() * Mathf.PI * 2f;
            }

            var prof = new float[w];
            for (int x = 0; x < w; x++)
            {
                float v = 0f;
                for (int k = 0; k < freqs.Length; k++)
                {
                    v += amps[k] * Mathf.Sin(2f * Mathf.PI * freqs[k] * x / w + phases[k]);
                }

                prof[x] = h * baseFrac + amp * v * 0.45f;
            }

            for (int x = 0; x < w; x++)
            {
                int yMax = Mathf.Min(h - 1, Mathf.RoundToInt(prof[x]));
                for (int y = 0; y <= yMax; y++)
                {
                    float t = y / Mathf.Max(1f, prof[x]);
                    Color c = Color.Lerp(bottom, top, t);
                    c.a = Mathf.Clamp01(y / (h * 0.28f));
                    img.Set(x, y, c);
                }
            }

            Color silhouette = DemoImg.Mul(bottom, 0.9f);
            for (int i = 0; i < trees; i++)
            {
                int x0 = rng.Next(0, w);
                float height = (14f + (float)rng.NextDouble() * 22f) * treeScale;
                float baseY = prof[x0] - 3f;
                for (int wrap = -1; wrap <= 1; wrap++)
                {
                    PineSilhouette(img, x0 + wrap * w, baseY, height, silhouette);
                }
            }

            img.Save(P(name), 32f, FilterMode.Bilinear);
        }

        static void PineSilhouette(DemoImg img, int cx, float baseY, float height, Color c)
        {
            for (int yy = 0; yy < Mathf.CeilToInt(height); yy++)
            {
                float f = yy / height;
                float tier = (f * 5f) % 1f;
                float half = 0.17f * height * (1f - f) * (0.7f + 0.3f * (1f - tier));
                for (int x = Mathf.FloorToInt(cx - half); x <= Mathf.CeilToInt(cx + half); x++)
                {
                    img.Set(x, Mathf.RoundToInt(baseY + yy), c);
                }
            }
        }

        // ------------------------------------------------------------------
        // Árboles
        // ------------------------------------------------------------------

        static void Pine(string name, int w, int h, int seed, float widthFactor)
        {
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            float cx = w * 0.5f;
            Color dark = new Color(0.06f, 0.13f, 0.15f);
            Color light = new Color(0.24f, 0.41f, 0.38f);
            Color bark = new Color(0.16f, 0.12f, 0.11f);

            int trunkTop = Mathf.RoundToInt(h * 0.86f);
            for (int y = 0; y < trunkTop; y++)
            {
                float half = Mathf.Lerp(w * 0.038f, w * 0.012f, y / (float)trunkTop);
                for (int x = Mathf.FloorToInt(cx - half); x <= Mathf.CeilToInt(cx + half); x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.35f, y * 0.04f + seed);
                    float side = x > cx ? 1.25f : 0.8f;
                    img.Set(x, y, DemoImg.Mul(Color.Lerp(DemoImg.Mul(bark, 0.7f), DemoImg.Mul(bark, 1.4f), n), side));
                }
            }

            const int tiers = 10;
            for (int i = 0; i < tiers; i++)
            {
                float f = i / (float)(tiers - 1);
                float yb = Mathf.Lerp(h * 0.12f, h * 0.80f, f);
                float ht = Mathf.Lerp(h * 0.23f, h * 0.18f, f);
                float hw = Mathf.Lerp(w * 0.5f, w * 0.14f, Mathf.Pow(f, 0.9f)) * widthFactor;
                float jitter = ((float)rng.NextDouble() * 2f - 1f) * w * 0.02f;

                for (int y = Mathf.RoundToInt(yb); y < Mathf.RoundToInt(yb + ht); y++)
                {
                    float tt = (y - yb) / ht;
                    float half = hw * (1f - tt);
                    for (int x = Mathf.FloorToInt(cx + jitter - half); x <= Mathf.CeilToInt(cx + jitter + half); x++)
                    {
                        float u = hw > 0f ? (x - cx - jitter) / hw : 0f;
                        float edge = yb + 7f * Mathf.Abs(Mathf.Sin(x * 0.45f + i * 1.7f)) * (1f - Mathf.Abs(u) * 0.35f);
                        if (y < edge)
                        {
                            continue;
                        }

                        float n = Mathf.PerlinNoise(x * 0.12f + seed * 0.37f, y * 0.12f + i * 5.1f);
                        float needles = 0.65f + 0.35f * Mathf.PerlinNoise(x * 0.55f, y * 0.95f + i);
                        float lat = 0.5f + 0.5f * Mathf.Clamp(u, -1f, 1f);
                        float v = (0.18f + 0.42f * lat + 0.25f * n) * needles;
                        v *= Mathf.Lerp(0.55f, 1.05f, tt * 0.7f + 0.3f);
                        img.Set(x, y, Color.Lerp(dark, light, Mathf.Clamp01(v)));
                    }
                }
            }

            img.Save(P(name), 32f, FilterMode.Bilinear);
        }

        static void Broadleaf(string name, int w, int h, int seed, float autumn)
        {
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            float cx = w * 0.5f;
            Color bark = new Color(0.17f, 0.13f, 0.12f);

            int trunkTop = Mathf.RoundToInt(h * 0.62f);
            for (int y = 0; y < trunkTop; y++)
            {
                float half = Mathf.Lerp(w * 0.05f, w * 0.03f, y / (float)trunkTop);
                for (int x = Mathf.FloorToInt(cx - half); x <= Mathf.CeilToInt(cx + half); x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.3f, y * 0.05f + seed);
                    img.Set(x, y, DemoImg.Mul(Color.Lerp(DemoImg.Mul(bark, 0.7f), DemoImg.Mul(bark, 1.4f), n), x > cx ? 1.2f : 0.8f));
                }
            }

            // ramas principales
            img.Capsule(cx, h * 0.5f, cx - w * 0.22f, h * 0.68f, 3.5f, 2f, bark);
            img.Capsule(cx, h * 0.52f, cx + w * 0.24f, h * 0.7f, 3.5f, 2f, bark);

            Color[] greens =
            {
                new Color(0.10f, 0.22f, 0.22f), new Color(0.14f, 0.29f, 0.27f), new Color(0.19f, 0.36f, 0.31f)
            };
            Color[] autumnC =
            {
                new Color(0.55f, 0.33f, 0.16f), new Color(0.66f, 0.43f, 0.19f), new Color(0.44f, 0.24f, 0.14f)
            };

            float ccx = cx;
            float ccy = h * 0.70f;
            float crx = w * 0.46f;
            float cry = h * 0.27f;
            const int blobs = 46;
            for (int i = 0; i < blobs; i++)
            {
                float a = (float)rng.NextDouble() * Mathf.PI * 2f;
                float rr = Mathf.Sqrt((float)rng.NextDouble());
                float bx = ccx + Mathf.Cos(a) * rr * crx;
                float by = ccy + Mathf.Sin(a) * rr * cry;
                float br = w * (0.08f + (float)rng.NextDouble() * 0.09f);
                bool isAutumn = rng.NextDouble() < autumn * 0.55f && by > ccy - cry * 0.2f;
                Color[] pal = isAutumn ? autumnC : greens;
                float lightness = Mathf.Clamp01(0.5f + ((bx - ccx) / crx) * 0.25f + ((by - ccy) / cry) * 0.35f);
                Color baseC = pal[Mathf.Clamp(Mathf.FloorToInt(lightness * pal.Length), 0, pal.Length - 1)];
                img.Disc(bx, by, br, DemoImg.Mul(baseC, 0.65f));
                img.Disc(bx + br * 0.2f, by + br * 0.22f, br * 0.78f, baseC);
            }

            img.Save(P(name), 32f, FilterMode.Bilinear);
        }

        static void ForegroundTrunk(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            Color bark = new Color(0.09f, 0.11f, 0.13f);
            for (int y = 0; y < h; y++)
            {
                float flare = Mathf.Lerp(1.35f, 1f, Mathf.Clamp01(y / (h * 0.18f)));
                float sway = Mathf.Sin(y * 0.012f + seed) * w * 0.04f;
                float half = w * 0.36f * flare;
                float c = w * 0.5f + sway;
                for (int x = Mathf.FloorToInt(c - half); x <= Mathf.CeilToInt(c + half); x++)
                {
                    float u = (x - c) / half;
                    float streak = Mathf.PerlinNoise(x * 0.22f + seed, y * 0.015f);
                    float v = 0.55f + 0.5f * streak - 0.25f * Mathf.Abs(u);
                    Color col = DemoImg.Mul(bark, v + 0.35f * Mathf.Clamp01(-u));
                    if (u < -0.35f && Mathf.PerlinNoise(x * 0.08f, y * 0.03f + seed) > 0.55f)
                    {
                        col = Color.Lerp(col, new Color(0.13f, 0.24f, 0.2f), 0.7f); // musgo en el lado izquierdo
                    }

                    img.Set(x, y, col);
                }
            }

            img.Save(P(name), 32f, FilterMode.Bilinear);
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        static void Rock(string name, int w, int h, int seed, bool moss)
        {
            var img = new DemoImg(w, h);
            float cx = w * 0.5f;
            float cy = h * 0.46f;
            float rx = w * 0.47f;
            float ry = h * 0.50f;
            Vector3 light = new Vector3(0.45f, 0.65f, 0.6f).normalized;
            Color lit = new Color(0.60f, 0.68f, 0.70f);
            Color shadow = new Color(0.13f, 0.17f, 0.22f);
            Color mossC = new Color(0.22f, 0.38f, 0.30f);

            for (int y = Mathf.RoundToInt(h * 0.05f); y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f - cx) / rx;
                    float ny = (y + 0.5f - cy) / ry;
                    float ang = Mathf.Atan2(ny, nx);
                    float r = 1f + 0.16f * (Mathf.PerlinNoise(Mathf.Cos(ang) * 1.6f + seed, Mathf.Sin(ang) * 1.6f + seed) - 0.5f) * 2f;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d > r)
                    {
                        continue;
                    }

                    float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - Mathf.Min(1f, d * d)));
                    Vector3 n = new Vector3(nx, ny, nz * 0.9f).normalized;
                    float lam = Mathf.Clamp01(Vector3.Dot(n, light) * 0.8f + 0.2f);
                    float speckle = Mathf.PerlinNoise(x * 0.35f + seed, y * 0.35f);
                    Color c = Color.Lerp(shadow, lit, Mathf.Clamp01(lam * (0.8f + 0.4f * speckle)));
                    if (moss && ny > 0.1f + 0.35f * Mathf.PerlinNoise(x * 0.09f, seed))
                    {
                        c = Color.Lerp(c, DemoImg.Mul(mossC, 0.6f + lam * 0.9f), 0.85f);
                    }

                    float ao = Mathf.Clamp01((y - h * 0.05f) / (h * 0.12f));
                    img.Set(x, y, DemoImg.Mul(c, Mathf.Lerp(0.55f, 1f, ao)));
                }
            }

            img.OuterOutline(new Color(0.06f, 0.08f, 0.1f, 0.85f));
            img.Save(P(name), 32f, FilterMode.Bilinear, new Vector2(0.5f, 0.08f));
        }

        /// <summary>Caja de madera vieja: marco oscuro, tablones horizontales y travesaño en diagonal.</summary>
        static void Crate(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            Color wood = new Color(0.50f, 0.36f, 0.24f);
            Color frame = new Color(0.30f, 0.21f, 0.15f);
            const int border = 4;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool isFrame = x < border || y < border || x >= w - border || y >= h - border;
                    float grain = 0.80f + 0.35f * Mathf.PerlinNoise(x * 0.9f + seed, y * 0.12f);
                    int plank = (y - border) / 8;
                    bool seam = !isFrame && (y - border) % 8 == 0;
                    Color c = isFrame ? DemoImg.Mul(frame, grain) : DemoImg.Mul(wood, grain * (0.92f + 0.08f * (plank % 2)));
                    if (seam)
                    {
                        c = DemoImg.Mul(frame, 0.8f);
                    }

                    // Travesaño diagonal
                    if (!isFrame && Mathf.Abs((x - border) - (y - border) * (float)(w - 2 * border) / (h - 2 * border)) < 2.5f)
                    {
                        c = DemoImg.Mul(frame, 1.15f * grain);
                    }

                    // Luz suave desde arriba
                    c = DemoImg.Mul(c, 0.82f + 0.25f * (y / (float)h));
                    img.Set(x, y, c);
                }
            }

            // Clavos
            Color nail = new Color(0.62f, 0.60f, 0.55f);
            img.Disc(2f, 2f, 1f, nail);
            img.Disc(w - 3f, 2f, 1f, nail);
            img.Disc(2f, h - 3f, 1f, nail);
            img.Disc(w - 3f, h - 3f, 1f, nail);
            img.OuterOutline(new Color(0.06f, 0.07f, 0.09f, 0.85f));
            img.Save(P(name), 32f, FilterMode.Point);
        }

        static void FallenLog(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            float ry = h * 0.5f - 1f;
            float cy = h * 0.5f;
            Color bark = new Color(0.34f, 0.26f, 0.21f);
            Color mossC = new Color(0.22f, 0.40f, 0.30f);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dxEnd = Mathf.Max(0f, Mathf.Max(ry - x, x - (w - ry)));
                    float ny = (y + 0.5f - cy) / ry;
                    float nd = Mathf.Sqrt((dxEnd / ry) * (dxEnd / ry) + ny * ny);
                    if (nd > 1f)
                    {
                        continue;
                    }

                    float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - ny * ny));
                    float lam = Mathf.Clamp01(0.30f + 0.55f * ny + 0.45f * nz);
                    float stripe = 0.7f + 0.5f * Mathf.PerlinNoise(x * 0.06f + seed, y * 0.7f);
                    Color c = DemoImg.Mul(bark, lam * stripe + 0.1f);
                    if (ny > 0.35f && Mathf.PerlinNoise(x * 0.07f + seed, 1.3f) > 0.42f)
                    {
                        c = Color.Lerp(c, DemoImg.Mul(mossC, 0.6f + lam), 0.85f);
                    }

                    img.Set(x, y, c);
                }
            }

            img.OuterOutline(new Color(0.06f, 0.07f, 0.09f, 0.85f));
            img.Save(P(name), 32f, FilterMode.Bilinear);
        }

        static void Fern(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            Color dark = new Color(0.08f, 0.20f, 0.18f);
            Color light = new Color(0.26f, 0.46f, 0.38f);
            float bx = w * 0.5f;
            const float by = 3f;
            int fronds = 9;
            for (int i = 0; i < fronds; i++)
            {
                float f = i / (float)(fronds - 1);
                float angle = Mathf.Lerp(-72f, 72f, f) + ((float)rng.NextDouble() - 0.5f) * 8f;
                float len = h * (0.62f + 0.3f * (1f - Mathf.Abs(f - 0.5f) * 2f)) * (0.85f + 0.15f * (float)rng.NextDouble());
                Vector2 dir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
                Vector2 perp = new Vector2(dir.y, -dir.x);
                float bend = Mathf.Sign(angle) * len * 0.22f * Mathf.Abs(angle) / 72f;
                Vector2 prev = new Vector2(bx, by);
                for (int s = 1; s <= 22; s++)
                {
                    float t = s / 22f;
                    Vector2 pos = new Vector2(bx, by) + dir * (t * len) + new Vector2(bend * t * t, -len * 0.18f * t * t);
                    Color c = Color.Lerp(dark, light, t);
                    img.Capsule(prev.x, prev.y, pos.x, pos.y, 1.1f, 0.9f, DemoImg.Mul(c, 0.8f));
                    float leaf = (1f - t) * 11f + 2f;
                    Vector2 tangent = (pos - prev).normalized;
                    Vector2 side = new Vector2(tangent.y, -tangent.x);
                    img.Capsule(pos.x, pos.y, pos.x + side.x * leaf + tangent.x * 3f, pos.y + side.y * leaf + tangent.y * 3f, 1.3f, 0.4f, c);
                    img.Capsule(pos.x, pos.y, pos.x - side.x * leaf + tangent.x * 3f, pos.y - side.y * leaf + tangent.y * 3f, 1.3f, 0.4f, DemoImg.Mul(c, 0.85f));
                    prev = pos;
                }
            }

            img.Save(P(name), 32f, FilterMode.Bilinear, new Vector2(0.5f, 0.03f));
        }

        static void GrassTuft(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            Color dark = new Color(0.10f, 0.25f, 0.21f);
            Color light = new Color(0.40f, 0.55f, 0.40f);
            for (int i = 0; i < 16; i++)
            {
                float f = i / 15f;
                float x0 = w * 0.5f + (f - 0.5f) * w * 0.4f;
                float angle = (f - 0.5f) * 70f + ((float)rng.NextDouble() - 0.5f) * 14f;
                float len = h * (0.55f + 0.4f * (float)rng.NextDouble());
                Vector2 dir = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
                Vector2 prev = new Vector2(x0, 1f);
                for (int s = 1; s <= 10; s++)
                {
                    float t = s / 10f;
                    Vector2 pos = new Vector2(x0, 1f) + dir * (t * len) + new Vector2(Mathf.Sign(angle) * len * 0.15f * t * t, 0f);
                    img.Capsule(prev.x, prev.y, pos.x, pos.y, 1.6f * (1f - t) + 0.4f, 1.6f * (1f - t) + 0.4f, Color.Lerp(dark, light, t));
                    prev = pos;
                }
            }

            img.Save(P(name), 32f, FilterMode.Bilinear, new Vector2(0.5f, 0.03f));
        }

        static void Bush(string name, int w, int h, int seed)
        {
            var img = new DemoImg(w, h);
            var rng = new System.Random(seed);
            Color[] greens =
            {
                new Color(0.07f, 0.17f, 0.17f), new Color(0.12f, 0.26f, 0.24f), new Color(0.19f, 0.36f, 0.31f)
            };
            for (int i = 0; i < 26; i++)
            {
                float f = (float)rng.NextDouble();
                float bx = Mathf.Lerp(w * 0.15f, w * 0.85f, f);
                float arch = 1f - Mathf.Pow((f - 0.5f) * 2f, 2f);
                float by = h * 0.18f + arch * h * 0.42f * (float)rng.NextDouble();
                float br = h * (0.14f + 0.12f * (float)rng.NextDouble());
                float lightness = Mathf.Clamp01(by / (h * 0.6f) + (f - 0.5f) * 0.3f);
                Color c = greens[Mathf.Clamp(Mathf.FloorToInt(lightness * 3f), 0, 2)];
                img.Disc(bx, by, br, DemoImg.Mul(c, 0.7f));
                img.Disc(bx + br * 0.15f, by + br * 0.2f, br * 0.75f, c);
            }

            img.Save(P(name), 32f, FilterMode.Bilinear, new Vector2(0.5f, 0.05f));
        }

        // ------------------------------------------------------------------
        // Indicador de interacción (tecla E)
        // ------------------------------------------------------------------

        static void PromptKey(string name)
        {
            var img = new DemoImg(28, 28);
            Color outline = new Color(0.10f, 0.12f, 0.16f, 1f);
            Color fill = new Color(0.96f, 0.92f, 0.80f, 1f);
            for (int y = 0; y < 28; y++)
            {
                for (int x = 0; x < 28; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Abs(x + 0.5f - 14f) - 9f);
                    float dy = Mathf.Max(0f, Mathf.Abs(y + 0.5f - 14f) - 9f);
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= 4f)
                    {
                        img.Set(x, y, d > 3f ? outline : fill);
                    }
                }
            }

            string[] glyph = { "#####", "#....", "#....", "####.", "#....", "#....", "#####" };
            for (int gy = 0; gy < glyph.Length; gy++)
            {
                for (int gx = 0; gx < 5; gx++)
                {
                    if (glyph[gy][gx] != '#')
                    {
                        continue;
                    }

                    for (int py = 0; py < 2; py++)
                    {
                        for (int px = 0; px < 2; px++)
                        {
                            img.Set(9 + gx * 2 + px, 7 + (glyph.Length - 1 - gy) * 2 + py, outline);
                        }
                    }
                }
            }

            img.Save(P(name), 32f, FilterMode.Point);
        }

        // ------------------------------------------------------------------
        // Terreno orgánico continuo
        // ------------------------------------------------------------------

        static float Hash(int x, int salt)
        {
            unchecked
            {
                int h = x * 73856093 ^ salt * 19349663;
                h = (h << 13) ^ h;
                return ((h * (h * h * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483647f;
            }
        }

        static void GenerateTerrain()
        {
            int w = Mathf.RoundToInt((DemoTerrain.MaxX - DemoTerrain.MinX) * DemoTerrain.Ppu);
            int h = Mathf.RoundToInt((DemoTerrain.TopY - DemoTerrain.BottomY) * DemoTerrain.Ppu);
            var img = new DemoImg(w, h);

            var top = new float[w];
            for (int x = 0; x < w; x++)
            {
                float wx = DemoTerrain.MinX + (x + 0.5f) / DemoTerrain.Ppu;
                top[x] = (DemoTerrain.Surface(wx) - DemoTerrain.BottomY) * DemoTerrain.Ppu;
            }

            Color rim = new Color(0.50f, 0.66f, 0.56f);
            Color moss = new Color(0.24f, 0.40f, 0.34f);
            Color rootSoil = new Color(0.18f, 0.26f, 0.26f);
            Color soilTop = new Color(0.19f, 0.22f, 0.25f);
            Color soilDeep = new Color(0.06f, 0.08f, 0.11f);
            Color stone = new Color(0.30f, 0.35f, 0.37f);

            for (int x = 0; x < w; x++)
            {
                int yMax = Mathf.Min(h - 1, Mathf.CeilToInt(top[x]));
                float wx = DemoTerrain.MinX + (x + 0.5f) / DemoTerrain.Ppu;
                for (int y = 0; y <= yMax; y++)
                {
                    float depth = top[x] - (y + 0.5f);
                    if (depth < 0f)
                    {
                        continue;
                    }

                    // Cara lateral expuesta (paredes del barranco y del risco).
                    for (int d = 1; d <= 6; d++)
                    {
                        int xl = x - d;
                        int xr = x + d;
                        if ((xl >= 0 && top[xl] < y) || (xr < w && top[xr] < y))
                        {
                            depth = Mathf.Min(depth, d - 0.5f);
                            break;
                        }
                    }

                    float n = Mathf.PerlinNoise(wx * 2.3f, y * 0.07f);
                    Color c;
                    if (depth < 3f)
                    {
                        c = Color.Lerp(rim, moss, depth / 3f);
                        c = DemoImg.Mul(c, 0.85f + 0.3f * n);
                    }
                    else if (depth < 10f)
                    {
                        c = Color.Lerp(moss, rootSoil, (depth - 3f) / 7f);
                        c = DemoImg.Mul(c, 0.8f + 0.35f * n);
                    }
                    else
                    {
                        float t = Mathf.Clamp01((depth - 10f) / 240f);
                        c = Color.Lerp(Color.Lerp(rootSoil, soilTop, Mathf.Clamp01((depth - 10f) / 30f)), soilDeep, t);
                        float strata = 0.05f * Mathf.Sin(y * 0.33f + n * 4f);
                        c = DemoImg.Mul(c, 0.9f + 0.25f * n + strata);
                        if (Mathf.PerlinNoise(wx * 4.1f + 9f, y * 0.11f + 3f) > 0.76f)
                        {
                            c = Color.Lerp(c, stone, 0.6f * (1f - t));
                        }
                    }

                    img.Set(x, y, c);
                }
            }

            // Hierba: briznas y matas sobre la superficie.
            Color bladeDark = new Color(0.14f, 0.30f, 0.26f);
            Color bladeLight = new Color(0.42f, 0.58f, 0.44f);
            for (int x = 1; x < w - 1; x++)
            {
                float wx = DemoTerrain.MinX + (x + 0.5f) / DemoTerrain.Ppu;
                if (Mathf.Abs(top[x] - top[x - 1]) > 2.5f || Mathf.Abs(top[x] - top[x + 1]) > 2.5f)
                {
                    continue;
                }

                if (Hash(x, 1) > 0.55f)
                {
                    int bladeH = 2 + Mathf.FloorToInt(Hash(x, 2) * 6f);
                    int lean = Hash(x, 3) > 0.5f ? 1 : -1;
                    for (int k = 0; k < bladeH; k++)
                    {
                        int px = x + (k > bladeH / 2 ? lean : 0);
                        img.Set(px, Mathf.RoundToInt(top[x]) + k, Color.Lerp(bladeDark, bladeLight, k / (float)bladeH));
                    }
                }

                if (x % 29 == Mathf.FloorToInt(Hash(x / 29, 7) * 29f))
                {
                    for (int b = -2; b <= 2; b++)
                    {
                        int bh = 8 + Mathf.FloorToInt(Hash(x + b, 5) * 9f) - Mathf.Abs(b) * 2;
                        for (int k = 0; k < bh; k++)
                        {
                            float t = k / (float)bh;
                            int px = x + b * 2 + Mathf.RoundToInt(b * t * 3f);
                            img.Set(px, Mathf.RoundToInt(top[x]) + k, Color.Lerp(bladeDark, bladeLight, t));
                        }
                    }
                }
            }

            img.Save(P("terrain_main"), DemoTerrain.Ppu, FilterMode.Bilinear, null, 8192);
        }
    }
}
