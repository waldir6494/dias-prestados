using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Dibuja Collider2D en Scene View. Solo Editor (no builds).
    /// Menú: Tools > Dias Prestados > Mostrar colliders del escenario
    /// </summary>
    [InitializeOnLoad]
    public static class SceneColliderDebug
    {
        const string PrefKey = "DiasPrestados.SceneColliderDebug";

        static bool showOverlay;

        static SceneColliderDebug()
        {
            showOverlay = EditorPrefs.GetBool(PrefKey, false);
            SceneView.duringSceneGui += DrawSceneOverlay;
        }

        [MenuItem("Tools/Dias Prestados/Mostrar colliders del escenario")]
        static void ToggleMenu()
        {
            showOverlay = !showOverlay;
            EditorPrefs.SetBool(PrefKey, showOverlay);
            SceneView.RepaintAll();
            Debug.Log(showOverlay
                ? "[Días Prestados] Colliders visibles en Scene View."
                : "[Días Prestados] Overlay de colliders oculto.");
        }

        [MenuItem("Tools/Dias Prestados/Mostrar colliders del escenario", true)]
        static bool ToggleMenuValidate()
        {
            Menu.SetChecked("Tools/Dias Prestados/Mostrar colliders del escenario", showOverlay);
            return true;
        }

        static void DrawSceneOverlay(SceneView view)
        {
            if (!showOverlay)
            {
                return;
            }

            foreach (var col in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude))
            {
                if (col == null || !col.enabled)
                {
                    continue;
                }

                Color outline;
                Color face;
                GetColors(col, out face, out outline);

                DrawShape(col, face, outline);
            }

            Handles.BeginGUI();
            GUI.Box(new Rect(10, 10, 340, 78),
                "Colliders Scene View\nVerde: plataformas atravesables\nCyan: suelo sólido | Naranja: obstáculos\nMagenta: walls | Azul: player | Rojo: decoración");
            Handles.EndGUI();
        }

        /// <summary>Dibuja la forma real del collider (polígono, caja rotada) y no solo su AABB.</summary>
        static void DrawShape(Collider2D col, Color face, Color outline)
        {
            if (col is PolygonCollider2D poly)
            {
                for (int p = 0; p < poly.pathCount; p++)
                {
                    Vector2[] path = poly.GetPath(p);
                    var verts = new Vector3[path.Length];
                    for (int i = 0; i < path.Length; i++)
                    {
                        verts[i] = poly.transform.TransformPoint(path[i] + poly.offset);
                    }

                    Handles.color = outline; // solo contorno: el polígono puede ser cóncavo
                    Handles.DrawAAPolyLine(2.5f, Close(verts));
                }

                return;
            }

            if (col is BoxCollider2D box)
            {
                Vector2 h = box.size * 0.5f;
                Vector2 o = box.offset;
                Transform t = box.transform;
                Vector3[] v =
                {
                    t.TransformPoint(new Vector3(o.x - h.x, o.y - h.y, 0f)),
                    t.TransformPoint(new Vector3(o.x - h.x, o.y + h.y, 0f)),
                    t.TransformPoint(new Vector3(o.x + h.x, o.y + h.y, 0f)),
                    t.TransformPoint(new Vector3(o.x + h.x, o.y - h.y, 0f))
                };
                Handles.DrawSolidRectangleWithOutline(v, face, outline);
                return;
            }

            Vector3 center = col.bounds.center;
            Vector3 half = col.bounds.size * 0.5f;
            Vector3[] rect =
            {
                center + new Vector3(-half.x, -half.y, 0f),
                center + new Vector3(-half.x, half.y, 0f),
                center + new Vector3(half.x, half.y, 0f),
                center + new Vector3(half.x, -half.y, 0f)
            };
            Handles.DrawSolidRectangleWithOutline(rect, face, outline);
        }

        static Vector3[] Close(Vector3[] v)
        {
            var r = new Vector3[v.Length + 1];
            v.CopyTo(r, 0);
            r[v.Length] = v[0];
            return r;
        }

        static void GetColors(Collider2D col, out Color face, out Color outline)
        {
            string n = col.gameObject.name;
            string root = col.transform.root.name;

            if (root == "Player" || n == "Player")
            {
                face = new Color(0.25f, 0.5f, 1f, 0.12f);
                outline = new Color(0.4f, 0.7f, 1f, 0.95f);
                return;
            }

            if (n.StartsWith("Platform_") || col.GetComponent<PlatformEffector2D>() != null)
            {
                face = new Color(0.2f, 0.95f, 0.35f, 0.14f);
                outline = new Color(0.15f, 1f, 0.4f, 0.95f);
                return;
            }

            if (n == "Ground" || n == "RaisedShelf" || n.StartsWith("Terrain_"))
            {
                face = new Color(0.1f, 0.95f, 0.9f, 0.12f);
                outline = new Color(0.05f, 1f, 0.95f, 0.95f);
                return;
            }

            if (n.StartsWith("_ForestStyle") || n.StartsWith("FG_") || root == "ForestVisuals")
            {
                face = new Color(1f, 0.1f, 0.1f, 0.2f);
                outline = new Color(1f, 0.15f, 0.15f, 0.95f);
                return;
            }

            if (n.StartsWith("Wall_"))
            {
                face = new Color(1f, 0.2f, 1f, 0.08f);
                outline = new Color(1f, 0.35f, 1f, 0.9f);
                return;
            }

            face = new Color(1f, 0.5f, 0.1f, 0.14f);
            outline = new Color(1f, 0.6f, 0.2f, 0.95f);
        }
    }
}
