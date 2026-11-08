using System.IO;
using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>Lienzo RGBA mínimo con mezcla alfa para generar arte procedural del demo.</summary>
    public sealed class DemoImg
    {
        public readonly int W;
        public readonly int H;
        public readonly Color[] P;

        public DemoImg(int w, int h)
        {
            W = w;
            H = h;
            P = new Color[w * h];
        }

        public Color Get(int x, int y)
        {
            return x < 0 || y < 0 || x >= W || y >= H ? Color.clear : P[y * W + x];
        }

        public void Set(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H)
            {
                return;
            }

            P[y * W + x] = c;
        }

        public void Over(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H || c.a <= 0f)
            {
                return;
            }

            int i = y * W + x;
            if (c.a >= 0.999f)
            {
                P[i] = c;
                return;
            }

            Color d = P[i];
            float a = c.a + d.a * (1f - c.a);
            if (a <= 0f)
            {
                return;
            }

            P[i] = new Color(
                (c.r * c.a + d.r * d.a * (1f - c.a)) / a,
                (c.g * c.a + d.g * d.a * (1f - c.a)) / a,
                (c.b * c.a + d.b * d.a * (1f - c.a)) / a,
                a);
        }

        public void Fill(Color c)
        {
            for (int i = 0; i < P.Length; i++)
            {
                P[i] = c;
            }
        }

        /// <summary>Elipse con borde duro (softness 0) o difuminado (0..1).</summary>
        public void Ellipse(float cx, float cy, float rx, float ry, Color c, float softness = 0f)
        {
            int x0 = Mathf.FloorToInt(cx - rx - 1f);
            int x1 = Mathf.CeilToInt(cx + rx + 1f);
            int y0 = Mathf.FloorToInt(cy - ry - 1f);
            int y1 = Mathf.CeilToInt(cy + ry + 1f);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float nx = (x + 0.5f - cx) / Mathf.Max(0.001f, rx);
                    float ny = (y + 0.5f - cy) / Mathf.Max(0.001f, ry);
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d > 1f)
                    {
                        continue;
                    }

                    Color col = c;
                    if (softness > 0f)
                    {
                        col.a *= Mathf.Clamp01((1f - d) / softness);
                    }

                    Over(x, y, col);
                }
            }
        }

        public void Disc(float cx, float cy, float r, Color c)
        {
            Ellipse(cx, cy, r, r, c);
        }

        /// <summary>Segmento con extremos redondeados y radio variable.</summary>
        public void Capsule(float x0, float y0, float x1, float y1, float r0, float r1, Color c)
        {
            float rmax = Mathf.Max(r0, r1);
            int minX = Mathf.FloorToInt(Mathf.Min(x0, x1) - rmax - 1f);
            int maxX = Mathf.CeilToInt(Mathf.Max(x0, x1) + rmax + 1f);
            int minY = Mathf.FloorToInt(Mathf.Min(y0, y1) - rmax - 1f);
            int maxY = Mathf.CeilToInt(Mathf.Max(y0, y1) + rmax + 1f);
            Vector2 a = new Vector2(x0, y0);
            Vector2 b = new Vector2(x1, y1);
            Vector2 ab = b - a;
            float len2 = Mathf.Max(0.0001f, ab.sqrMagnitude);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                    float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
                    float r = Mathf.Lerp(r0, r1, t);
                    if ((p - (a + ab * t)).sqrMagnitude <= r * r)
                    {
                        Over(x, y, c);
                    }
                }
            }
        }

        public DemoImg Upscale(int factor)
        {
            var o = new DemoImg(W * factor, H * factor);
            for (int y = 0; y < o.H; y++)
            {
                for (int x = 0; x < o.W; x++)
                {
                    o.P[y * o.W + x] = P[(y / factor) * W + (x / factor)];
                }
            }

            return o;
        }

        /// <summary>Contorno exterior de 1 px (arte con silueta legible).</summary>
        public void OuterOutline(Color outline)
        {
            var src = (Color[])P.Clone();
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    if (src[y * W + x].a > 0.05f)
                    {
                        continue;
                    }

                    bool near =
                        (x > 0 && src[y * W + x - 1].a > 0.5f) ||
                        (x < W - 1 && src[y * W + x + 1].a > 0.5f) ||
                        (y > 0 && src[(y - 1) * W + x].a > 0.5f) ||
                        (y < H - 1 && src[(y + 1) * W + x].a > 0.5f);
                    if (near)
                    {
                        P[y * W + x] = outline;
                    }
                }
            }
        }

        public static Color Mul(Color c, float f)
        {
            return new Color(Mathf.Clamp01(c.r * f), Mathf.Clamp01(c.g * f), Mathf.Clamp01(c.b * f), c.a);
        }

        public static Color A(Color c, float a)
        {
            return new Color(c.r, c.g, c.b, a);
        }

        /// <summary>Escribe el PNG y configura el importador como Sprite.</summary>
        public void Save(string assetPath, float ppu, FilterMode filter, Vector2? pivot = null, int maxSize = 2048)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            tex.SetPixels(P);
            tex.Apply();
            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            ProceduralSpriteUtil.EnsureFolder(Path.GetDirectoryName(assetPath).Replace('\\', '/'));
            File.WriteAllBytes(ProceduralSpriteUtil.ToAbsolutePath(assetPath), png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = filter;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = maxSize;
            importer.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            if (pivot.HasValue)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot.Value;
            }
            else
            {
                settings.spriteAlignment = (int)SpriteAlignment.Center;
            }

            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
