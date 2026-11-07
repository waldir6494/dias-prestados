using System.IO;
using DiasPrestados;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Generador histórico de ForestDemo. Menú desactivado: la escena principal es ForestOrganicDemo.
    /// Se conservan las constantes OneWay* (tests / código compartido).
    /// </summary>
    public static class ForestDemoSceneBuilder
    {
        const string PrototypePath = "Assets/Scenes/Prototype.unity";
        const string ForestDemoPath = "Assets/Scenes/ForestDemo.unity";
        const string VisualsRootName = "ForestVisuals";
        const string StyleChildName = "_ForestStyle";
        const string LayerBackground = "Background";
        const string LayerMidground = "Midground";
        const string LayerGameplay = "Gameplay";
        const string LayerForeground = "Foreground";
        const string SpriteUnlitPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const string ParticlesUnlitPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat";

        // Misma configuración que las pruebas de física (Assets/Tests/Editor).
        public const float OneWaySurfaceArc = 180f;
        public const float OneWaySideArc = 1f;
        public const bool OneWayGrouping = true;

        // Menú obsoleto (ForestDemo.unity eliminada).

        public static void CreateOrUpdateForestDemo(bool interactive = true)
        {
            Debug.LogWarning(
                "[Días Prestados] Generación de ForestDemo desactivada. Usa Assets/Scenes/ForestOrganicDemo.unity.");
            if (interactive && !Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Días Prestados",
                    "ForestDemo ya no forma parte del proyecto.\nAbre ForestOrganicDemo.unity.",
                    "Aceptar");
            }
        }

        static Scene OpenForestDemoWithoutPrompt(bool showUi)
        {
            Scene current = EditorSceneManager.GetActiveScene();
            string openPath = (current.path ?? string.Empty).Replace('\\', '/');
            if (string.Equals(openPath, ForestDemoPath, System.StringComparison.OrdinalIgnoreCase))
            {
                return current;
            }

            if (!showUi)
            {
                EditorSceneManager.SaveOpenScenes();
            }

            return EditorSceneManager.OpenScene(ForestDemoPath, OpenSceneMode.Single);
        }

        static void ApplyForestAtmosphere(Scene scene)
        {
            Transform cam = FindByName("Main Camera");
            Transform player = FindByName("Player");
            if (cam == null || player == null)
            {
                throw new System.InvalidOperationException(
                    "ForestDemo debe conservar Main Camera y Player del prototipo.");
            }

            // Quitar fondo simple del prototipo; no tocar Environment jugable ni Player.
            DestroyIfExists("Background");
            DestroyIfExists(VisualsRootName);

            RestyleGameplaySurfaces();
            BuildVisualLayers(cam);
            ConfigureLighting();
            ConfigureCameraMood(cam.GetComponent<Camera>());
            EnsureAmbientParticles(cam);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        static void RestyleGameplaySurfaces()
        {
            Sprite ground = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Ground);
            Sprite shelf = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.GroundShelf);
            Sprite platform = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Platform);
            Sprite platformShadow = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.PlatformShadow);
            Sprite crate = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Crate);
            Sprite rockSolid = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockSolid);
            Sprite bush = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Bush);
            Sprite grass = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Grass);

            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
            {
                if (sr == null)
                {
                    continue;
                }

                if (sr.transform.root.name == "Player")
                {
                    if (sr.gameObject.name == "Visual")
                    {
                        ApplySorting(sr, LayerGameplay, 20);
                    }

                    continue;
                }

                string n = sr.gameObject.name;
                if (n == "Ground")
                {
                    ApplySurfaceSprite(sr, ground, Color.white, keepCollider: true);
                    ApplySorting(sr, LayerGameplay, 0);
                    AlignSpriteToGameplayBounds(sr);
                    ConfigureSolidCollider(sr.gameObject);
                    ClearStyleChildren(sr.transform);
                    // Hierba solo en extremos, baja opacidad, no tapa el borde superior.
                    AddSurfaceDecor(sr.transform, grass, new Vector2(-0.42f, 0.52f), new Vector2(0.7f, 0.42f),
                        new Color(0.75f, 0.9f, 0.72f, 0.7f), LayerMidground, -2);
                    AddSurfaceDecor(sr.transform, grass, new Vector2(0.42f, 0.52f), new Vector2(0.65f, 0.4f),
                        new Color(0.7f, 0.86f, 0.68f, 0.65f), LayerMidground, -2);
                }
                else if (n == "RaisedShelf")
                {
                    ApplySurfaceSprite(sr, shelf != null ? shelf : ground, Color.white, keepCollider: true);
                    ApplySorting(sr, LayerGameplay, 1);
                    AlignSpriteToGameplayBounds(sr);
                    ConfigureSolidCollider(sr.gameObject);
                    ClearStyleChildren(sr.transform);
                    AddSurfaceDecor(sr.transform, grass, new Vector2(-0.4f, 0.52f), new Vector2(0.55f, 0.36f),
                        new Color(0.78f, 0.9f, 0.7f, 0.6f), LayerMidground, -1);
                }
                else if (n.StartsWith("Platform_"))
                {
                    ApplySurfaceSprite(sr, platform, Color.white, keepCollider: true);
                    ApplySorting(sr, LayerGameplay, 2);
                    // El sprite de madera mide 3x1; sin este ajuste el dibujo queda el triple de ancho que el collider.
                    AlignSpriteToGameplayBounds(sr);
                    ConfigureOneWayPlatform(sr.gameObject);
                    ClearStyleChildren(sr.transform);
                    float width = sr.GetComponent<Collider2D>() != null
                        ? sr.GetComponent<Collider2D>().bounds.size.x
                        : 3f;
                    AddSurfaceDecor(sr.transform, platformShadow, new Vector2(0f, -0.72f), new Vector2(width * 1.02f, 0.14f),
                        new Color(1f, 1f, 1f, 0.55f), LayerGameplay, 1);
                    // Musgo mínimo en esquinas; el centro de la tapa queda libre.
                    AddSurfaceDecor(sr.transform, grass, new Vector2(-0.38f, 0.62f), new Vector2(0.28f, 0.2f),
                        new Color(0.7f, 0.88f, 0.62f, 0.55f), LayerGameplay, 3);
                    AddSurfaceDecor(sr.transform, grass, new Vector2(0.38f, 0.62f), new Vector2(0.26f, 0.18f),
                        new Color(0.66f, 0.84f, 0.58f, 0.5f), LayerGameplay, 3);
                }
                else if (n.StartsWith("Obstacle_Crate"))
                {
                    ApplySurfaceSprite(sr, crate, Color.white, keepCollider: true);
                    ApplySorting(sr, LayerGameplay, 3);
                    AlignSpriteToGameplayBounds(sr);
                    ConfigureSolidCollider(sr.gameObject);
                    ClearStyleChildren(sr.transform);
                }
                else if (n.StartsWith("Rock_"))
                {
                    ApplySurfaceSprite(sr, rockSolid != null ? rockSolid : ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockLarge),
                        Color.white, keepCollider: true);
                    ApplySorting(sr, LayerGameplay, 4);
                    // La elipse no llena las esquinas; el collider queda un poco dentro del sprite, sin cambiar la escala visual.
                    FitColliderInsideSprite(sr, fill: 0.94f);
                    ConfigureSolidCollider(sr.gameObject);
                    ClearStyleChildren(sr.transform);
                }
                else if (n.StartsWith("Wall_"))
                {
                    sr.color = new Color(1f, 1f, 1f, 0f);
                    ApplySorting(sr, LayerGameplay, 0);
                    ConfigureSolidCollider(sr.gameObject);
                }
            }

            StripDecorativeColliders();

            // Arbustos decorativos: midground, bajo contraste, nunca parecen sólidos.
            Transform env = FindByName("Environment");
            if (env != null && bush != null)
            {
                ClearStyleChildren(env);
                float[] xs = { -15.5f, -11f, -7.2f, -1.5f, 2.8f, 7.5f, 11.2f, 16.5f };
                for (int i = 0; i < xs.Length; i++)
                {
                    var go = new GameObject($"{StyleChildName}_Bush_{i:00}");
                    go.transform.SetParent(env, false);
                    go.transform.position = new Vector3(xs[i], 0.35f + (i % 3) * 0.04f, 0f);
                    go.transform.localScale = new Vector3(1.0f + (i % 2) * 0.25f, 0.85f + (i % 3) * 0.12f, 1f);
                    var bushSr = go.AddComponent<SpriteRenderer>();
                    bushSr.sprite = bush;
                    bushSr.color = new Color(0.82f, 0.95f, 0.8f, 0.78f);
                    ApplySorting(bushSr, LayerMidground, -15 + (i % 3));
                }
            }
        }

        static void ApplySurfaceSprite(SpriteRenderer sr, Sprite sprite, Color color, bool keepCollider)
        {
            if (sprite != null)
            {
                sr.sprite = sprite;
                sr.drawMode = SpriteDrawMode.Simple;
            }

            sr.color = color;
            if (!keepCollider)
            {
                foreach (var col in sr.GetComponents<Collider2D>())
                {
                    Object.DestroyImmediate(col);
                }
            }
        }

        static void EnsureSortingLayersExist()
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning("[Días Prestados] No se pudo cargar TagManager.asset.");
                return;
            }

            var tagManager = new SerializedObject(assets[0]);
            SerializedProperty sortingLayers = tagManager.FindProperty("m_SortingLayers");
            if (sortingLayers == null)
            {
                return;
            }

            string[] required = { LayerBackground, LayerMidground, LayerGameplay, LayerForeground };
            // uniqueID debe caber en int positivo al serializar con intValue (> int.MaxValue se corrompe).
            int[] ids = { 11001001, 11001002, 11001003, 11001004 };

            for (int r = 0; r < required.Length; r++)
            {
                int index = FindSortingLayerPropertyIndex(sortingLayers, required[r]);
                if (index < 0)
                {
                    sortingLayers.InsertArrayElementAtIndex(sortingLayers.arraySize);
                    index = sortingLayers.arraySize - 1;
                }

                SerializedProperty entry = sortingLayers.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("name").stringValue = required[r];
                entry.FindPropertyRelative("uniqueID").intValue = ids[r];
                entry.FindPropertyRelative("locked").intValue = 0;
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset("ProjectSettings/TagManager.asset", ImportAssetOptions.ForceUpdate);

            for (int i = 0; i < required.Length; i++)
            {
                if (SortingLayer.NameToID(required[i]) == 0)
                {
                    Debug.LogWarning(
                        $"[Días Prestados] Sorting Layer '{required[i]}' sigue en Default tras registrar TagManager.");
                }
            }
        }

        static int FindSortingLayerPropertyIndex(SerializedProperty sortingLayers, string layerName)
        {
            for (int i = 0; i < sortingLayers.arraySize; i++)
            {
                SerializedProperty nameProp = sortingLayers.GetArrayElementAtIndex(i).FindPropertyRelative("name");
                if (nameProp != null && nameProp.stringValue == layerName)
                {
                    return i;
                }
            }

            return -1;
        }

        static void ApplySorting(SpriteRenderer sr, string layerName, int order)
        {
            if (sr == null)
            {
                return;
            }

            // sortingLayerName fuerza la resolución por nombre tras EnsureSortingLayersExist.
            sr.sortingLayerName = layerName;
            int id = SortingLayer.NameToID(layerName);
            if (id != 0)
            {
                sr.sortingLayerID = id;
            }

            sr.sortingOrder = order;
        }

        /// <summary>
        /// Conserva el AABB físico actual (el tamaño jugable del prototipo) y escala el sprite para que coincida.
        /// El sprite de plataforma/suelo no mide 1x1: dejar el scale del cuadrado blanco estiraba solo el dibujo.
        /// </summary>
        static void AlignSpriteToGameplayBounds(SpriteRenderer sr)
        {
            if (sr == null || sr.sprite == null)
            {
                return;
            }

            var box = sr.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                return;
            }

            Vector2 native = sr.sprite.bounds.size;
            if (native.x < 0.0001f || native.y < 0.0001f)
            {
                return;
            }

            Vector3 lossy = sr.transform.lossyScale;
            Vector2 world = new Vector2(Mathf.Abs(lossy.x) * box.size.x, Mathf.Abs(lossy.y) * box.size.y);
            Vector3 parentScale = sr.transform.parent != null ? sr.transform.parent.lossyScale : Vector3.one;
            sr.transform.localScale = new Vector3(
                world.x / native.x / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                world.y / native.y / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
                1f);

            box.size = native;
            box.offset = sr.sprite.bounds.center;
            box.edgeRadius = 0f;
        }

        static void FitColliderInsideSprite(SpriteRenderer sr, float fill)
        {
            if (sr == null || sr.sprite == null)
            {
                return;
            }

            var box = sr.GetComponent<BoxCollider2D>();
            if (box == null)
            {
                return;
            }

            box.size = (Vector2)sr.sprite.bounds.size * fill;
            box.offset = sr.sprite.bounds.center;
            box.edgeRadius = 0f;
            box.usedByEffector = false;
        }

        static void ConfigureOneWayPlatform(GameObject go)
        {
            var box = go != null ? go.GetComponent<BoxCollider2D>() : null;
            if (box == null)
            {
                return;
            }

            Collider2D[] existing = go.GetComponents<Collider2D>();
            for (int i = existing.Length - 1; i >= 0; i--)
            {
                if (existing[i] != box)
                {
                    Object.DestroyImmediate(existing[i]);
                }
            }

            box.isTrigger = false;
            box.usedByEffector = true;
            var effector = go.GetComponent<PlatformEffector2D>();
            if (effector == null)
            {
                effector = go.AddComponent<PlatformEffector2D>();
            }

            effector.useOneWay = true;
            effector.useOneWayGrouping = OneWayGrouping;
            effector.surfaceArc = OneWaySurfaceArc;
            effector.sideArc = OneWaySideArc;
            effector.rotationalOffset = 0f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            effector.useColliderMask = false;
        }

        static void ConfigureSolidCollider(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            var effector = go.GetComponent<PlatformEffector2D>();
            if (effector != null)
            {
                Object.DestroyImmediate(effector);
            }

            foreach (var col in go.GetComponents<Collider2D>())
            {
                col.isTrigger = false;
                col.usedByEffector = false;
            }
        }

        static void StripDecorativeColliders()
        {
            var doomed = new System.Collections.Generic.List<Collider2D>();
            foreach (var col in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Exclude))
            {
                if (col == null)
                {
                    continue;
                }

                string n = col.gameObject.name;
                string root = col.transform.root != null ? col.transform.root.name : string.Empty;
                bool decor = n.StartsWith(StyleChildName)
                             || n.StartsWith("FG_")
                             || root == VisualsRootName;
                if (decor)
                {
                    doomed.Add(col);
                }
            }

            for (int i = 0; i < doomed.Count; i++)
            {
                if (doomed[i] != null)
                {
                    Object.DestroyImmediate(doomed[i]);
                }
            }
        }

        static void ClearStyleChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child.name.StartsWith(StyleChildName))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Coloca decoración en fracciones del collider (0 = centro, ±0.5 = bordes) con tamaño en unidades de mundo.
        /// No añade Collider2D.
        /// </summary>
        static void AddSurfaceDecor(
            Transform parent,
            Sprite sprite,
            Vector2 anchor,
            Vector2 worldSize,
            Color color,
            string sortingLayer,
            int sortingOrder)
        {
            if (sprite == null || parent == null)
            {
                return;
            }

            var col = parent.GetComponent<Collider2D>();
            Vector2 surface = col != null ? (Vector2)col.bounds.size : Vector2.one;
            Vector2 origin = col != null ? (Vector2)col.bounds.center : (Vector2)parent.position;
            Vector3 worldPos = origin + Vector2.Scale(anchor, surface);

            var go = new GameObject($"{StyleChildName}_Decor");
            go.transform.SetParent(null, false);
            go.transform.position = new Vector3(worldPos.x, worldPos.y, parent.position.z);
            Vector2 native = sprite.bounds.size;
            go.transform.localScale = new Vector3(
                worldSize.x / Mathf.Max(0.0001f, native.x),
                worldSize.y / Mathf.Max(0.0001f, native.y),
                1f);
            go.transform.SetParent(parent, true);

            var decorSr = go.AddComponent<SpriteRenderer>();
            decorSr.sprite = sprite;
            decorSr.color = color;
            ApplySorting(decorSr, sortingLayer, sortingOrder);
        }

        static void BuildVisualLayers(Transform cam)
        {
            Sprite sky = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.SkyGradient);
            Sprite mountainFar = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.MountainFar);
            Sprite mountainNear = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.MountainNear);
            Sprite hill = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Hill);
            Sprite pineA = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.TreePineA);
            Sprite pineB = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.TreePineB);
            Sprite broad = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.TreeBroad);
            Sprite rail = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Rail);
            Sprite station = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Station);
            Sprite cloud = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Cloud);
            Sprite rockDecor = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockDecor)
                               ?? ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockSmall);
            Sprite bush = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Bush);
            Sprite grass = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Grass);

            var root = new GameObject(VisualsRootName).transform;

            // Fondo pintado en Unlit: el atardecer se ve aunque la luz 2D no lo alcance.
            var skyLayer = CreateLayer(root, "SkySunset", cam, 1f, 1f);
            CreateSprite(skyLayer, "Sky", sky, new Vector3(0f, 4.5f, 0f), new Vector3(55f, 18f, 1f),
                Color.white, LayerBackground, -200, unlit: true);

            var clouds = CreateLayer(root, "Clouds", cam, 0.95f, 0.9f);
            CreateSprite(clouds, "Cloud_A", cloud, new Vector3(-8f, 6.5f, 0f), new Vector3(6f, 2.2f, 1f),
                new Color(1f, 0.82f, 0.7f, 0.55f), LayerBackground, -195, unlit: true);
            CreateSprite(clouds, "Cloud_B", cloud, new Vector3(6f, 7.2f, 0f), new Vector3(7.5f, 2.4f, 1f),
                new Color(1f, 0.75f, 0.62f, 0.4f), LayerBackground, -194, unlit: true);

            var mountainsFar = CreateLayer(root, "MountainsFar", cam, 0.9f, 0.55f);
            CreateSprite(mountainsFar, "Mountains_Far_A", mountainFar, new Vector3(-6f, 2.6f, 0f), new Vector3(1.8f, 1.5f, 1f),
                new Color(0.85f, 0.78f, 0.9f, 1f), LayerBackground, -180, unlit: true);
            CreateSprite(mountainsFar, "Mountains_Far_B", mountainFar, new Vector3(10f, 2.4f, 0f), new Vector3(-1.6f, 1.35f, 1f),
                new Color(0.8f, 0.74f, 0.86f, 1f), LayerBackground, -179, unlit: true);

            var mountainsNear = CreateLayer(root, "MountainsNear", cam, 0.78f, 0.4f);
            CreateSprite(mountainsNear, "Mountains_Near", mountainNear, new Vector3(2f, 2.0f, 0f), new Vector3(2.1f, 1.4f, 1f),
                new Color(0.72f, 0.7f, 0.78f, 1f), LayerBackground, -170, unlit: true);

            var hills = CreateLayer(root, "Hills", cam, 0.68f, 0.3f);
            CreateSprite(hills, "Hills_A", hill, new Vector3(-10f, 0.9f, 0f), new Vector3(1.4f, 1.1f, 1f),
                new Color(0.7f, 0.82f, 0.72f), LayerBackground, -160, unlit: true);
            CreateSprite(hills, "Hills_B", hill, new Vector3(8f, 0.85f, 0f), new Vector3(1.6f, 1.15f, 1f),
                new Color(0.64f, 0.76f, 0.66f), LayerBackground, -159, unlit: true);

            var railway = CreateLayer(root, "Railway", cam, 0.62f, 0.25f);
            CreateSprite(railway, "Rails_Left", rail, new Vector3(-6f, 0.55f, 0f), new Vector3(1.4f, 1f, 1f),
                new Color(0.75f, 0.75f, 0.78f), LayerBackground, -150, unlit: true);
            CreateSprite(railway, "Rails_Right", rail, new Vector3(8f, 0.55f, 0f), new Vector3(1.5f, 1f, 1f),
                new Color(0.7f, 0.7f, 0.74f), LayerBackground, -150, unlit: true);
            CreateSprite(railway, "AbandonedStation", station, new Vector3(4.5f, 1.55f, 0f), new Vector3(1.35f, 1.35f, 1f),
                Color.white, LayerBackground, -145, unlit: true);

            // La textura del árbol ya es verde oscuro. Un tinte casi negro la apagaba del todo.
            var treesFar = CreateLayer(root, "TreesFar", cam, 0.48f, 0.18f);
            ScatterTrees(treesFar, pineA, pineB, broad,
                new[] { -18f, -14f, -9f, -4f, 1f, 6f, 11f, 16f },
                baseY: 1.4f, scale: 1.5f, tone: new Color(0.9f, 0.92f, 0.98f, 1f),
                sortingLayer: LayerBackground, sorting: -120);

            var treesMid = CreateLayer(root, "TreesMid", cam, 0.3f, 0.1f);
            ScatterTrees(treesMid, pineB, pineA, broad,
                new[] { -16.5f, -12f, -7.5f, -2.5f, 3f, 8.5f, 13.5f, 17.5f },
                baseY: 1.55f, scale: 1.85f, tone: new Color(0.82f, 0.95f, 0.84f, 1f),
                sortingLayer: LayerMidground, sorting: -40);

            var treesNear = CreateLayer(root, "TreesNear", cam, 0.14f, 0.05f);
            ScatterTrees(treesNear, broad, pineA, pineB,
                new[] { -17.5f, -13.2f, -8.8f, -3.8f, 0.8f, 5.8f, 10.8f, 15.2f, 18.2f },
                baseY: 1.7f, scale: 2.15f, tone: new Color(0.75f, 0.9f, 0.76f, 1f),
                sortingLayer: LayerMidground, sorting: -25);

            // Primer plano: siluetas suaves, claramente no sólidas, pero legibles.
            var foreground = CreateLayer(root, "Foreground", cam, -0.08f, 0.02f);
            CreateSprite(foreground, "FG_Bush_L", bush, new Vector3(-17.2f, 0.35f, 0f), new Vector3(1.6f, 1.2f, 1f),
                new Color(0.7f, 0.88f, 0.68f, 0.82f), LayerForeground, 10);
            CreateSprite(foreground, "FG_Bush_R", bush, new Vector3(17.4f, 0.4f, 0f), new Vector3(1.8f, 1.3f, 1f),
                new Color(0.66f, 0.84f, 0.64f, 0.82f), LayerForeground, 10);
            CreateSprite(foreground, "FG_Rock_L", rockDecor, new Vector3(-15.8f, 0.25f, 0f), new Vector3(1.4f, 1.1f, 1f),
                new Color(0.78f, 0.8f, 0.84f, 0.75f), LayerForeground, 8);
            CreateSprite(foreground, "FG_Rock_R", rockDecor, new Vector3(14.8f, 0.22f, 0f), new Vector3(1.5f, 1.15f, 1f),
                new Color(0.74f, 0.76f, 0.8f, 0.72f), LayerForeground, 8);
            CreateSprite(foreground, "FG_Grass_A", grass, new Vector3(-10.5f, 0.2f, 0f), new Vector3(1.2f, 1f, 1f),
                new Color(0.7f, 0.9f, 0.66f, 0.75f), LayerForeground, 6);
            CreateSprite(foreground, "FG_Grass_B", grass, new Vector3(9.2f, 0.18f, 0f), new Vector3(1.3f, 1.05f, 1f),
                new Color(0.66f, 0.86f, 0.62f, 0.72f), LayerForeground, 6);
        }

        static Transform CreateLayer(Transform parent, string name, Transform cam, float px, float py)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = Vector3.zero;
            var parallax = go.AddComponent<ParallaxLayer>();
            parallax.Configure(cam, px, py);
            return go.transform;
        }

        static void CreateSprite(
            Transform parent,
            string name,
            Sprite sprite,
            Vector3 position,
            Vector3 scale,
            Color color,
            string sortingLayer,
            int sortingOrder,
            bool unlit = false)
        {
            if (sprite == null)
            {
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            ApplySorting(sr, sortingLayer, sortingOrder);
            if (unlit)
            {
                Material mat = SpriteUnlitMaterial();
                if (mat != null)
                {
                    sr.sharedMaterial = mat;
                }
            }
        }

        static void ScatterTrees(
            Transform parent,
            Sprite a,
            Sprite b,
            Sprite c,
            float[] xs,
            float baseY,
            float scale,
            Color tone,
            string sortingLayer,
            int sorting)
        {
            Sprite[] set = { a, b, c };
            for (int i = 0; i < xs.Length; i++)
            {
                Sprite sprite = set[i % set.Length];
                float s = scale * (0.85f + (i % 4) * 0.08f);
                Color color = Color.Lerp(tone, Color.white, (i % 3) * 0.03f);
                CreateSprite(
                    parent,
                    $"Tree_{i:00}",
                    sprite,
                    new Vector3(xs[i], baseY + (i % 2) * 0.15f, 0f),
                    new Vector3((i % 2 == 0 ? 1f : -1f) * s * 0.55f, s, 1f),
                    color,
                    sortingLayer,
                    sorting + (i % 3));
            }
        }

        static void ConfigureLighting()
        {
            // Ambiente cálido de atardecer. La visibilidad no depende de los puntos.
            Light2D global = null;
            foreach (var light in Object.FindObjectsByType<Light2D>(FindObjectsInactive.Exclude))
            {
                if (light.lightType == Light2D.LightType.Global)
                {
                    global = light;
                    break;
                }
            }

            if (global == null)
            {
                var go = new GameObject("Global Light 2D");
                global = go.AddComponent<Light2D>();
                global.lightType = Light2D.LightType.Global;
            }

            global.blendStyleIndex = 0;
            global.color = new Color(1f, 0.86f, 0.70f, 1f);
            global.intensity = 1.08f;
            ApplyLightToAllSortingLayers(global);

            DestroyIfExists("WarmLights");
            var root = new GameObject("WarmLights").transform;

            CreateWarmPoint(root, "Warm_Station", new Vector3(4.5f, 1.8f, 0f), new Color(1f, 0.62f, 0.32f), 0.42f, 4.6f);
            CreateWarmPoint(root, "Warm_Platform02", new Vector3(-3.2f, 3.8f, 0f), new Color(1f, 0.72f, 0.4f), 0.26f, 3.2f);
            CreateWarmPoint(root, "Warm_Clearing", new Vector3(-12.5f, 1.2f, 0f), new Color(1f, 0.68f, 0.42f), 0.24f, 3.4f);
            CreateWarmPoint(root, "Warm_Shelf", new Vector3(14f, 1.4f, 0f), new Color(1f, 0.6f, 0.35f), 0.28f, 3.6f);
        }

        static void ApplyLightToAllSortingLayers(Light2D light)
        {
            if (light == null)
            {
                return;
            }

            SortingLayer[] layers = SortingLayer.layers;
            var ids = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                ids[i] = layers[i].id;
            }

            light.targetSortingLayers = ids;
            light.shadowsEnabled = false;
            light.shadowIntensity = 0f;
        }

        static void CreateWarmPoint(Transform parent, string name, Vector3 pos, Color color, float intensity, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.blendStyleIndex = 0;
            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius = radius * 0.2f;
            light.pointLightOuterRadius = radius;
            light.falloffIntensity = 0.65f;
            ApplyLightToAllSortingLayers(light);
        }

        static Material SpriteUnlitMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitPath);
        }

        static Material ParticlesUnlitMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<Material>(ParticlesUnlitPath);
        }

        static void ConfigureCameraMood(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            cam.orthographic = true;
            cam.backgroundColor = new Color(0.55f, 0.38f, 0.42f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        static void EnsureAmbientParticles(Transform cam)
        {
            DestroyIfExists("AmbientDust");

            var go = new GameObject("AmbientDust");
            go.transform.position = cam.position + new Vector3(0f, 1.5f, 5f);
            var dustFollow = go.AddComponent<AmbientDustParticles>();
            dustFollow.Configure(cam, new Vector3(0f, 1.2f, 5f));

            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = go.AddComponent<ParticleSystem>();
            }

            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 12f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.9f, 0.7f, 0.35f),
                new Color(0.85f, 0.75f, 0.95f, 0.2f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 80;
            main.gravityModifier = -0.01f;

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 8f, 1f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            // Unity exige el mismo MinMaxCurve.mode en cada grupo de ejes (TwoConstants).
            velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalOffsetX = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalOffsetY = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.orbitalOffsetZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.radial = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.speedModifier = new ParticleSystem.MinMaxCurve(1f, 1f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.white, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.45f, 0.2f),
                    new GradientAlphaKey(0.2f, 0.8f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = g;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = LayerForeground;
            renderer.sortingOrder = 5;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material particleMat = ParticlesUnlitMaterial();
            if (particleMat != null)
            {
                renderer.sharedMaterial = particleMat;
            }
            else
            {
                Debug.LogWarning("[Días Prestados] No se encontró ParticlesUnlit.mat; las motas pueden verse magenta.");
            }

            Sprite dust = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Dust);
            if (dust != null)
            {
                var sheet = ps.textureSheetAnimation;
                sheet.enabled = true;
                sheet.mode = ParticleSystemAnimationMode.Sprites;
                if (sheet.spriteCount == 0)
                {
                    sheet.AddSprite(dust);
                }
                else
                {
                    sheet.SetSprite(0, dust);
                }
            }

            var collision = ps.collision;
            collision.enabled = false;
            var trigger = ps.trigger;
            trigger.enabled = false;
        }

        static Transform FindByName(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name == name && t.parent == null)
                {
                    return t;
                }
            }

            // Fallback: cualquier coincidencia de nombre.
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name == name)
                {
                    return t;
                }
            }

            return null;
        }

        static void DestroyIfExists(string name)
        {
            Transform t = FindByName(name);
            if (t != null)
            {
                Object.DestroyImmediate(t.gameObject);
            }
        }
    }
}
