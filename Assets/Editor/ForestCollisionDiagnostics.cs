using System.Collections.Generic;
using System.Text;
using DiasPrestados;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Revisa ForestDemo: plataformas unidireccionales, obstáculos sólidos, capas y luz global.
    /// Menú: Tools > Dias Prestados > Diagnosticar colisiones Forest Demo
    /// </summary>
    public static class ForestCollisionDiagnostics
    {
        const float RectWidthTolerance = 0.08f;
        const float RockWidthTolerance = 0.12f;
        const float SampleFractions = 5f;

        [MenuItem("Tools/Dias Prestados/Diagnosticar colisiones de la escena")]
        public static void DiagnoseFromMenu()
        {
            int issues = LogActiveSceneAndProbe();
            if (issues == 0)
            {
                EditorUtility.DisplayDialog(
                    "Días Prestados",
                    "Diagnóstico sin problemas.\nEl detalle está en la consola.",
                    "Aceptar");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Días Prestados",
                    issues + " problema(s). Revisa la consola.",
                    "Aceptar");
            }
        }

        public static int LogActiveSceneAndProbe()
        {
            var report = new StringBuilder();
            Scene scene = EditorSceneManager.GetActiveScene();
            report.AppendLine("[Días Prestados] Diagnóstico de '" + scene.name + "'");
            int issues = Analyze(scene, report);
            issues += RunPhysicsProbe(report);
            if (issues == 0)
            {
                report.AppendLine("Resultado: sin problemas de colisión ni de la prueba física.");
                Debug.Log(report.ToString());
            }
            else
            {
                report.AppendLine("Resultado: " + issues + " problema(s).");
                Debug.LogWarning(report.ToString());
            }

            return issues;
        }

        public static int Analyze(Scene scene, StringBuilder report)
        {
            int issues = 0;
            var gameplay = new List<Collider2D>();
            int playerMask = ~0;
            float jumpForce = 13f;
            float moveSpeed = 7f;
            float airControl = 0.7f;
            float fallGravity = 1.35f;
            float gravityScale = 3f;
            bool foundPlayer = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var col in root.GetComponentsInChildren<Collider2D>(true))
                {
                    if (col == null || col.GetComponent<PlayerController2D>() != null || col.transform.root.name == "Player")
                    {
                        continue;
                    }

                    gameplay.Add(col);
                }

                var player = root.GetComponentInChildren<PlayerController2D>(true);
                if (player == null)
                {
                    continue;
                }

                foundPlayer = true;
                var so = new SerializedObject(player);
                playerMask = so.FindProperty("groundLayers").intValue;
                jumpForce = so.FindProperty("jumpForce").floatValue;
                moveSpeed = so.FindProperty("moveSpeed").floatValue;
                airControl = so.FindProperty("airControlMultiplier").floatValue;
                fallGravity = so.FindProperty("fallGravityMultiplier").floatValue;
                var body = player.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    gravityScale = body.gravityScale;
                }
            }

            if (!foundPlayer)
            {
                issues++;
                report.AppendLine("- No hay PlayerController2D en la escena.");
            }

            foreach (var col in gameplay)
            {
                issues += InspectCollider(col, playerMask, report);
            }

            issues += InspectOverlaps(gameplay, report);
            issues += InspectReachability(gameplay, jumpForce, moveSpeed, airControl, fallGravity, gravityScale, report);
            issues += InspectLighting(scene, report);
            return issues;
        }

        static int InspectCollider(Collider2D col, int playerMask, StringBuilder report)
        {
            int issues = 0;
            string n = col.gameObject.name;
            string root = col.transform.root != null ? col.transform.root.name : string.Empty;
            bool decor = n.StartsWith("_ForestStyle") || n.StartsWith("FG_") || root == "ForestVisuals";
            if (decor)
            {
                issues++;
                report.AppendLine($"- Decoración '{PathOf(col)}' tiene {col.GetType().Name}.");
                return issues;
            }

            int layerBit = 1 << col.gameObject.layer;
            if ((playerMask & layerBit) == 0 || col.gameObject.layer == 2)
            {
                issues++;
                report.AppendLine($"- '{n}' está en la capa física {col.gameObject.layer}, fuera de groundLayers del jugador.");
            }

            bool oneWayName = n.StartsWith("Platform_");
            bool solidName = n == "Ground" || n == "RaisedShelf" || n.StartsWith("Obstacle_")
                             || n.StartsWith("Rock_") || n.StartsWith("Wall_");
            var effector = col.GetComponent<PlatformEffector2D>();

            if (oneWayName)
            {
                if (col.GetComponents<Collider2D>().Length != 1)
                {
                    issues++;
                    report.AppendLine($"- '{n}' tiene más de un Collider2D.");
                }

                if (!(col is BoxCollider2D) || col.isTrigger || !col.usedByEffector || effector == null || !effector.useOneWay)
                {
                    issues++;
                    report.AppendLine($"- '{n}' no es una plataforma unidireccional (falta BoxCollider usedByEffector + PlatformEffector2D.useOneWay).");
                }
                else if (effector.surfaceArc < 140f || effector.surfaceArc > 200f || Mathf.Abs(effector.rotationalOffset) > 1f)
                {
                    issues++;
                    report.AppendLine($"- '{n}' tiene surfaceArc={effector.surfaceArc} u offset={effector.rotationalOffset}; la superficie no es uniforme.");
                }

                issues += SamplePlatformUnderside(col, report);
            }
            else if (solidName)
            {
                if (effector != null || col.usedByEffector)
                {
                    issues++;
                    report.AppendLine($"- Obstáculo sólido '{n}' tiene colisión unidireccional.");
                }
            }

            var sr = col.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null && sr.color.a > 0.05f && (oneWayName || solidName) && !n.StartsWith("Wall_"))
            {
                float visual = sr.bounds.size.x;
                float physical = col.bounds.size.x;
                float delta = Mathf.Abs(visual - physical) / Mathf.Max(0.001f, visual);
                float limit = n.StartsWith("Rock_") ? RockWidthTolerance : RectWidthTolerance;
                if (delta > limit)
                {
                    issues++;
                    report.AppendLine($"- '{n}' ancho visual {visual:0.00} vs collider {physical:0.00} (diferencia {delta:P0}).");
                }
            }

            return issues;
        }

        static int SamplePlatformUnderside(Collider2D col, StringBuilder report)
        {
            int issues = 0;
            Bounds b = col.bounds;
            int blocked = 0;
            for (int i = 0; i < 5; i++)
            {
                float t = (i + 0.5f) / SampleFractions;
                float x = Mathf.Lerp(b.min.x, b.max.x, t);
                Vector2 below = new Vector2(x, b.min.y - 0.06f);
                Collider2D hit = Physics2D.OverlapPoint(below);
                if (hit != null && hit != col && !hit.isTrigger && hit.transform.root.name != "Player")
                {
                    blocked++;
                }

                Vector2 onTop = new Vector2(x, b.max.y - 0.02f);
                if (!col.OverlapPoint(onTop))
                {
                    issues++;
                    report.AppendLine($"- '{col.name}' no cubre su propia tapa en x={x:0.00}.");
                }
            }

            if (blocked > 0 && blocked < 5)
            {
                issues++;
                report.AppendLine($"- '{col.name}' tiene el paso desde abajo tapado solo en {blocked}/5 tramos.");
            }
            else if (blocked == 5)
            {
                issues++;
                report.AppendLine($"- '{col.name}' tiene otro collider sólido en todo el lado inferior.");
            }

            return issues;
        }

        static int InspectOverlaps(List<Collider2D> cols, StringBuilder report)
        {
            int issues = 0;
            for (int i = 0; i < cols.Count; i++)
            {
                for (int j = i + 1; j < cols.Count; j++)
                {
                    Collider2D a = cols[i];
                    Collider2D b = cols[j];
                    if (a == null || b == null || !a.bounds.Intersects(b.bounds))
                    {
                        continue;
                    }

                    float overlapW = Mathf.Min(a.bounds.max.x, b.bounds.max.x) - Mathf.Max(a.bounds.min.x, b.bounds.min.x);
                    float overlapH = Mathf.Min(a.bounds.max.y, b.bounds.max.y) - Mathf.Max(a.bounds.min.y, b.bounds.min.y);
                    if (overlapW < 0.08f || overlapH < 0.08f)
                    {
                        continue;
                    }

                    bool oneA = a.GetComponent<PlatformEffector2D>() != null && a.usedByEffector;
                    bool oneB = b.GetComponent<PlatformEffector2D>() != null && b.usedByEffector;
                    if (oneA == oneB)
                    {
                        continue;
                    }

                    issues++;
                    report.AppendLine($"- '{a.name}' y '{b.name}' se solapan y mezclan plataforma atravesable con sólido.");
                }
            }

            return issues;
        }

        static int InspectReachability(
            List<Collider2D> cols,
            float jumpForce,
            float moveSpeed,
            float airControl,
            float fallGravity,
            float gravityScale,
            StringBuilder report)
        {
            var surfaces = new List<Collider2D>();
            foreach (var col in cols)
            {
                string n = col.gameObject.name;
                if (n == "Ground" || n == "RaisedShelf" || n.StartsWith("Platform_"))
                {
                    surfaces.Add(col);
                }
            }

            float g = Mathf.Abs(Physics2D.gravity.y) * Mathf.Max(0.01f, gravityScale);
            float jumpHeight = (jumpForce * jumpForce) / (2f * g);
            float ascent = jumpForce / g;
            float airTime = ascent + (ascent / Mathf.Max(1f, fallGravity));
            float horiz = moveSpeed * airControl * airTime * 0.9f;
            report.AppendLine($"Salto estimado: altura {jumpHeight:0.00} u, alcance horizontal {horiz:0.00} u (sin cambiar al jugador).");

            int issues = 0;
            foreach (var target in surfaces)
            {
                if (!target.name.StartsWith("Platform_"))
                {
                    continue;
                }

                bool reachable = false;
                foreach (var source in surfaces)
                {
                    if (source == target)
                    {
                        continue;
                    }

                    float dy = target.bounds.max.y - source.bounds.max.y;
                    if (dy < -0.05f)
                    {
                        float dropGap = EdgeGap(source.bounds, target.bounds);
                        if (dropGap <= moveSpeed * 1.1f)
                        {
                            reachable = true;
                            break;
                        }

                        continue;
                    }

                    if (dy > jumpHeight * 0.98f)
                    {
                        continue;
                    }

                    float gap = EdgeGap(source.bounds, target.bounds);
                    if (gap <= horiz)
                    {
                        reachable = true;
                        break;
                    }
                }

                if (!reachable)
                {
                    issues++;
                    report.AppendLine($"- '{target.name}' no parece alcanzable con el salto actual (tapa y={target.bounds.max.y:0.00}).");
                }
            }

            return issues;
        }

        static float EdgeGap(Bounds a, Bounds b)
        {
            if (a.max.x < b.min.x)
            {
                return b.min.x - a.max.x;
            }

            if (b.max.x < a.min.x)
            {
                return a.min.x - b.max.x;
            }

            return 0f;
        }

        static int InspectLighting(Scene scene, StringBuilder report)
        {
            int issues = 0;
            Light2D global = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var light in root.GetComponentsInChildren<Light2D>(true))
                {
                    if (light.lightType == Light2D.LightType.Global)
                    {
                        global = light;
                    }
                }

                var dust = FindNamed(root.transform, "AmbientDust");
                if (dust != null)
                {
                    var renderer = dust.GetComponent<ParticleSystemRenderer>();
                    if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.shader == null
                        || renderer.sharedMaterial.shader.name.Contains("InternalErrorShader"))
                    {
                        issues++;
                        report.AppendLine("- AmbientDust no tiene un material URP válido (riesgo de motas magenta).");
                    }
                }
            }

            if (global == null)
            {
                issues++;
                report.AppendLine("- No hay Global Light 2D.");
                return issues;
            }

            if (global.intensity < 0.95f)
            {
                issues++;
                report.AppendLine($"- Global Light 2D demasiado tenue (intensidad {global.intensity:0.00}).");
            }

            string[] needed = { "Background", "Midground", "Gameplay", "Foreground" };
            for (int i = 0; i < needed.Length; i++)
            {
                int id = SortingLayer.NameToID(needed[i]);
                bool found = false;
                int[] targets = global.targetSortingLayers;
                if (targets != null)
                {
                    for (int t = 0; t < targets.Length; t++)
                    {
                        if (targets[t] == id && id != 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    issues++;
                    report.AppendLine($"- Global Light 2D no ilumina la Sorting Layer '{needed[i]}'.");
                }
            }

            int casters = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                casters += root.GetComponentsInChildren<ShadowCaster2D>(true).Length;
            }

            report.AppendLine(casters == 0
                ? "ShadowCaster2D: ninguno (las sombras 2D no tapan el recorrido)."
                : "ShadowCaster2D: " + casters + ".");
            return issues;
        }

        static Transform FindNamed(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindNamed(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        static string PathOf(Collider2D col)
        {
            return col.transform.parent != null ? col.transform.parent.name + "/" + col.name : col.name;
        }

        public static int RunPhysicsProbe(StringBuilder report)
        {
            // En Edit Mode, SceneManager.CreateScene con física local no está permitido.
            // La escena aditiva comparte el mundo físico; se suspenden los demás cuerpos
            // y el banco de prueba queda lejos del nivel.
            Scene previous = SceneManager.GetActiveScene();
            SimulationMode2D previousMode = Physics2D.simulationMode;
            var suspended = new List<Rigidbody2D>();
            var wasSimulated = new List<bool>();
            Scene probeScene = default;
            int issues = 0;
            try
            {
                probeScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                SceneManager.SetActiveScene(probeScene);
                foreach (var rb in UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include))
                {
                    if (rb == null || rb.gameObject.scene == probeScene)
                    {
                        continue;
                    }

                    suspended.Add(rb);
                    wasSimulated.Add(rb.simulated);
                    rb.simulated = false;
                }

                Physics2D.simulationMode = SimulationMode2D.Script;
                PhysicsScene2D physics = probeScene.GetPhysicsScene2D();
                const float baseY = 400f;
                float[] xs = { -2f, -1f, 0f, 1f, 2f };
                issues += ProbePass(physics, probeScene, xs, baseY, oneWay: true, expectPass: true, report, "unidireccional");
                issues += ProbeLand(physics, probeScene, xs, baseY, report);
                issues += ProbePass(physics, probeScene, xs, baseY, oneWay: false, expectPass: false, report, "sólida");
            }
            catch (System.Exception ex)
            {
                issues++;
                report.AppendLine("- La prueba física aislada no pudo ejecutarse: " + ex.Message);
            }
            finally
            {
                Physics2D.simulationMode = previousMode;
                for (int i = 0; i < suspended.Count; i++)
                {
                    if (suspended[i] != null)
                    {
                        suspended[i].simulated = wasSimulated[i];
                    }
                }

                if (probeScene.IsValid() && probeScene.isLoaded)
                {
                    EditorSceneManager.CloseScene(probeScene, true);
                }

                if (previous.IsValid() && previous.isLoaded)
                {
                    SceneManager.SetActiveScene(previous);
                }
            }

            return issues;
        }

        static int ProbeLand(PhysicsScene2D physics, Scene scene, float[] xs, float baseY, StringBuilder report)
        {
            int issues = 0;
            float top = baseY + 0.175f;
            GameObject platform = CreateProbePlatform(scene, oneWay: true, baseY);
            try
            {
                for (int i = 0; i < xs.Length; i++)
                {
                    GameObject body = CreateProbeBody(scene, new Vector2(xs[i], baseY + 2.8f), Vector2.zero);
                    var rb = body.GetComponent<Rigidbody2D>();
                    float maxY = rb.position.y;
                    Simulate(physics, rb, 2.2f, ref maxY);
                    float feet = rb.position.y - 0.85f;
                    if (feet < baseY || Mathf.Abs(feet - top) > 0.28f)
                    {
                        issues++;
                        report.AppendLine($"- Aterrizaje en x={xs[i]:0.0}: pies en y={feet:0.00} (se esperaba ~{top:0.00}).");
                    }

                    UnityEngine.Object.DestroyImmediate(body);
                }
            }
            finally
            {
                if (platform != null)
                {
                    UnityEngine.Object.DestroyImmediate(platform);
                }
            }

            if (issues == 0)
            {
                report.AppendLine("Prueba física: aterriza en 5 puntos de la misma plataforma.");
            }

            return issues;
        }

        static int ProbePass(
            PhysicsScene2D physics,
            Scene scene,
            float[] xs,
            float baseY,
            bool oneWay,
            bool expectPass,
            StringBuilder report,
            string label)
        {
            int issues = 0;
            GameObject platform = CreateProbePlatform(scene, oneWay, baseY);
            try
            {
                for (int i = 0; i < xs.Length; i++)
                {
                    GameObject body = CreateProbeBody(scene, new Vector2(xs[i], baseY - 2.2f), new Vector2(0f, 18f));
                    var rb = body.GetComponent<Rigidbody2D>();
                    float maxY = rb.position.y;
                    Simulate(physics, rb, 1.6f, ref maxY);
                    bool passed = maxY > baseY + 0.6f;
                    if (passed != expectPass)
                    {
                        issues++;
                        report.AppendLine($"- Paso desde abajo ({label}) en x={xs[i]:0.0}: maxY={maxY:0.00}, se esperaba {(expectPass ? "cruzar" : "bloquear")}.");
                    }

                    UnityEngine.Object.DestroyImmediate(body);
                }
            }
            finally
            {
                if (platform != null)
                {
                    UnityEngine.Object.DestroyImmediate(platform);
                }
            }

            if (issues == 0)
            {
                report.AppendLine(expectPass
                    ? "Prueba física: se atraviesa desde abajo en 5 puntos."
                    : "Prueba física: el obstáculo sólido bloquea desde abajo en 5 puntos.");
            }

            return issues;
        }

        static GameObject CreateProbePlatform(Scene scene, bool oneWay, float baseY)
        {
            var go = new GameObject(oneWay ? "ProbePlatform" : "ProbeSolid");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = new Vector3(0f, baseY, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(6f, 0.35f);
            box.offset = Vector2.zero;
            if (!oneWay)
            {
                return go;
            }

            box.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = ForestDemoSceneBuilder.OneWayGrouping;
            effector.surfaceArc = ForestDemoSceneBuilder.OneWaySurfaceArc;
            effector.sideArc = ForestDemoSceneBuilder.OneWaySideArc;
            effector.rotationalOffset = 0f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            effector.useColliderMask = false;
            return go;
        }

        static GameObject CreateProbeBody(Scene scene, Vector2 position, Vector2 velocity)
        {
            var go = new GameObject("ProbeBody");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.None;
            rb.position = position;
            rb.linearVelocity = velocity;
            var capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.55f, 1.7f);
            capsule.direction = CapsuleDirection2D.Vertical;
            return go;
        }

        static void Simulate(PhysicsScene2D physics, Rigidbody2D tracked, float seconds, ref float maxY)
        {
            int steps = Mathf.CeilToInt(seconds / 0.02f);
            for (int i = 0; i < steps; i++)
            {
                physics.Simulate(0.02f);
                if (tracked != null && tracked.position.y > maxY)
                {
                    maxY = tracked.position.y;
                }
            }
        }
    }
}
