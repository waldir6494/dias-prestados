using System.IO;
using DiasPrestados;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using P = DiasPrestados.Editor.ForestOrganicArtGenerator.Paths;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// "Rediseño orgánico": crea ForestOrganicDemo.unity a partir de ForestAtmosphereDemo.unity.
    /// Conserva luz, paleta, niebla, parallax y atmósfera; sustituye las plataformas/obstáculos arcade
    /// por terreno con relieve, tronco caído, peñasco y pasarela ferroviaria sobre pilares.
    /// Las escenas previas, PlayerController2D y CameraFollow2D NO se modifican.
    /// Menú: Tools > Dias Prestados > Crear / actualizar Forest Organic Demo
    /// </summary>
    public static class ForestOrganicSceneBuilder
    {
        public const string SourcePath = "Assets/Scenes/ForestAtmosphereDemo.unity";
        public const string ScenePath = "Assets/Scenes/ForestOrganicDemo.unity";

        public const string GameplayRootName = "OrganicGameplay";
        const string LayerMidground = "Midground";
        const string LayerGameplay = "Gameplay";
        const string StyleChildName = "_ForestStyle";
        const string SpriteUnlitPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        static readonly string[] ObsoleteNames =
        {
            "Ground", "RaisedShelf", "Platform_01", "Platform_02", "Platform_03", "Platform_04",
            "Obstacle_Crate", "Obstacle_Crate_2", "Rock_A", "Rock_B", GameplayRootName
        };

        [MenuItem("Tools/Dias Prestados/Crear / actualizar Forest Organic Demo")]
        public static void CreateOrUpdateMenu()
        {
            CreateOrUpdate(interactive: true);
        }

        public static void CreateOrUpdate(bool interactive)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new System.InvalidOperationException("Sal de Play Mode antes de generar ForestOrganicDemo.");
            }

            if (!File.Exists(SourcePath))
            {
                throw new FileNotFoundException("Falta ForestAtmosphereDemo.unity", SourcePath);
            }

            bool showUi = interactive && !Application.isBatchMode;
            try
            {
                ForestOrganicArtGenerator.EnsureAllSprites();

                if (!File.Exists(ScenePath))
                {
                    if (!AssetDatabase.CopyAsset(SourcePath, ScenePath))
                    {
                        throw new IOException("No se pudo copiar ForestAtmosphereDemo.unity.");
                    }

                    AssetDatabase.Refresh();
                }

                if (!PrepareToOpenScene(showUi))
                {
                    return;
                }

                Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Apply(scene);

                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                {
                    throw new IOException("No se pudo guardar ForestOrganicDemo.unity.");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[Días Prestados] ForestOrganicDemo listo en " + ScenePath);

                if (showUi)
                {
                    EditorUtility.DisplayDialog("Días Prestados",
                        "ForestOrganicDemo creada/actualizada.\n\nAbre: " + ScenePath +
                        "\nLas escenas anteriores no se han modificado.", "Aceptar");
                }
            }
            finally
            {
                if (showUi)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        static bool PrepareToOpenScene(bool interactive)
        {
            Scene active = EditorSceneManager.GetActiveScene();
            if (!active.isDirty)
            {
                return true;
            }

            string activePath = (active.path ?? string.Empty).Replace('\\', '/');
            if (string.Equals(activePath, ScenePath, System.StringComparison.OrdinalIgnoreCase))
            {
                EditorSceneManager.SaveScene(active);
                return true;
            }

            if (interactive)
            {
                return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            Debug.LogError("[Días Prestados] La escena abierta tiene cambios sin guardar; guárdala y repite.");
            return false;
        }

        // ------------------------------------------------------------------

        static void Apply(Scene scene)
        {
            if (FindByName("Player") == null || FindByName("Main Camera") == null)
            {
                throw new System.InvalidOperationException("Faltan Player o Main Camera en la escena de origen.");
            }

            foreach (string n in ObsoleteNames)
            {
                while (true)
                {
                    Transform t = FindByName(n);
                    if (t == null)
                    {
                        break;
                    }

                    Object.DestroyImmediate(t.gameObject);
                }
            }

            var root = new GameObject(GameplayRootName).transform;

            BuildTerrain(root);
            BuildLog(root);
            BuildTrack(root);
            BuildPillars(root);
            BuildDeck(root);
            BuildBoulder(root);
            BuildSurfaceDetails();
            MoveWarmLights();

            EditorSceneManager.MarkSceneDirty(scene);
        }

        // ------------------------------------------------------------------
        // Elementos jugables
        // ------------------------------------------------------------------

        static void BuildTerrain(Transform root)
        {
            var go = new GameObject("Terrain_Main");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(ForestOrganicLayout.TerrainCenter.x, ForestOrganicLayout.TerrainCenter.y, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteUtil.LoadSprite(P.Terrain);
            ApplySorting(sr, LayerGameplay, 1);

            var poly = go.AddComponent<PolygonCollider2D>();
            poly.pathCount = 1;
            poly.SetPath(0, ForestOrganicLayout.TerrainColliderPath());
            poly.isTrigger = false;
        }

        static void BuildLog(Transform root)
        {
            Vector2 a = ForestOrganicLayout.LogLeftTop;
            Vector2 b = ForestOrganicLayout.LogRightTop;
            Vector2 mid = (a + b) * 0.5f;
            float length = ForestOrganicLayout.LogLength;

            var go = new GameObject("Log_Fallen_OneWay");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(mid.x, mid.y, 0f);
            go.transform.rotation = Quaternion.Euler(0f, 0f, ForestOrganicLayout.LogAngleDeg);

            // Collider: losa superior del tronco (el cuerpo que cuelga por debajo es solo visual y se cruza).
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(length, ForestOrganicLayout.LogSlabThickness);
            box.offset = new Vector2(0f, -ForestOrganicLayout.LogSlabThickness * 0.5f);
            ConfigureOneWay(go, box);

            var visual = new GameObject("_ForestStyle_LogSprite");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, -ForestOrganicLayout.LogBodyThickness * 0.5f, 0f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteUtil.LoadSprite(P.Log);
            ApplySorting(sr, LayerGameplay, 0);
        }

        static void BuildTrack(Transform root)
        {
            Sprite s = ProceduralSpriteUtil.LoadSprite(P.Track);
            if (s == null)
            {
                return;
            }

            float w = ForestOrganicLayout.TrackTexWidth / ForestOrganicLayout.Ppu;
            float h = ForestOrganicLayout.TrackTexHeight / ForestOrganicLayout.Ppu;
            var go = new GameObject("Decor_RailTrack");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(ForestOrganicLayout.TrackLeftX + w * 0.5f, h * 0.5f - 0.04f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            ApplySorting(sr, LayerGameplay, 2);
        }

        static void BuildPillars(Transform root)
        {
            Sprite s = ProceduralSpriteUtil.LoadSprite(P.Pillar);
            string[] names = { "Pillar_A_Solid", "Pillar_B_Solid", "Pillar_C_Solid" };
            for (int i = 0; i < ForestOrganicLayout.PillarXs.Length; i++)
            {
                var go = new GameObject(names[i]);
                go.transform.SetParent(root, false);
                go.transform.position = new Vector3(ForestOrganicLayout.PillarXs[i], ForestOrganicLayout.PillarCenterY, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = s;
                ApplySorting(sr, LayerGameplay, 3);

                var box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(ForestOrganicLayout.PillarColliderWidth, ForestOrganicLayout.PillarHeight);
                box.offset = Vector2.zero;
            }
        }

        static void BuildDeck(Transform root)
        {
            var go = new GameObject("RailDeck_OneWay");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(ForestOrganicLayout.DeckCenterX, ForestOrganicLayout.DeckCenterY, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteUtil.LoadSprite(P.Deck);
            ApplySorting(sr, LayerGameplay, 4);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(ForestOrganicLayout.DeckLength, ForestOrganicLayout.DeckThickness);
            ConfigureOneWay(go, box);
        }

        static void BuildBoulder(Transform root)
        {
            var go = new GameObject("Boulder_Solid");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(ForestOrganicLayout.BoulderCenterX, 0f, 0f);

            Vector2[] pts = ForestOrganicLayout.BoulderPoints;
            var poly = go.AddComponent<PolygonCollider2D>();
            poly.pathCount = 1;
            poly.SetPath(0, pts);

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (Vector2 p in pts)
            {
                minY = Mathf.Min(minY, p.y);
                maxY = Mathf.Max(maxY, p.y);
            }

            var visual = new GameObject("_ForestStyle_BoulderSprite");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, (minY + maxY) * 0.5f, 0f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = ProceduralSpriteUtil.LoadSprite(P.Boulder);
            ApplySorting(sr, LayerGameplay, 5);
        }

        static void ConfigureOneWay(GameObject go, BoxCollider2D box)
        {
            box.isTrigger = false;
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
        }

        // ------------------------------------------------------------------
        // Detalles: arbustos pegados al terreno y bruma en el barranco
        // ------------------------------------------------------------------

        static void BuildSurfaceDetails()
        {
            Transform env = FindByName("Environment");
            if (env == null)
            {
                return;
            }

            for (int i = env.childCount - 1; i >= 0; i--)
            {
                Transform child = env.GetChild(i);
                if (child.name.StartsWith(StyleChildName))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            Sprite bush = ProceduralSpriteUtil.LoadSprite(ForestAtmosphereArtGenerator.Paths.Bush);

            if (bush == null)
            {
                return;
            }

            // Bajo contraste y detrás del plano de juego (Midground): no parecen obstáculos.
            float[] xs = { -17.5f, -15.0f, -12.4f, -8.9f, -3.9f, 2.2f, 12.4f, 16.5f, 18.5f };
            for (int i = 0; i < xs.Length; i++)
            {
                var go = new GameObject($"{StyleChildName}_Bush_{i:00}");
                go.transform.SetParent(env, false);
                float y = ForestOrganicLayout.SurfaceY(xs[i]) + 0.03f;
                go.transform.position = new Vector3(xs[i], y + 0.3f, 0f);
                go.transform.localScale = new Vector3(1.2f + (i % 2) * 0.3f, 0.9f + (i % 3) * 0.1f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bush;
                sr.color = new Color(0.36f, 0.45f, 0.44f, 0.38f);
                ApplySorting(sr, LayerMidground, -15 + (i % 3));
            }
        }

        static void MoveWarmLights()
        {
            Move("Warm_Platform02", new Vector3(8.4f, 3.2f, 0f));
            Move("Warm_Clearing", new Vector3(-13.5f, 1.3f, 0f));
            Move("Warm_Shelf", new Vector3(15.5f, 2.2f, 0f));
            Move("Warm_Station", new Vector3(4.5f, 1.8f, 0f));
        }

        static void Move(string name, Vector3 pos)
        {
            Transform t = FindByName(name);
            if (t != null)
            {
                t.position = pos;
            }
        }

        // ------------------------------------------------------------------

        static void ApplySorting(SpriteRenderer sr, string layerName, int order)
        {
            sr.sortingLayerName = layerName;
            int id = SortingLayer.NameToID(layerName);
            if (id != 0)
            {
                sr.sortingLayerID = id;
            }

            sr.sortingOrder = order;
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitPath);
            _ = mat; // Gameplay usa el material por defecto (Lit) para recibir la luz 2D.
        }

        static Transform FindByName(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name == name)
                {
                    return t;
                }
            }

            return null;
        }
    }

    /// <summary>Ejecuta la generación una vez si existe Temp/BuildForestOrganic.flag (sale de Play si hace falta).</summary>
    [InitializeOnLoad]
    static class ForestOrganicAutoBuildOnce
    {
        const string FlagPath = "Temp/BuildForestOrganic.flag";
        static bool waiting;

        static ForestOrganicAutoBuildOnce()
        {
            EditorApplication.delayCall += TryRun;
        }

        static void TryRun()
        {
            if (!File.Exists(FlagPath))
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!waiting)
                {
                    waiting = true;
                    EditorApplication.playModeStateChanged += OnPlayMode;
                    EditorApplication.ExitPlaymode();
                }

                return;
            }

            waiting = false;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            try
            {
                File.Delete(FlagPath);
            }
            catch
            {
                return;
            }

            try
            {
                ForestOrganicSceneBuilder.CreateOrUpdate(interactive: false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Días Prestados] Organic auto-build falló: {ex}");
            }
        }

        static void OnPlayMode(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayMode;
                waiting = false;
                EditorApplication.delayCall += TryRun;
            }
        }
    }
}
