using System.Collections.Generic;
using UnityEngine;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Perfil del terreno de la primera prueba (Hito 1). Fuente única para textura, collider y colocación de objetos.
    /// Unidades Unity; y = 0 es el suelo base.
    /// </summary>
    public static class DemoTerrain
    {
        public const float MinX = -8f;
        public const float MaxX = 208f;
        public const float BottomY = -9f;
        public const float TopY = 5f;
        public const int Ppu = 32;

        public const float RavineLeft = 82f;
        public const float RavineRight = 85.4f;
        public const float RavineFloor = -7f;

        public const float StepX = 37f;          // escalón pequeño (el hermano lo salta solo)
        public const float StepHeight = 1.1f;
        public const float CliffX = 118f;        // risco alto (el hermano necesita ayuda)
        public const float CliffHeight = 2.3f;
        public const float CliffTopEndX = 146f;
        public const float LedgeX = 100f;        // plataforma alta del minipuzle de la caja (no se alcanza saltando)
        public const float LedgeHeight = 3.3f;
        public const float LedgeEndX = 112f;

        public static float SmoothStep(float a, float b, float x)
        {
            float t = Mathf.Clamp01((x - a) / (b - a));
            return t * t * (3f - 2f * t);
        }

        /// <summary>Altura de la superficie sin tener en cuenta el barranco.</summary>
        public static float Height(float x)
        {
            float h = 0f;
            h += StepHeight * SmoothStep(StepX - 1.2f, StepX + 1.2f, x);
            h -= StepHeight * SmoothStep(60f, 72f, x);
            h += CliffHeight * SmoothStep(CliffX - 0.15f, CliffX + 0.15f, x);
            h -= CliffHeight * SmoothStep(CliffTopEndX, 160f, x);
            h += LedgeHeight * (SmoothStep(LedgeX - 0.15f, LedgeX + 0.15f, x) - SmoothStep(LedgeEndX - 0.15f, LedgeEndX + 0.15f, x));

            float undulation = 0.13f * Mathf.Sin(x * 0.23f) + 0.08f * Mathf.Sin(x * 0.61f + 1.3f);
            // Se atenúa junto a los elementos clave para que el borde sea legible y fiable.
            float nearFeature = Mathf.Min(
                Mathf.Min(Mathf.Abs(x - RavineLeft), Mathf.Abs(x - RavineRight)),
                Mathf.Min(Mathf.Min(Mathf.Abs(x - CliffX), Mathf.Abs(x - StepX)), Mathf.Min(Mathf.Abs(x - LedgeX), Mathf.Abs(x - LedgeEndX))));
            float weight = Mathf.Clamp01((nearFeature - 1.5f) / 3f);
            // Suelo llano bajo la caja y sobre la plataforma.
            weight *= x < LedgeX - 12f ? 1f : (x < LedgeX - 10f ? 1f - SmoothStep(LedgeX - 12f, LedgeX - 10f, x) : (x <= LedgeEndX ? 0f : 1f));
            return h + undulation * weight;
        }

        public static bool InRavine(float x)
        {
            return x >= RavineLeft && x < RavineRight;
        }

        /// <summary>Altura real de la superficie (incluye el fondo del barranco).</summary>
        public static float Surface(float x)
        {
            return InRavine(x) ? RavineFloor : Height(x);
        }

        /// <summary>Polilínea superior, de izquierda a derecha, con paredes verticales en el barranco.</summary>
        public static List<Vector2> TopPolyline(float step = 0.2f)
        {
            var pts = new List<Vector2>();
            for (float x = MinX; x < RavineLeft; x += step)
            {
                pts.Add(new Vector2(x, Height(x)));
            }

            pts.Add(new Vector2(RavineLeft, Height(RavineLeft)));
            pts.Add(new Vector2(RavineLeft, RavineFloor));
            pts.Add(new Vector2(RavineRight, RavineFloor));
            pts.Add(new Vector2(RavineRight, Height(RavineRight)));

            for (float x = RavineRight + step; x <= MaxX; x += step)
            {
                pts.Add(new Vector2(x, Height(x)));
            }

            pts.Add(new Vector2(MaxX, Height(MaxX)));
            return pts;
        }

        public static Vector2[] CollisionPolygon()
        {
            List<Vector2> top = TopPolyline();
            top.Add(new Vector2(MaxX, BottomY));
            top.Add(new Vector2(MinX, BottomY));
            return top.ToArray();
        }
    }
}
