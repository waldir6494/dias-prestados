using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Sprites del tramo orgánico en Assets/Art/ForestOrganic. Cada sprite se rasteriza a partir de
    /// ForestOrganicLayout (los mismos puntos que usan los colliders), de modo que la silueta visible
    /// coincide con la física. Paleta "Memoria desvanecida". Arte provisional procedural.
    /// </summary>
    public static class ForestOrganicArtGenerator
    {
        public const string ArtFolder = "Assets/Art/ForestOrganic";

        public static class Paths
        {
            public const string Terrain = ArtFolder + "/terrain_organic.png";
            public const string Log = ArtFolder + "/log_fallen.png";
            public const string Boulder = ArtFolder + "/boulder_solid.png";
            public const string Deck = ArtFolder + "/rail_deck.png";
            public const string Pillar = ArtFolder + "/rail_pillar.png";
            public const string Track = ArtFolder + "/rail_track_ground.png";
        }

        public static void EnsureAllSprites()
        {
            ProceduralSpriteUtil.EnsureFolder("Assets/Art");
            ProceduralSpriteUtil.EnsureFolder(ArtFolder);

            Write(Paths.Terrain, Terrain(), ForestOrganicLayout.Ppu);
            Write(Paths.Log, LogTexture(), ForestOrganicLayout.Ppu);
            Write(Paths.Boulder, Boulder(), ForestOrganicLayout.BoulderPpu);
            Write(Paths.Deck, Deck(), ForestOrganicLayout.DeckPpu);
            Write(Paths.Pillar, Pillar(), ForestOrganicLayout.DeckPpu);
            Write(Paths.Track, Track(), ForestOrganicLayout.Ppu);
            AssetDatabase.Refresh();
        }

        static void Write(string path, Texture2D tex, float ppu)
        {
            ProceduralSpriteUtil.WriteSprite(path, tex, ppu, FilterMode.Bilinear);
        }

        // ------------------------------------------------------------------
        // Utilidades
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
            return Mathf.Lerp(
                Mathf.Lerp(Hash(xi, yi, seed), Hash(xi + 1, yi, seed), tx),
                Mathf.Lerp(Hash(xi, yi + 1, seed), Hash(xi + 1, yi + 1, seed), tx),
                ty);
        }

        static float Fbm(float x, float y, int seed, int octaves = 3)
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

        /// <summary>Pinta solo sobre píxeles ya opacos (raíces, grietas): nunca amplía la silueta.</summary>
        static void PutInside(Color[] px, int w, int h, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            Color d = px[y * w + x];
            if (d.a < 0.9f)
            {
                return;
            }

            Color o = Color.Lerp(d, c, c.a);
            o.a = d.a;
            px[y * w + x] = o;
        }

        static Color C(string hex)
        {
            return AtmospherePalette.Hex(hex);
        }

        static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(0.0001f, ab.sqrMagnitude));
            return Vector2.Distance(p, a + ab * t);
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
        // Terreno: heightfield con tierra/musgo en las superficies y roca en las paredes
        // ------------------------------------------------------------------

        static Texture2D Terrain()
        {
            int w = ForestOrganicLayout.TerrainTexWidth;
            int h = ForestOrganicLayout.TerrainTexHeight;
            float ppu = ForestOrganicLayout.Ppu;
            var px = NewBuffer(w, h);

            var sy = new float[w];
            for (int x = 0; x < w; x++)
            {
                sy[x] = ForestOrganicLayout.SurfaceY(ForestOrganicLayout.TerrainMinX + (x + 0.5f) / ppu);
            }

            Color rim = C("C3C7B4");
            Color lip = C("8C9A92");
            Color mossA = C("56685F");
            Color mossB = C("4A5C57");
            Color bodyTop = C("3F5052");
            Color bodyDeep = AtmospherePalette.Slate;
            Color stoneLight = C("66777A");
            Color stoneDark = C("33434A");
            Color stoneEdge = C("A9B9B2");

            for (int y = 0; y < h; y++)
            {
                float wy = ForestOrganicLayout.TerrainBottom + (y + 0.5f) / ppu;
                for (int x = 0; x < w; x++)
                {
                    float dv = sy[x] - wy;
                    if (dv <= -1f / ppu)
                    {
                        continue;
                    }

                    int xa = Mathf.Max(0, x - 2);
                    int xb = Mathf.Min(w - 1, x + 2);
                    float slope = (sy[xb] - sy[xa]) / Mathf.Max(0.0001f, (xb - xa) / ppu);
                    float cos = 1f / Mathf.Sqrt(1f + slope * slope);
                    float depth = dv * ppu * cos;
                    float alpha = Mathf.Clamp01(depth + 0.5f);
                    if (alpha <= 0f)
                    {
                        continue;
                    }

                    float steep = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.42f, 0.95f, Mathf.Abs(slope)));
                    float n = Fbm(x / 14f, y / 4f, 61, 3) - 0.5f;
                    float nFine = Hash(x, y, 7) - 0.5f;

                    // Tierra / musgo (superficies casi horizontales).
                    Color earth;
                    if (depth < 1.8f)
                    {
                        earth = rim;
                    }
                    else if (depth < 3.6f)
                    {
                        earth = lip;
                    }
                    else if (depth < 11f)
                    {
                        earth = Color.Lerp(mossA, mossB, (depth - 3.6f) / 7.4f);
                        earth += new Color(n, n, n, 0f) * 0.14f;
                    }
                    else
                    {
                        float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(11f, 85f, depth));
                        earth = Color.Lerp(bodyTop, bodyDeep, k);
                        float strata = Mathf.Abs(Mathf.Sin((wy * 11f) + n * 5f)) > 0.93f ? -0.06f : 0f;
                        earth += new Color(n + strata, n + strata, n + strata, 0f) * 0.12f;
                    }

                    // Roca erosionada (paredes y salientes): estratos y bloques.
                    float strataRock = Mathf.Sin(wy * 26f + x * 0.05f + n * 6f);
                    float block = Fbm(x / 6f, y / 3.2f, 83, 3);
                    float rockK = Mathf.Clamp01(0.5f + strataRock * 0.18f + (block - 0.5f) * 0.9f);
                    Color rock = Color.Lerp(stoneDark, stoneLight, rockK);
                    if (depth < 1.6f)
                    {
                        rock = Color.Lerp(rock, stoneEdge, 0.8f);
                    }

                    Color c = Color.Lerp(earth, rock, steep);
                    c += new Color(nFine, nFine, nFine, 0f) * 0.03f;

                    // Oscurece hacia el fondo para fundir con la bruma.
                    float deepFade = Mathf.Clamp01((-0.9f - wy) / 1.3f);
                    c = Color.Lerp(c, bodyDeep, deepFade * 0.55f);
                    c.a = alpha;
                    px[y * w + x] = c;
                }
            }

            DrawRoots(px, w, h, sy);
            return ToTexture(px, w, h);
        }

        /// <summary>Raíces que se aferran a las caras del terraplén y de la meseta (solo dentro de la silueta).</summary>
        static void DrawRoots(Color[] px, int w, int h, float[] sy)
        {
            float ppu = ForestOrganicLayout.Ppu;
            Color dark = C("1F2827");
            Color hi = C("62705F");
            float[] starts = { -12.6f, -12.0f, -11.2f, -10.5f, -9.9f, -9.2f, -8.6f, -7.6f, -7.0f, -6.6f };
            for (int s = 0; s < starts.Length; s++)
            {
                float wx = starts[s];
                float wy = ForestOrganicLayout.SurfaceY(wx);
                float angle = -80f + (Hash(s, 4, 19) - 0.5f) * 40f;
                float length = 0.6f + Hash(s, 5, 19) * 0.7f;
                int steps = Mathf.RoundToInt(length * ppu);
                float phase = Hash(s, 6, 19) * 6.28f;
                float x = (wx - ForestOrganicLayout.TerrainMinX) * ppu;
                float y = (wy - ForestOrganicLayout.TerrainBottom) * ppu;
                for (int i = 0; i < steps; i++)
                {
                    float t = i / (float)steps;
                    float a = (angle + Mathf.Sin(t * 7f + phase) * 28f) * Mathf.Deg2Rad;
                    x += Mathf.Cos(a) * 0.9f;
                    y += Mathf.Sin(a) * 0.9f;
                    float thick = Mathf.Lerp(2.3f, 0.5f, t);
                    int ix = Mathf.RoundToInt(x);
                    int iy = Mathf.RoundToInt(y);
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -2; dy <= 2; dy++)
                        {
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            if (d <= thick * 0.5f + 0.4f)
                            {
                                PutInside(px, w, h, ix + dx, iy + dy, new Color(dark.r, dark.g, dark.b, 0.9f));
                            }
                        }
                    }

                    PutInside(px, w, h, ix - 1, iy + 1, new Color(hi.r, hi.g, hi.b, 0.35f * (1f - t)));
                }
            }
        }

        // ------------------------------------------------------------------
        // Tronco caído
        // ------------------------------------------------------------------

        static Texture2D LogTexture()
        {
            int w = ForestOrganicLayout.LogTexLength;
            int h = ForestOrganicLayout.LogTexThickness;
            var px = NewBuffer(w, h);
            Color rim = C("B1B49E");
            Color topFace = C("858874");
            Color barkLight = C("56564C");
            Color barkDark = C("272E2E");
            Color groove = C("1C2323");
            Color moss = C("5A765E");

            for (int x = 0; x < w; x++)
            {
                float tx = x / (float)(w - 1);
                // Más grueso en la base (izquierda) y más fino y astillado hacia la copa.
                float thickness = Mathf.Lerp(h, h * 0.74f, tx) + (Fbm(x / 18f, 2.3f, 5, 3) - 0.5f) * 3.2f;
                int bottom = Mathf.Max(0, Mathf.RoundToInt(h - thickness));
                // Extremos rotos e irregulares.
                float jagL = 5f + Hash(x, 1, 3) * 0f;
                float jagR = w - 1 - 5f;
                for (int y = bottom; y < h; y++)
                {
                    float jL = 4f + (Hash(y, 2, 5)) * 6f;
                    float jR = w - 1 - (4f + Hash(y, 3, 5) * 9f);
                    if (x < jL || x > jR)
                    {
                        continue;
                    }

                    int d = h - 1 - y;
                    float v = (float)(y - bottom) / Mathf.Max(1, h - bottom);
                    Color c;
                    if (d < 2)
                    {
                        c = rim;
                    }
                    else if (d < 5)
                    {
                        c = Color.Lerp(topFace, barkLight, (d - 2f) / 3f);
                    }
                    else
                    {
                        c = Color.Lerp(barkDark, barkLight, Mathf.Clamp01(v * 1.1f));
                    }

                    float g = Fbm(x / 26f, y / 1.8f, 31, 3);
                    if (g > 0.62f && d >= 3)
                    {
                        c = Color.Lerp(c, groove, 0.55f);
                    }

                    float m = Fbm(x / 14f, y / 3f, 47, 3);
                    if (m > 0.6f && d >= 2 && d < 8)
                    {
                        c = Color.Lerp(c, moss, 0.55f);
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }

                _ = jagL;
                _ = jagR;
            }

            // Nudos y muñones de ramas que cuelgan por debajo.
            int[] knots = { Mathf.RoundToInt(w * 0.18f), Mathf.RoundToInt(w * 0.46f), Mathf.RoundToInt(w * 0.72f) };
            for (int k = 0; k < knots.Length; k++)
            {
                int kx = knots[k];
                int ky = Mathf.RoundToInt(h * (0.35f + 0.1f * k));
                for (int dy = -3; dy <= 3; dy++)
                {
                    for (int dx = -4; dx <= 4; dx++)
                    {
                        float d = Mathf.Sqrt(dx * dx * 0.6f + dy * dy);
                        if (d < 3.2f)
                        {
                            PutInside(px, w, h, kx + dx, ky + dy, new Color(groove.r, groove.g, groove.b, d < 2.2f ? 0.8f : 0.45f));
                        }
                    }
                }

                // Muñón triangular bajo el tronco.
                for (int i = 0; i < 5; i++)
                {
                    int by = Mathf.Max(0, ky - 6 - i);
                    int half = 3 - i / 2;
                    for (int dx = -half; dx <= half; dx++)
                    {
                        Put(px, w, h, kx + dx + k, by, new Color(barkDark.r, barkDark.g, barkDark.b, 1f));
                    }
                }
            }

            return ToTexture(px, w, h);
        }

        // ------------------------------------------------------------------
        // Peñasco sólido facetado
        // ------------------------------------------------------------------

        static Texture2D Boulder()
        {
            Vector2[] pts = ForestOrganicLayout.BoulderPoints;
            float ppu = ForestOrganicLayout.BoulderPpu;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (Vector2 p in pts)
            {
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }

            int w = Mathf.RoundToInt((maxX - minX) * ppu);
            int h = Mathf.RoundToInt((maxY - minY) * ppu);
            var local = new Vector2[pts.Length];
            for (int i = 0; i < pts.Length; i++)
            {
                local[i] = new Vector2((pts[i].x - minX) * ppu, (pts[i].y - minY) * ppu);
            }

            var px = NewBuffer(w, h);
            Color shade = C("33444A");
            Color baseCol = C("5E6E6D");
            Color lit = C("8FA09B");
            Color edge = C("141D21");
            Color moss = C("55705A");
            Color topEdge = C("B3C2BA");
            Vector2 center = new Vector2(w * 0.5f, h * 0.45f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    if (!PointInPolygon(p, local))
                    {
                        continue;
                    }

                    float dist = float.MaxValue;
                    for (int i = 0, j = local.Length - 1; i < local.Length; j = i++)
                    {
                        dist = Mathf.Min(dist, DistToSegment(p, local[j], local[i]));
                    }

                    float ly = y / (float)h;
                    float lx = x / (float)w;
                    float light = Mathf.Clamp01(ly * 0.85f + (1f - lx) * 0.3f);
                    float ang = Mathf.Atan2(p.y - center.y, p.x - center.x);
                    int sector = Mathf.FloorToInt((ang + Mathf.PI) / (Mathf.PI * 2f) * 9f);
                    float facet = (Hash(sector, 3, 29) - 0.5f) * 0.28f;
                    Color c = Color.Lerp(shade, baseCol, Mathf.Clamp01(light + facet));
                    if (ly > 0.7f)
                    {
                        c = Color.Lerp(c, lit, (ly - 0.7f) / 0.3f * 0.85f);
                    }

                    float n = Fbm(x / 5f, y / 5f, 71, 3) - 0.5f;
                    c += new Color(n, n, n, 0f) * 0.18f;
                    if (ly > 0.8f && n > -0.04f)
                    {
                        c = Color.Lerp(c, moss, 0.55f);
                    }

                    // Grieta diagonal.
                    float crack = Mathf.Abs((x - w * 0.42f) - (y - h * 0.25f) * 0.4f);
                    if (crack < 0.9f && y > h * 0.18f && y < h * 0.62f)
                    {
                        c = Color.Lerp(c, edge, 0.75f);
                    }

                    if (dist < 1.4f)
                    {
                        c = edge;
                    }
                    else if (dist < 2.6f)
                    {
                        c = Color.Lerp(c, edge, 0.5f);
                    }

                    // Cima: arista clara que marca la superficie en la que se puede estar.
                    if (ly > 0.9f && dist < 3.6f && dist >= 1.4f)
                    {
                        c = Color.Lerp(c, topEdge, 0.55f);
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        // ------------------------------------------------------------------
        // Pasarela ferroviaria
        // ------------------------------------------------------------------

        static Texture2D Deck()
        {
            int w = ForestOrganicLayout.DeckTexWidth;
            int h = ForestOrganicLayout.DeckTexHeight;
            var px = NewBuffer(w, h);
            Color rim = C("BFC4B0");
            Color plankHi = C("8C8C76");
            Color plankLo = C("5C6155");
            Color seam = C("1D262A");
            Color beamDark = C("1F2A2E");
            Color flangeHi = C("55656B");
            Color flangeLo = C("2A363B");
            Color web = C("34434A");
            Color rust = C("6C5B49");

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int d = h - 1 - y;
                    Color c = Color.clear;
                    bool solid = true;

                    if (d < 2)
                    {
                        c = rim;
                    }
                    else if (d < 9)
                    {
                        float board = Hash(x / 14, 3, 11);
                        c = Color.Lerp(plankHi, plankLo, (d - 2f) / 7f);
                        c = Color.Lerp(c, plankLo, board * 0.35f);
                        if (x % 14 == 0)
                        {
                            c = Color.Lerp(c, seam, 0.85f);
                        }
                    }
                    else if (d < 11)
                    {
                        c = beamDark;
                    }
                    else if (d < 13)
                    {
                        c = Color.Lerp(flangeHi, flangeLo, (d - 11f) * 0.5f);
                    }
                    else if (d < 20)
                    {
                        // Celosía triangulada: huecos entre diagonales y montantes.
                        float u = Mathf.Repeat(x, 20f) / 20f;
                        float rel = (d - 13f) / 6f;
                        float m1 = u;
                        float m2 = 1f - u;
                        bool member = Mathf.Abs(rel - m1) < 0.22f || Mathf.Abs(rel - m2) < 0.22f || Mathf.Repeat(x, 20f) < 2f;
                        solid = member;
                        c = web;
                    }
                    else
                    {
                        c = flangeLo;
                    }

                    // Extremo izquierdo derrumbado: la celosía y el ala inferior se rompen en dientes.
                    if (x < 46 && d >= 11)
                    {
                        float tear = 12f + Hash(x, 7, 13) * 34f;
                        if (x < tear * 0.6f + (d - 11) * 2.2f)
                        {
                            solid = false;
                        }
                    }

                    // Extremo derecho: ala inferior astillada.
                    if (x > w - 26 && d >= 17 && Hash(x, 8, 17) > 0.45f)
                    {
                        solid = false;
                    }

                    if (!solid)
                    {
                        continue;
                    }

                    float r = Fbm(x / 9f, y / 3f, 53, 3);
                    if (r > 0.64f && d >= 2)
                    {
                        c = Color.Lerp(c, rust, 0.4f);
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            // Remaches en el ala superior.
            for (int x = 8; x < w - 6; x += 12)
            {
                Put(px, w, h, x, h - 12, new Color(0.62f, 0.68f, 0.7f, 0.9f));
            }

            return ToTexture(px, w, h);
        }

        static Texture2D Pillar()
        {
            int w = ForestOrganicLayout.PillarTexWidth;
            int h = ForestOrganicLayout.PillarTexHeight;
            var px = NewBuffer(w, h);
            Color flangeHi = C("5E6E73");
            Color flangeLo = C("2B383D");
            Color web = C("3A4A50");
            Color rivet = C("94A4A8");
            Color rust = C("6C5B49");
            Color plate = C("222D31");

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Color c = Color.clear;
                    bool filled = false;
                    if (x >= 3 && x < 7)
                    {
                        c = Color.Lerp(flangeHi, flangeLo, (x - 3f) / 4f);
                        filled = true;
                    }
                    else if (x >= 17 && x < 21)
                    {
                        c = Color.Lerp(flangeLo, flangeHi, 0.15f);
                        filled = true;
                    }
                    else if (x >= 7 && x < 17)
                    {
                        c = web;
                        // Barras diagonales de arriostramiento.
                        float u = Mathf.Repeat(y, 24f) / 24f;
                        float diag = (x - 7f) / 10f;
                        if (Mathf.Abs(u - diag) < 0.09f || Mathf.Abs((1f - u) - diag) < 0.09f)
                        {
                            c = flangeLo;
                        }

                        filled = true;
                    }

                    // Placa base y capitel.
                    if (y < 5 && x >= 1 && x < 23)
                    {
                        c = plate;
                        filled = true;
                    }

                    if (y > h - 6 && x >= 2 && x < 22)
                    {
                        c = plate;
                        filled = true;
                    }

                    if (!filled)
                    {
                        continue;
                    }

                    float r = Fbm(x / 3f, y / 14f, 91, 3);
                    if (r > 0.62f)
                    {
                        c = Color.Lerp(c, rust, 0.45f);
                    }

                    if ((y % 9 == 4) && (x == 4 || x == 19))
                    {
                        c = rivet;
                    }

                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }

            return ToTexture(px, w, h);
        }

        /// <summary>Tramo de vía caída entre los pilares (decoración baja, sin collider).</summary>
        static Texture2D Track()
        {
            int w = ForestOrganicLayout.TrackTexWidth;
            int h = ForestOrganicLayout.TrackTexHeight;
            var px = NewBuffer(w, h);
            Color tie = C("232E31");
            Color rail = C("3C4B50");
            Color railHi = C("7A8C90");
            for (int x = 3; x < w; x += 14)
            {
                if (Hash(x, 2, 9) < 0.12f)
                {
                    continue;
                }

                for (int dx = 0; dx < 8; dx++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        Put(px, w, h, x + dx, y, new Color(tie.r, tie.g, tie.b, 0.95f));
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                // Carril roto en un punto.
                if (x > 118 && x < 127)
                {
                    continue;
                }

                Put(px, w, h, x, 4, new Color(rail.r, rail.g, rail.b, 1f));
                Put(px, w, h, x, 5, new Color(rail.r, rail.g, rail.b, 1f));
                Put(px, w, h, x, 6, new Color(railHi.r, railHi.g, railHi.b, 0.9f));
            }

            return ToTexture(px, w, h);
        }
    }
}
