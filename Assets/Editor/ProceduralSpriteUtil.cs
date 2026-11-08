using System.IO;
using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Utilidades compartidas para crear sprites PNG persistentes en Assets.
    /// </summary>
    public static class ProceduralSpriteUtil
    {
        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        public static void WriteSprite(string assetPath, Texture2D texture, float pixelsPerUnit, FilterMode filter = FilterMode.Bilinear)
        {
            string absolute = ToAbsolutePath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? absolute);
            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            File.WriteAllBytes(absolute, png);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = filter;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        public static Sprite LoadSprite(string assetPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets != null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is Sprite sprite)
                    {
                        return sprite;
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        public static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectRoot ?? Application.dataPath, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        public static Texture2D CreateTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Clear(tex, Color.clear);
            return tex;
        }

        public static void Clear(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
        }

        public static void Set(Texture2D tex, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
            {
                return;
            }

            tex.SetPixel(x, y, color);
        }

        public static void FillRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    Set(tex, px, py, color);
                }
            }
        }

        public static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            for (int py = cy - radius; py <= cy + radius; py++)
            {
                for (int px = cx - radius; px <= cx + radius; px++)
                {
                    int dx = px - cx;
                    int dy = py - cy;
                    if (dx * dx + dy * dy <= r2)
                    {
                        Set(tex, px, py, color);
                    }
                }
            }
        }

        public static void FillEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color color)
        {
            for (int py = cy - ry; py <= cy + ry; py++)
            {
                for (int px = cx - rx; px <= cx + rx; px++)
                {
                    float nx = (px - cx) / (float)Mathf.Max(1, rx);
                    float ny = (py - cy) / (float)Mathf.Max(1, ry);
                    if (nx * nx + ny * ny <= 1f)
                    {
                        Set(tex, px, py, color);
                    }
                }
            }
        }

        public static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            int minX = Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
            int maxX = Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
            int minY = Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
            int maxY = Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y)));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, c))
                    {
                        Set(tex, x, y, color);
                    }
                }
            }
        }

        static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public static void VerticalGradient(Texture2D tex, Color bottom, Color top)
        {
            for (int y = 0; y < tex.height; y++)
            {
                float t = y / (float)(tex.height - 1);
                Color c = Color.Lerp(bottom, top, t);
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, c);
                }
            }
        }
    }
}
