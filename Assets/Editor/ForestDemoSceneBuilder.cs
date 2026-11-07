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
    /// Crea/actualiza ForestDemo a partir de Prototype sin destruir Prototype.
    /// Solo regenera el nodo gestionado "ForestVisuals" y retoques visuales de Environment.
    /// Menú: Tools > Dias Prestados > Crear / actualizar Forest Demo
    /// </summary>
    public static class ForestDemoSceneBuilder
    {
        const string PrototypePath = "Assets/Scenes/Prototype.unity";
        const string ForestDemoPath = "Assets/Scenes/ForestDemo.unity";
        const string VisualsRootName = "ForestVisuals";
        const string StyleChildName = "_ForestStyle";

        [MenuItem("Tools/Dias Prestados/Crear / actualizar Forest Demo")]
        public static void CreateOrUpdateForestDemoMenu()
        {
            CreateOrUpdateForestDemo(interactive: true);
        }

        public static void CreateOrUpdateForestDemo(bool interactive = true)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new System.InvalidOperationException(
                    "Sal de Play Mode antes de generar ForestDemo (EditorSceneManager.OpenScene no funciona en Play).");
            }

            if (!File.Exists(PrototypePath) && AssetDatabase.LoadAssetAtPath<SceneAsset>(PrototypePath) == null)
            {
                if (interactive && !Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Días Prestados",
                        "No existe Prototype.unity.\nGenera primero el prototipo con Tools > Dias Prestados > Crear prototipo.",
                        "Aceptar");
                }

                throw new FileNotFoundException("Falta Prototype.unity", PrototypePath);
            }

            bool showUi = interactive && !Application.isBatchMode;

            try
            {
                if (showUi)
                {
                    EditorUtility.DisplayProgressBar("Días Prestados", "Generando arte del bosque...", 0.15f);
                }

                ForestArtGenerator.EnsureAllSprites();

                bool forestExists = File.Exists(ForestDemoPath) ||
                                    AssetDatabase.LoadAssetAtPath<SceneAsset>(ForestDemoPath) != null;

                if (!forestExists)
                {
                    if (showUi)
                    {
                        EditorUtility.DisplayProgressBar("Días Prestados", "Copiando Prototype → ForestDemo...", 0.35f);
                    }

                    if (!AssetDatabase.CopyAsset(PrototypePath, ForestDemoPath))
                    {
                        throw new IOException("No se pudo copiar Prototype.unity a ForestDemo.unity.");
                    }

                    AssetDatabase.Refresh();
                }
                else if (showUi)
                {
                    bool update = EditorUtility.DisplayDialog(
                        "Días Prestados",
                        "ForestDemo.unity ya existe.\n\nSe actualizarán solo los visuales gestionados (" +
                        VisualsRootName + ") y el estilo de suelo/plataformas.\n" +
                        "No se tocará Prototype.unity ni el PlayerController.\n\n¿Continuar?",
                        "Actualizar visuales",
                        "Cancelar");
                    if (!update)
                    {
                        return;
                    }
                }
                else
                {
                    Debug.Log("[Días Prestados] ForestDemo existe: se actualizan solo visuales gestionados.");
                }

                if (showUi)
                {
                    EditorUtility.DisplayProgressBar("Días Prestados", "Aplicando atmósfera de bosque...", 0.6f);
                }

                var scene = EditorSceneManager.OpenScene(ForestDemoPath, OpenSceneMode.Single);
                ApplyForestAtmosphere(scene);

                bool saved = EditorSceneManager.SaveScene(scene, ForestDemoPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!saved)
                {
                    throw new IOException("No se pudo guardar ForestDemo.unity.");
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ForestDemoPath);
                if (sceneAsset != null && showUi)
                {
                    Selection.activeObject = sceneAsset;
                    EditorGUIUtility.PingObject(sceneAsset);
                }

                Debug.Log("[Días Prestados] ForestDemo listo en Assets/Scenes/ForestDemo.unity");
                if (showUi)
                {
                    EditorUtility.DisplayDialog(
                        "Días Prestados",
                        "Forest Demo creado/actualizado.\n\nAbre: Assets/Scenes/ForestDemo.unity\n" +
                        "Prototype.unity se conserva intacto.\n\nPulsa Play y recorre el bosque.",
                        "Aceptar");
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
            Sprite platform = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Platform);
            Sprite rock = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockLarge);
            Sprite bush = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Bush);
            Sprite grass = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Grass);

            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
            {
                if (sr == null || sr.gameObject.name is "Player" || sr.transform.root.name == "Player")
                {
                    continue;
                }

                string n = sr.gameObject.name;
                if (n is "Ground" or "RaisedShelf")
                {
                    ApplySurfaceSprite(sr, ground, Color.white, keepCollider: true);
                    ClearStyleChildren(sr.transform);
                    AddNoCollisionDecor(sr.transform, grass, new Vector3(-0.35f, 0.55f, 0f), new Vector3(0.08f, 0.25f, 1f),
                        new Color(0.55f, 0.75f, 0.45f), 6);
                    AddNoCollisionDecor(sr.transform, grass, new Vector3(0.3f, 0.55f, 0f), new Vector3(0.07f, 0.22f, 1f),
                        new Color(0.45f, 0.65f, 0.38f), 6);
                }
                else if (n.StartsWith("Platform_"))
                {
                    ApplySurfaceSprite(sr, platform, Color.white, keepCollider: true);
                    ClearStyleChildren(sr.transform);
                    AddNoCollisionDecor(sr.transform, grass, new Vector3(-0.3f, 0.7f, 0f), new Vector3(0.12f, 0.45f, 1f),
                        new Color(0.5f, 0.7f, 0.4f), 5);
                }
                else if (n.StartsWith("Obstacle_Crate"))
                {
                    sr.color = new Color(0.48f, 0.32f, 0.18f);
                    ClearStyleChildren(sr.transform);
                }
                else if (n.StartsWith("Rock_"))
                {
                    ApplySurfaceSprite(sr, rock, new Color(0.7f, 0.7f, 0.74f), keepCollider: true);
                }
                else if (n.StartsWith("Wall_"))
                {
                    // Muros invisibles de límite: sin sprite visible.
                    sr.color = new Color(1f, 1f, 1f, 0f);
                }
            }

            // Arbustos decorativos junto al suelo (sin colisión), hijos de Environment si existe.
            Transform env = FindByName("Environment");
            if (env != null && bush != null)
            {
                ClearStyleChildren(env);
                float[] xs = { -15.5f, -11f, -7.2f, -1.5f, 2.8f, 7.5f, 11.2f, 16.5f };
                for (int i = 0; i < xs.Length; i++)
                {
                    var go = new GameObject($"{StyleChildName}_Bush_{i:00}");
                    go.transform.SetParent(env, false);
                    go.transform.position = new Vector3(xs[i], 0.45f + (i % 3) * 0.05f, 0f);
                    go.transform.localScale = new Vector3(1.1f + (i % 2) * 0.3f, 0.9f + (i % 3) * 0.15f, 1f);
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = bush;
                    sr.color = new Color(0.35f, 0.48f, 0.32f);
                    sr.sortingOrder = 8;
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
            // Los colliders Box en objetos escalados siguen usando size local (1,1) del generador original.
            if (!keepCollider)
            {
                foreach (var col in sr.GetComponents<Collider2D>())
                {
                    Object.DestroyImmediate(col);
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

        static void AddNoCollisionDecor(
            Transform parent,
            Sprite sprite,
            Vector3 localPos,
            Vector3 localScale,
            Color color,
            int sortingOrder)
        {
            if (sprite == null)
            {
                return;
            }

            var go = new GameObject($"{StyleChildName}_Decor");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
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
            Sprite rockSmall = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.RockSmall);
            Sprite bush = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Bush);
            Sprite grass = ProceduralSpriteUtil.LoadSprite(ForestArtGenerator.Paths.Grass);

            var root = new GameObject(VisualsRootName).transform;

            // Cielo atardecer (casi pegado a cámara).
            var skyLayer = CreateLayer(root, "SkySunset", cam, 1f, 1f);
            CreateSprite(skyLayer, "Sky", sky, new Vector3(0f, 4.5f, 0f), new Vector3(55f, 18f, 1f),
                Color.white, -200);

            var clouds = CreateLayer(root, "Clouds", cam, 0.95f, 0.9f);
            CreateSprite(clouds, "Cloud_A", cloud, new Vector3(-8f, 6.5f, 0f), new Vector3(6f, 2.2f, 1f),
                new Color(1f, 0.82f, 0.7f, 0.55f), -195);
            CreateSprite(clouds, "Cloud_B", cloud, new Vector3(6f, 7.2f, 0f), new Vector3(7.5f, 2.4f, 1f),
                new Color(1f, 0.75f, 0.62f, 0.4f), -194);

            var mountainsFar = CreateLayer(root, "MountainsFar", cam, 0.9f, 0.55f);
            CreateSprite(mountainsFar, "Mountains_Far_A", mountainFar, new Vector3(-6f, 2.6f, 0f), new Vector3(1.8f, 1.5f, 1f),
                new Color(0.55f, 0.48f, 0.62f, 1f), -180);
            CreateSprite(mountainsFar, "Mountains_Far_B", mountainFar, new Vector3(10f, 2.4f, 0f), new Vector3(-1.6f, 1.35f, 1f),
                new Color(0.5f, 0.45f, 0.58f, 1f), -179);

            var mountainsNear = CreateLayer(root, "MountainsNear", cam, 0.78f, 0.4f);
            CreateSprite(mountainsNear, "Mountains_Near", mountainNear, new Vector3(2f, 2.0f, 0f), new Vector3(2.1f, 1.4f, 1f),
                new Color(0.38f, 0.36f, 0.42f, 1f), -170);

            var hills = CreateLayer(root, "Hills", cam, 0.68f, 0.3f);
            CreateSprite(hills, "Hills_A", hill, new Vector3(-10f, 0.9f, 0f), new Vector3(1.4f, 1.1f, 1f),
                new Color(0.28f, 0.34f, 0.30f), -160);
            CreateSprite(hills, "Hills_B", hill, new Vector3(8f, 0.85f, 0f), new Vector3(1.6f, 1.15f, 1f),
                new Color(0.24f, 0.30f, 0.26f), -159);

            // Estación y raíles (fondo, sin colisión).
            var railway = CreateLayer(root, "Railway", cam, 0.62f, 0.25f);
            CreateSprite(railway, "Rails_Left", rail, new Vector3(-6f, 0.55f, 0f), new Vector3(1.4f, 1f, 1f),
                new Color(0.2f, 0.2f, 0.22f), -150);
            CreateSprite(railway, "Rails_Right", rail, new Vector3(8f, 0.55f, 0f), new Vector3(1.5f, 1f, 1f),
                new Color(0.18f, 0.18f, 0.2f), -150);
            CreateSprite(railway, "AbandonedStation", station, new Vector3(4.5f, 1.55f, 0f), new Vector3(1.35f, 1.35f, 1f),
                Color.white, -145);

            var treesFar = CreateLayer(root, "TreesFar", cam, 0.48f, 0.18f);
            ScatterTrees(treesFar, pineA, pineB, broad,
                new[] { -18f, -14f, -9f, -4f, 1f, 6f, 11f, 16f },
                baseY: 1.4f, scale: 1.5f, tone: new Color(0.25f, 0.32f, 0.28f), sorting: -130);

            var treesMid = CreateLayer(root, "TreesMid", cam, 0.3f, 0.1f);
            ScatterTrees(treesMid, pineB, pineA, broad,
                new[] { -16.5f, -12f, -7.5f, -2.5f, 3f, 8.5f, 13.5f, 17.5f },
                baseY: 1.55f, scale: 1.85f, tone: new Color(0.18f, 0.28f, 0.2f), sorting: -110);

            var treesNear = CreateLayer(root, "TreesNear", cam, 0.14f, 0.05f);
            ScatterTrees(treesNear, broad, pineA, pineB,
                new[] { -17.5f, -13.2f, -8.8f, -3.8f, 0.8f, 5.8f, 10.8f, 15.2f, 18.2f },
                baseY: 1.7f, scale: 2.15f, tone: new Color(0.12f, 0.22f, 0.14f), sorting: -90);

            // Primer plano: vegetación y rocas (delante del jugador visualmente en bordes, sin colisión).
            var foreground = CreateLayer(root, "Foreground", cam, -0.08f, 0.02f);
            CreateSprite(foreground, "FG_Bush_L", bush, new Vector3(-17.2f, 0.35f, 0f), new Vector3(1.6f, 1.2f, 1f),
                new Color(0.15f, 0.28f, 0.16f), 35);
            CreateSprite(foreground, "FG_Bush_R", bush, new Vector3(17.4f, 0.4f, 0f), new Vector3(1.8f, 1.3f, 1f),
                new Color(0.14f, 0.26f, 0.15f), 35);
            CreateSprite(foreground, "FG_Rock_L", rockSmall, new Vector3(-15.8f, 0.25f, 0f), new Vector3(1.4f, 1.1f, 1f),
                new Color(0.35f, 0.34f, 0.38f), 34);
            CreateSprite(foreground, "FG_Rock_R", rockSmall, new Vector3(14.8f, 0.22f, 0f), new Vector3(1.5f, 1.15f, 1f),
                new Color(0.32f, 0.31f, 0.35f), 34);
            CreateSprite(foreground, "FG_Grass_A", grass, new Vector3(-10.5f, 0.2f, 0f), new Vector3(1.2f, 1f, 1f),
                new Color(0.2f, 0.35f, 0.18f), 33);
            CreateSprite(foreground, "FG_Grass_B", grass, new Vector3(9.2f, 0.18f, 0f), new Vector3(1.3f, 1.05f, 1f),
                new Color(0.18f, 0.32f, 0.16f), 33);
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
            int sortingOrder)
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
            sr.sortingOrder = sortingOrder;
            // Garantía: decoración de fondo nunca añade colliders.
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
            int sorting)
        {
            Sprite[] set = { a, b, c };
            for (int i = 0; i < xs.Length; i++)
            {
                Sprite sprite = set[i % set.Length];
                float s = scale * (0.85f + (i % 4) * 0.08f);
                Color color = Color.Lerp(tone, Color.black, (i % 3) * 0.05f);
                CreateSprite(
                    parent,
                    $"Tree_{i:00}",
                    sprite,
                    new Vector3(xs[i], baseY + (i % 2) * 0.15f, 0f),
                    new Vector3((i % 2 == 0 ? 1f : -1f) * s * 0.55f, s, 1f),
                    color,
                    sorting + (i % 3));
            }
        }

        static void ConfigureLighting()
        {
            // Global frío.
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

            global.color = new Color(0.72f, 0.80f, 0.95f);
            global.intensity = 0.78f;

            // Elimina puntos cálidos previos gestionados.
            DestroyIfExists("WarmLights");
            var root = new GameObject("WarmLights").transform;

            CreateWarmPoint(root, "Warm_Station", new Vector3(4.5f, 1.8f, 0f), new Color(1f, 0.62f, 0.32f), 1.15f, 5.5f);
            CreateWarmPoint(root, "Warm_Platform02", new Vector3(-3.2f, 3.8f, 0f), new Color(1f, 0.7f, 0.4f), 0.7f, 3.5f);
            CreateWarmPoint(root, "Warm_Clearing", new Vector3(-12.5f, 1.2f, 0f), new Color(1f, 0.68f, 0.42f), 0.55f, 3.2f);
            CreateWarmPoint(root, "Warm_Shelf", new Vector3(14f, 1.4f, 0f), new Color(1f, 0.6f, 0.35f), 0.65f, 4f);
        }

        static void CreateWarmPoint(Transform parent, string name, Vector3 pos, Color color, float intensity, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius = radius * 0.15f;
            light.pointLightOuterRadius = radius;
            light.falloffIntensity = 0.55f;
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
            velocity.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);

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
            renderer.sortingOrder = 40;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

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
