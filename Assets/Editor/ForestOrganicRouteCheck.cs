using System.Text;
using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Comprobación geométrica en Edit Mode (NO sustituye una prueba real en Play):
    /// pendientes del terreno, saltos requeridos frente al salto máximo y huecos de cabeza.
    /// Menú: Tools > Dias Prestados > Comprobar ruta Forest Organic
    /// </summary>
    public static class ForestOrganicRouteCheck
    {
        // Valores derivados de PlayerController2D (solo lectura, no se modifican).
        const float MaxJump = 2.87f;
        const float PlayerHeight = 1.7f;
        const float PlayerWidth = 0.55f;

        const string FlagPath = "Temp/CheckForestOrganic.flag";
        const string ReportPath = "Temp/organic_check.txt";

        [InitializeOnLoadMethod]
        static void AutoRun()
        {
            EditorApplication.delayCall += () =>
            {
                if (!System.IO.File.Exists(FlagPath) || EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                System.IO.File.Delete(FlagPath);
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ForestOrganicSceneBuilder.ScenePath)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(ForestOrganicSceneBuilder.ScenePath);
                }

                Run();
            };
        }

        [MenuItem("Tools/Dias Prestados/Comprobar ruta Forest Organic")]
        public static void Run()
        {
            var sb = new StringBuilder("[Días Prestados] Comprobación de ruta Forest Organic\n");
            Physics2D.SyncTransforms();

            var terrain = GameObject.Find("Terrain_Main")?.GetComponent<Collider2D>();
            if (terrain == null)
            {
                Debug.LogError("[Días Prestados] No hay Terrain_Main en la escena abierta (abre ForestOrganicDemo).");
                return;
            }

            // 1) Perfil del terreno: pendiente máxima y mayor escalón local.
            float maxSlope = 0f;
            float prevY = float.NaN;
            float maxSlopeX = 0f;
            int blocked = 0;
            for (float x = -19.5f; x <= 19.5f; x += 0.1f)
            {
                float y = TerrainY(terrain, x);
                if (!float.IsNaN(prevY))
                {
                    float slope = Mathf.Abs(y - prevY) / 0.1f;
                    if (slope > maxSlope)
                    {
                        maxSlope = slope;
                        maxSlopeX = x;
                    }
                }

                prevY = y;

                // Huecos de cabeza: la cápsula del jugador de pie sobre el terreno no debe solapar nada sólido
                // (el tronco y la pasarela son de un solo sentido y se excluyen).
                Vector2 center = new Vector2(x, y + 0.03f + PlayerHeight * 0.5f);
                foreach (Collider2D hit in Physics2D.OverlapCapsuleAll(center, new Vector2(PlayerWidth, PlayerHeight - 0.1f), CapsuleDirection2D.Vertical, 0f))
                {
                    if (hit == terrain || hit.GetComponent<PlatformEffector2D>() != null || hit.transform.root.name == "Player")
                    {
                        continue;
                    }

                    blocked++;
                    sb.AppendLine($"  Bloqueo en x={x:F1}: {hit.name}");
                    break;
                }
            }

            sb.AppendLine($"Pendiente máxima: {maxSlope:F2} ({Mathf.Atan(maxSlope) * Mathf.Rad2Deg:F0}°) en x={maxSlopeX:F1}");
            sb.AppendLine($"Posiciones de pie con cápsula bloqueada por sólido: {blocked} (esperado: solo junto a peñasco/pilares, que son muros)");

            // 2) Saltos requeridos.
            Report(sb, "Peñasco", "Boulder_Solid", terrain, 3.7f);
            var deck = GameObject.Find("RailDeck_OneWay")?.GetComponent<Collider2D>();
            if (deck != null)
            {
                float top = deck.bounds.max.y;
                float under = deck.bounds.min.y;
                float ground = TerrainY(terrain, 7.0f);
                sb.AppendLine($"Pasarela: cara superior y={top:F2}, desde suelo ({ground:F2}) requiere {top - ground:F2} (máx {MaxJump:F2}); hueco bajo la pasarela {under - ground:F2} (jugador {PlayerHeight:F2})");
            }

            var log = GameObject.Find("Log_Fallen_OneWay")?.GetComponent<Collider2D>();
            if (log != null)
            {
                float ly = log.bounds.max.y;
                float gully = TerrainY(terrain, -3.5f);
                sb.AppendLine($"Tronco: y máx {log.bounds.max.y:F2}; fondo del barranco {gully:F2}: salto desde el fondo {ly - gully:F2} (máx {MaxJump:F2})");
                float leftEnd = TerrainY(terrain, -7.4f);
                float rightEnd = TerrainY(terrain, 0.9f);
                var lt = ForestOrganicLayout.LogLeftTop;
                var rt = ForestOrganicLayout.LogRightTop;
                sb.AppendLine($"Extremos tronco vs terreno: izq {lt.y - leftEnd:+0.000;-0.000}, der {rt.y - rightEnd:+0.000;-0.000} (0 = enrasado)");
            }

            Debug.Log(sb.ToString());
            System.IO.File.WriteAllText(ReportPath, sb.ToString());
        }

        static void Report(StringBuilder sb, string label, string objName, Collider2D terrain, float x)
        {
            var go = GameObject.Find(objName);
            var col = go != null ? go.GetComponent<Collider2D>() : null;
            if (col == null)
            {
                sb.AppendLine($"{label}: no encontrado");
                return;
            }

            float ground = TerrainY(terrain, x - 1.6f);
            float rise = col.bounds.max.y - ground;
            sb.AppendLine($"{label}: altura sobre el suelo {rise:F2} (salto máx {MaxJump:F2}); anchura de cima aprox {col.bounds.size.x:F2}");
        }

        /// <summary>Altura de la superficie del terreno en x (raycast contra el collider del terreno).</summary>
        static float TerrainY(Collider2D terrain, float x)
        {
            var origin = new Vector2(x, 10f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 20f);
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider == terrain)
                {
                    return h.point.y;
                }
            }

            return float.NaN;
        }
    }
}
