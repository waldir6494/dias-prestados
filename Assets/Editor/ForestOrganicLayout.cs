using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Fuente única de verdad de la geometría del tramo orgánico (ForestOrganicDemo).
    /// El arte (ForestOrganicArtGenerator) y los colliders (ForestOrganicSceneBuilder) salen de los mismos
    /// puntos, así que la silueta visible y la física coinciden por construcción.
    /// Unidades en metros de Unity. Suelo base y = 0. Salto del jugador: ~2.8 u de altura máxima.
    /// </summary>
    public static class ForestOrganicLayout
    {
        // ---------------- Terreno ----------------
        public const float Ppu = 32f;
        public const float TerrainMinX = -20f;
        public const float TerrainMaxX = 20f;
        public const float TerrainBottom = -2.3f;
        const float TerrainTopLimit = 1.6f;
        const float SampleStep = 0.25f;

        public static int TerrainTexWidth => Mathf.RoundToInt((TerrainMaxX - TerrainMinX) * Ppu);
        public static int TerrainTexHeight => Mathf.CeilToInt((TerrainTopLimit - TerrainBottom) * Ppu);
        public static float TerrainTexTop => TerrainBottom + TerrainTexHeight / Ppu;
        public static Vector2 TerrainCenter => new Vector2(0f, (TerrainBottom + TerrainTexTop) * 0.5f);

        /// <summary>Perfil de la superficie (x, y). Recorrido: llano → terraplén → meseta → barranco → ribera → peñasco → estación.</summary>
        static readonly Vector2[] Keys =
        {
            new Vector2(-20f, 0f), new Vector2(-14f, 0f),
            // Terraplén con raíces (desnivel natural, pendiente máx. ~27°).
            new Vector2(-12.8f, 0.10f), new Vector2(-11.6f, 0.45f), new Vector2(-10.6f, 0.95f),
            new Vector2(-9.6f, 1.25f), new Vector2(-8.2f, 1.35f), new Vector2(-7.0f, 1.30f),
            // Barranco poco profundo: pared izquierda de piedra, fondo plano, ribera derecha suave.
            new Vector2(-6.4f, 1.05f), new Vector2(-5.7f, 0.40f), new Vector2(-5.0f, -0.30f),
            new Vector2(-4.4f, -0.65f), new Vector2(-2.8f, -0.70f), new Vector2(-1.9f, -0.50f),
            new Vector2(-1.0f, -0.05f), new Vector2(-0.2f, 0.50f), new Vector2(0.6f, 0.85f),
            new Vector2(1.4f, 0.85f), new Vector2(2.2f, 0.45f), new Vector2(3.0f, 0.05f),
            // Llano bajo la pasarela.
            new Vector2(3.6f, 0f), new Vector2(11.0f, 0f),
            // Talud hacia la meseta final.
            new Vector2(12.2f, 0.35f), new Vector2(13.4f, 0.85f), new Vector2(14.6f, 1.0f),
            new Vector2(20f, 1.0f)
        };

        static Vector2[] cachedTop;

        /// <summary>Puntos de la superficie muestreados cada 0.25 u (la polilínea que ven arte y collider).</summary>
        public static Vector2[] TerrainTop()
        {
            if (cachedTop != null)
            {
                return cachedTop;
            }

            int n = Mathf.RoundToInt((TerrainMaxX - TerrainMinX) / SampleStep) + 1;
            var pts = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                float x = TerrainMinX + i * SampleStep;
                float y = HermiteY(x) + Jitter(x);
                pts[i] = new Vector2(x, y);
            }

            cachedTop = pts;
            return pts;
        }

        /// <summary>Altura de la superficie en x (interpolación lineal de TerrainTop).</summary>
        public static float SurfaceY(float x)
        {
            Vector2[] top = TerrainTop();
            x = Mathf.Clamp(x, TerrainMinX, TerrainMaxX);
            float f = (x - TerrainMinX) / SampleStep;
            int i = Mathf.Clamp(Mathf.FloorToInt(f), 0, top.Length - 2);
            return Mathf.Lerp(top[i].y, top[i + 1].y, f - i);
        }

        /// <summary>Polígono cerrado del terreno en coordenadas locales de Terrain_Main.</summary>
        public static Vector2[] TerrainColliderPath()
        {
            Vector2[] top = TerrainTop();
            Vector2 c = TerrainCenter;
            var path = new Vector2[top.Length + 2];
            for (int i = 0; i < top.Length; i++)
            {
                path[i] = top[i] - c;
            }

            path[top.Length] = new Vector2(TerrainMaxX, TerrainBottom) - c;
            path[top.Length + 1] = new Vector2(TerrainMinX, TerrainBottom) - c;
            return path;
        }

        static float HermiteY(float x)
        {
            int n = Keys.Length;
            int i = 0;
            while (i < n - 2 && x > Keys[i + 1].x)
            {
                i++;
            }

            Vector2 a = Keys[i];
            Vector2 b = Keys[i + 1];
            float h = b.x - a.x;
            float t = Mathf.Clamp01((x - a.x) / h);
            float ma = Tangent(i) * h;
            float mb = Tangent(i + 1) * h;
            float t2 = t * t;
            float t3 = t2 * t;
            return (2f * t3 - 3f * t2 + 1f) * a.y + (t3 - 2f * t2 + t) * ma +
                   (-2f * t3 + 3f * t2) * b.y + (t3 - t2) * mb;
        }

        /// <summary>Tangente monótona (sin sobreoscilación) en la clave k.</summary>
        static float Tangent(int k)
        {
            int n = Keys.Length;
            if (k <= 0 || k >= n - 1)
            {
                return 0f;
            }

            float dl = (Keys[k].y - Keys[k - 1].y) / (Keys[k].x - Keys[k - 1].x);
            float dr = (Keys[k + 1].y - Keys[k].y) / (Keys[k + 1].x - Keys[k].x);
            if (dl * dr <= 0f)
            {
                return 0f;
            }

            float m = (dl + dr) * 0.5f;
            float limit = 3f * Mathf.Min(Mathf.Abs(dl), Mathf.Abs(dr));
            return Mathf.Sign(m) * Mathf.Min(Mathf.Abs(m), limit);
        }

        /// <summary>Irregularidad mínima (±1.5 cm) salvo donde la superficie debe ser exacta (extremos del tronco, llano).</summary>
        static float Jitter(float x)
        {
            bool flatZone = (x >= -8.4f && x <= -6.4f) || (x >= 0.0f && x <= 1.8f) || (x >= 3.4f && x <= 11.2f);
            if (flatZone)
            {
                return 0f;
            }

            float n = Mathf.PerlinNoise(x * 0.55f + 11.3f, 3.7f) - 0.5f;
            return n * 0.03f;
        }

        // ---------------- Tronco caído (atravesable por arriba) ----------------
        public const float LogLeftX = -7.4f;
        public const float LogRightX = 0.9f;
        public const float LogSlabThickness = 0.34f;
        public const float LogBodyThickness = 0.78f;

        public static Vector2 LogLeftTop => new Vector2(LogLeftX, SurfaceY(LogLeftX));
        public static Vector2 LogRightTop => new Vector2(LogRightX, SurfaceY(LogRightX));

        /// <summary>Longitud en píxeles del sprite del tronco (a lo largo de la pendiente).</summary>
        public static int LogTexLength => Mathf.RoundToInt((LogRightTop - LogLeftTop).magnitude * Ppu);
        public static int LogTexThickness => Mathf.RoundToInt(LogBodyThickness * Ppu);
        public static float LogLength => LogTexLength / Ppu;
        public static float LogAngleDeg
        {
            get
            {
                Vector2 d = LogRightTop - LogLeftTop;
                return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            }
        }

        // ---------------- Peñasco sólido ----------------
        public const float BoulderCenterX = 3.7f;
        public const float BoulderPpu = 48f;

        /// <summary>Contorno del peñasco (local al objeto, base en y=0 con 0.1 u empotradas). Cima plana para poder subirse.</summary>
        public static readonly Vector2[] BoulderPoints =
        {
            new Vector2(-1.15f, -0.10f), new Vector2(-1.15f, 0.30f), new Vector2(-1.00f, 0.85f),
            new Vector2(-0.75f, 1.25f), new Vector2(-0.45f, 1.48f), new Vector2(0.45f, 1.50f),
            new Vector2(0.75f, 1.30f), new Vector2(1.00f, 0.90f), new Vector2(1.15f, 0.40f),
            new Vector2(1.15f, -0.10f)
        };

        // ---------------- Pasarela ferroviaria ----------------
        public const float DeckPpu = 48f;
        public const float DeckLeftX = 5.0f;
        public const float DeckTopY = 2.35f;
        public const int DeckTexWidth = 298;
        public const int DeckTexHeight = 22;
        public static float DeckLength => DeckTexWidth / DeckPpu;
        public static float DeckThickness => DeckTexHeight / DeckPpu;
        public static float DeckCenterX => DeckLeftX + DeckLength * 0.5f;
        public static float DeckCenterY => DeckTopY - DeckThickness * 0.5f;
        public static float DeckUndersideY => DeckTopY - DeckThickness;

        public static readonly float[] PillarXs = { 6.0f, 8.4f, 10.8f };
        public const int PillarTexWidth = 24;
        public const int PillarTexHeight = 96;
        public const float PillarColliderWidth = 0.4f;
        public static float PillarHeight => PillarTexHeight / DeckPpu;
        public static float PillarCenterY => DeckUndersideY - PillarHeight * 0.5f;

        // ---------------- Vía caída en el suelo ----------------
        public const float TrackLeftX = 4.9f;
        public const int TrackTexWidth = 200;
        public const int TrackTexHeight = 8;
    }
}
