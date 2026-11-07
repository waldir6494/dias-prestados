using System.IO;
using DiasPrestados;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using P = DiasPrestados.Editor.ForestAtmosphereArtGenerator.Paths;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Fase 1 "Memoria desvanecida": crea/actualiza ForestAtmosphereDemo.unity a partir de ForestDemo.unity.
    /// ForestDemo.unity y Prototype.unity NO se modifican. No se tocan colliders, transforms de gameplay,
    /// PlayerController2D ni CameraFollow2D: solo sprites/colores, capas visuales, luz, partículas y viñeta.
    /// Menú de generación desactivado: la escena principal es ForestOrganicDemo.
    /// Se conserva el código (helpers de atmósfera); no regenera escenas eliminadas.
    /// </summary>
    public static class ForestAtmosphereSceneBuilder
    {
        public const string SourcePath = "Assets/Scenes/ForestDemo.unity";
        public const string ScenePath = "Assets/Scenes/ForestAtmosphereDemo.unity";

        const string VisualsRootName = "ForestVisuals";
        const string StyleChildName = "_ForestStyle";
        const string VignetteName = "AtmosphereVignette";
        const string LayerBackground = "Background";
        const string LayerMidground = "Midground";
        const string LayerGameplay = "Gameplay";
        const string LayerForeground = "Foreground";
        const string SpriteUnlitPath = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";
        const string ParticleMaterialPath = ForestAtmosphereArtGenerator.ArtFolder + "/Mote_Particle.mat";

        // Ajustes de efectos (editables aquí o en el GameObject de la escena).
        const float VignetteAlpha = 0.35f;
        const float GlobalLightIntensity = 1.0f;

        static readonly Color GlobalLightColor = new Color(0.93f, 0.96f, 1f, 1f);

        // Menú obsoleto (ForestAtmosphereDemo.unity eliminada).

        public static void CreateOrUpdate(bool interactive)
        {
            Debug.LogWarning(
                "[Días Prestados] Generación de ForestAtmosphereDemo desactivada. Usa Assets/Scenes/ForestOrganicDemo.unity.");
            if (interactive && !Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Días Prestados",
                    "ForestAtmosphereDemo ya no forma parte del proyecto.\nAbre ForestOrganicDemo.unity.",
                    "Aceptar");
            }
        }

        /// <summary>Evita diálogos y no guarda por sorpresa ForestDemo.unity.</summary>
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

            Debug.LogError("[Días Prestados] La escena abierta tiene cambios sin guardar. Guárdala y repite " +
                           "(no se guarda automáticamente para no tocar ForestDemo.unity).");
            return false;
        }

        // ------------------------------------------------------------------
        // Aplicación
        // ------------------------------------------------------------------

        static void ApplyAtmosphere(Scene scene)
        {
            Transform cam = FindByName("Main Camera");
            Transform player = FindByName("Player");
            if (cam == null || player == null)
            {
                throw new System.InvalidOperationException(
                    "ForestAtmosphereDemo debe conservar Main Camera y Player.");
            }

            WarnIfSortingLayersMissing();
            DestroyIfExists(VisualsRootName);
            DestroyIfExists("WarmLights");
            DestroyIfExists("AmbientDust");
            DestroyIfExists(VignetteName);

            RestyleGameplaySurfaces();
            BuildVisualLayers(cam);
            ConfigureLighting();
            ConfigureCamera(cam.GetComponent<Camera>());
            BuildMotes(cam);
            BuildVignette(cam);

            EditorSceneManager.MarkSceneDirty(scene);
        }

        static void WarnIfSortingLayersMissing()
        {
            string[] names = { LayerBackground, LayerMidground, LayerGameplay, LayerForeground };
            foreach (string n in names)
            {
                if (SortingLayer.NameToID(n) == 0)
                {
                    Debug.LogWarning(
                        $"[Días Prestados] Sorting Layer '{n}' no existe. Ejecuta Tools > Dias Prestados > Crear / actualizar Forest Demo una vez.");
                }
            }
        }

        /// <summary>
        /// Cambia sprites y tintes de las superficies existentes. No toca colliders, escalas ni posiciones:
        /// los sprites nuevos tienen exactamente los mismos bounds que los de ForestDemo (se comprueba).
        /// </summary>
        static void RestyleGameplaySurfaces()
        {
            Sprite ground = Load(P.Ground);
            Sprite shelf = Load(P.Shelf);
            Sprite platform = Load(P.Platform);
            Sprite platformShadow = Load(P.PlatformShadow);
            Sprite crate = Load(P.Crate);
            Sprite rock = Load(P.RockSolid);
            Sprite bush = Load(P.Bush);

            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
            {
                if (sr == null || sr.transform.root.name == "Player")
                {
                    continue;
                }

                string n = sr.gameObject.name;
                if (n.StartsWith(StyleChildName))
                {
                    continue;
                }

                if (n == "Ground")
                {
                    SwapSprite(sr, ground);
                    ApplySorting(sr, LayerGameplay, 0);
                }
                else if (n == "RaisedShelf")
                {
                    SwapSprite(sr, shelf);
                    ApplySorting(sr, LayerGameplay, 1);
                }
                else if (n.StartsWith("Platform_"))
                {
                    SwapSprite(sr, platform);
                    ApplySorting(sr, LayerGameplay, 2);
                }
                else if (n.StartsWith("Obstacle_Crate"))
                {
                    SwapSprite(sr, crate);
                    ApplySorting(sr, LayerGameplay, 3);
                }
                else if (n.StartsWith("Rock_"))
                {
                    SwapSprite(sr, rock);
                    ApplySorting(sr, LayerGameplay, 4);
                }
            }

            // Quitar decoración de estilo anterior (hierba/musgo/sombras) de los elementos jugables.
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
            {
                if (sr == null)
                {
                    continue;
                }

                string n = sr.gameObject.name;
                if (n == "Ground" || n == "RaisedShelf" || n.StartsWith("Platform_"))
                {
                    ClearStyleChildren(sr.transform);
                }
            }

            // Sombra suave bajo las plataformas: las ancla al suelo visualmente sin cubrir su borde superior.
            foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude))
            {
                if (sr != null && sr.gameObject.name.StartsWith("Platform_"))
                {
                    var col = sr.GetComponent<Collider2D>();
                    float width = col != null ? col.bounds.size.x : 3f;
                    AddSurfaceDecor(sr.transform, platformShadow, new Vector2(0f, -0.72f),
                        new Vector2(width * 1.04f, 0.2f), new Color(1f, 1f, 1f, 0.8f), LayerGameplay, 1);
                }
            }

            // Arbustos del entorno: bruma baja en Midground, bajo contraste, no parecen obstáculos.
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
                    go.transform.localScale = new Vector3(1.3f + (i % 2) * 0.3f, 0.95f + (i % 3) * 0.12f, 1f);
                    var bushSr = go.AddComponent<SpriteRenderer>();
                    bushSr.sprite = bush;
                    bushSr.color = new Color(0.36f, 0.45f, 0.44f, 0.38f);
                    ApplySorting(bushSr, LayerMidground, -15 + (i % 3));
                }
            }
        }

        static void SwapSprite(SpriteRenderer sr, Sprite sprite)
        {
            if (sr == null || sprite == null)
            {
                return;
            }

            if (sr.sprite != null)
            {
                Vector2 oldSize = sr.sprite.bounds.size;
                Vector2 newSize = sprite.bounds.size;
                if ((oldSize - newSize).sqrMagnitude > 0.0001f)
                {
                    Debug.LogWarning(
                        $"[Días Prestados] {sr.name}: el sprite atmosférico mide {newSize} y el actual {oldSize}. " +
                        "Se conserva el actual para no desalinear el collider.");
                    sr.color = Color.white;
                    return;
                }
            }

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.drawMode = SpriteDrawMode.Simple;
        }

        static void BuildVisualLayers(Transform cam)
        {
            Sprite sky = Load(P.Sky);
            Sprite glow = Load(P.HorizonGlow);
            Sprite fog = Load(P.FogBand);
            Sprite ridgeFar = Load(P.RidgeFar);
            Sprite ridgeNear = Load(P.RidgeNear);
            Sprite hills = Load(P.Hills);
            Sprite rails = Load(P.Rails);
            Sprite stationBody = Load(P.StationBody);
            Sprite stationWindows = Load(P.StationWindows);
            Sprite pineA = Load(P.PineA);
            Sprite pineB = Load(P.PineB);
            Sprite broad = Load(P.Broad);
            Sprite trunk = Load(P.TrunkTall);
            Sprite grass = Load(P.Grass);
            Sprite bush = Load(P.Bush);
            Sprite rockDecor = Load(P.RockDecor);

            Color fogCol = AtmospherePalette.Fog;
            Color horizon = AtmospherePalette.Horizon;
            Color green = AtmospherePalette.GreenGray;
            Color slate = AtmospherePalette.Slate;

            var root = new GameObject(VisualsRootName).transform;

            // Cielo frío + resplandor pálido en el horizonte (sin naranja).
            var skyLayer = CreateLayer(root, "SkyCold", cam, 1f, 1f);
            CreateSprite(skyLayer, "Sky", sky, new Vector3(0f, 4.5f, 0f), new Vector3(130f, 1f, 1f),
                Color.white, LayerBackground, -200, unlit: true);
            CreateSprite(skyLayer, "HorizonGlow", glow, new Vector3(5f, 1.4f, 0f), new Vector3(2.8f, 1.5f, 1f),
                AtmospherePalette.WithAlpha(horizon, 0.55f), LayerBackground, -198, unlit: true);

            // Progresión de profundidad: lo lejano se aproxima al color del horizonte; lo cercano, a pizarra.
            var mountainsFar = CreateLayer(root, "MountainsFar", cam, 0.9f, 0.55f);
            Color farTint = Color.Lerp(horizon, fogCol, 0.55f);
            CreateSprite(mountainsFar, "Mountains_Far_A", ridgeFar, new Vector3(-6f, 2.6f, 0f), new Vector3(1.9f, 1.5f, 1f),
                AtmospherePalette.WithAlpha(farTint, 0.9f), LayerBackground, -180, unlit: true);
            CreateSprite(mountainsFar, "Mountains_Far_B", ridgeFar, new Vector3(10f, 2.4f, 0f), new Vector3(-1.7f, 1.4f, 1f),
                AtmospherePalette.WithAlpha(Color.Lerp(farTint, fogCol, 0.15f), 0.9f), LayerBackground, -179, unlit: true);

            var fogA = CreateLayer(root, "FogFar", cam, 0.85f, 0.45f);
            CreateSprite(fogA, "Fog_Far_A", fog, new Vector3(-4f, 1.7f, 0f), new Vector3(5f, 1.8f, 1f),
                AtmospherePalette.WithAlpha(horizon, 0.55f), LayerBackground, -176, unlit: true);
            CreateSprite(fogA, "Fog_Far_B", fog, new Vector3(12f, 1.5f, 0f), new Vector3(4.5f, 1.6f, 1f),
                AtmospherePalette.WithAlpha(horizon, 0.5f), LayerBackground, -176, unlit: true);

            var mountainsNear = CreateLayer(root, "MountainsNear", cam, 0.78f, 0.4f);
            Color nearTint = Color.Lerp(fogCol, green, 0.55f);
            CreateSprite(mountainsNear, "Mountains_Near", ridgeNear, new Vector3(2f, 2.0f, 0f), new Vector3(2.2f, 1.4f, 1f),
                AtmospherePalette.WithAlpha(nearTint, 0.95f), LayerBackground, -170, unlit: true);

            var hillsLayer = CreateLayer(root, "Hills", cam, 0.68f, 0.3f);
            Color hillTint = Color.Lerp(green, slate, 0.25f);
            CreateSprite(hillsLayer, "Hills_A", hills, new Vector3(-10f, 0.9f, 0f), new Vector3(1.5f, 1.15f, 1f),
                hillTint, LayerBackground, -160, unlit: true);
            CreateSprite(hillsLayer, "Hills_B", hills, new Vector3(8f, 0.85f, 0f), new Vector3(1.7f, 1.2f, 1f),
                Color.Lerp(hillTint, slate, 0.12f), LayerBackground, -159, unlit: true);

            var fogB = CreateLayer(root, "FogHills", cam, 0.64f, 0.25f);
            CreateSprite(fogB, "Fog_Hills_A", fog, new Vector3(-8f, 0.7f, 0f), new Vector3(5.5f, 1.4f, 1f),
                AtmospherePalette.WithAlpha(fogCol, 0.5f), LayerBackground, -156, unlit: true);
            CreateSprite(fogB, "Fog_Hills_B", fog, new Vector3(9f, 0.6f, 0f), new Vector3(5f, 1.3f, 1f),
                AtmospherePalette.WithAlpha(fogCol, 0.45f), LayerBackground, -156, unlit: true);

            var railway = CreateLayer(root, "Railway", cam, 0.62f, 0.25f);
            Color railTint = Color.Lerp(green, slate, 0.5f);
            CreateSprite(railway, "Rails_Left", rails, new Vector3(-6f, 0.55f, 0f), new Vector3(1.4f, 1f, 1f),
                railTint, LayerBackground, -150, unlit: true);
            CreateSprite(railway, "Rails_Right", rails, new Vector3(8f, 0.55f, 0f), new Vector3(1.5f, 1f, 1f),
                railTint, LayerBackground, -150, unlit: true);
            CreateSprite(railway, "AbandonedStation", stationBody, new Vector3(4.5f, 1.55f, 0f), new Vector3(1.35f, 1.35f, 1f),
                Color.Lerp(green, slate, 0.55f), LayerBackground, -145, unlit: true);
            // Acento cálido muy localizado: solo dos ventanas.
            CreateSprite(railway, "Station_Windows", stationWindows, new Vector3(4.5f, 1.55f, 0f), new Vector3(1.35f, 1.35f, 1f),
                Color.white, LayerBackground, -144, unlit: true);

            // Árboles: cuanto más cerca, más oscuros y más opacos (tintes sobre siluetas blancas).
            var treesFar = CreateLayer(root, "TreesFar", cam, 0.48f, 0.18f);
            ScatterTrees(treesFar, pineA, pineB, broad,
                new[] { -18f, -14f, -9f, -4f, 1f, 6f, 11f, 16f },
                baseY: 1.4f, scale: 1.5f, tone: AtmospherePalette.WithAlpha(Color.Lerp(fogCol, green, 0.65f), 0.85f),
                sortingLayer: LayerBackground, sorting: -120, unlit: true);

            var fogC = CreateLayer(root, "FogTrees", cam, 0.4f, 0.14f);
            CreateSprite(fogC, "Fog_Trees_A", fog, new Vector3(-6f, 0.9f, 0f), new Vector3(5.5f, 1.8f, 1f),
                AtmospherePalette.WithAlpha(fogCol, 0.4f), LayerBackground, -100, unlit: true);
            CreateSprite(fogC, "Fog_Trees_B", fog, new Vector3(10f, 0.8f, 0f), new Vector3(5f, 1.6f, 1f),
                AtmospherePalette.WithAlpha(fogCol, 0.38f), LayerBackground, -100, unlit: true);

            var treesMid = CreateLayer(root, "TreesMid", cam, 0.3f, 0.1f);
            ScatterTrees(treesMid, pineB, pineA, broad,
                new[] { -16.5f, -12f, -7.5f, -2.5f, 3f, 8.5f, 13.5f, 17.5f },
                baseY: 1.55f, scale: 1.85f, tone: AtmospherePalette.WithAlpha(Color.Lerp(green, slate, 0.35f), 0.9f),
                sortingLayer: LayerMidground, sorting: -40);

            var trunks = CreateLayer(root, "TrunksMid", cam, 0.22f, 0.08f);
            float[] trunkXs = { -14.5f, -9.6f, -5f, 0.2f, 6.2f, 12.2f, 17f };
            for (int i = 0; i < trunkXs.Length; i++)
            {
                // Troncos que salen del encuadre por arriba: sugieren un bosque que no termina.
                float s = 1.9f + (i % 3) * 0.3f;
                CreateSprite(trunks, $"Trunk_{i:00}", trunk, new Vector3(trunkXs[i], 4.2f + (i % 2) * 0.3f, 0f),
                    new Vector3((i % 2 == 0 ? 1f : -1f) * s, s, 1f),
                    AtmospherePalette.WithAlpha(Color.Lerp(green, slate, 0.5f), 0.7f), LayerMidground, -32 + (i % 3));
            }

            var treesNear = CreateLayer(root, "TreesNear", cam, 0.14f, 0.05f);
            ScatterTrees(treesNear, broad, pineA, pineB,
                new[] { -17.5f, -13.2f, -8.8f, -3.8f, 0.8f, 5.8f, 10.8f, 15.2f, 18.2f },
                baseY: 1.7f, scale: 2.15f, tone: AtmospherePalette.WithAlpha(Color.Lerp(green, slate, 0.7f), 0.8f),
                sortingLayer: LayerMidground, sorting: -25);

            // Bruma detrás del plano de juego (nunca delante de las superficies pisables).
            var mist = CreateLayer(root, "GroundMist", cam, 0.1f, 0.02f);
            CreateSprite(mist, "Mist_A", fog, new Vector3(-8f, 0.35f, 0f), new Vector3(6f, 1.1f, 1f),
                AtmospherePalette.WithAlpha(horizon, 0.28f), LayerMidground, -8, unlit: true);
            CreateSprite(mist, "Mist_B", fog, new Vector3(9f, 0.3f, 0f), new Vector3(6f, 1.0f, 1f),
                AtmospherePalette.WithAlpha(horizon, 0.26f), LayerMidground, -8, unlit: true);

            // Primer plano: siluetas pizarra suaves en los bordes; no tapan el camino ni parecen sólidas.
            var foreground = CreateLayer(root, "Foreground", cam, -0.08f, 0.02f);
            Color fgDark = AtmospherePalette.WithAlpha(slate, 0.6f);
            CreateSprite(foreground, "FG_Bush_L", bush, new Vector3(-17.2f, 0.35f, 0f), new Vector3(1.7f, 1.25f, 1f),
                fgDark, LayerForeground, 10);
            CreateSprite(foreground, "FG_Bush_R", bush, new Vector3(17.4f, 0.4f, 0f), new Vector3(1.9f, 1.35f, 1f),
                fgDark, LayerForeground, 10);
            CreateSprite(foreground, "FG_Rock_L", rockDecor, new Vector3(-15.8f, 0.25f, 0f), new Vector3(1.5f, 1.15f, 1f),
                AtmospherePalette.WithAlpha(slate, 0.5f), LayerForeground, 8);
            CreateSprite(foreground, "FG_Rock_R", rockDecor, new Vector3(14.8f, 0.22f, 0f), new Vector3(1.6f, 1.2f, 1f),
                AtmospherePalette.WithAlpha(slate, 0.48f), LayerForeground, 8);
            CreateSprite(foreground, "FG_Grass_A", grass, new Vector3(-10.5f, 0.2f, 0f), new Vector3(1.3f, 1f, 1f),
                AtmospherePalette.WithAlpha(slate, 0.45f), LayerForeground, 6);
            CreateSprite(foreground, "FG_Grass_B", grass, new Vector3(9.2f, 0.18f, 0f), new Vector3(1.4f, 1.05f, 1f),
                AtmospherePalette.WithAlpha(slate, 0.42f), LayerForeground, 6);
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
            int sorting,
            bool unlit = false)
        {
            Sprite[] set = { a, b, c };
            for (int i = 0; i < xs.Length; i++)
            {
                Sprite sprite = set[i % set.Length];
                float s = scale * (0.85f + (i % 4) * 0.08f);
                float shade = (i % 3) * 0.04f;
                Color color = Color.Lerp(tone, AtmospherePalette.Slate, shade);
                color.a = tone.a;
                CreateSprite(
                    parent,
                    $"Tree_{i:00}",
                    sprite,
                    new Vector3(xs[i], baseY + (i % 2) * 0.15f, 0f),
                    new Vector3((i % 2 == 0 ? 1f : -1f) * s * 0.72f, s * 0.9f, 1f),
                    color,
                    sortingLayer,
                    sorting + (i % 3),
                    unlit);
            }
        }

        // ------------------------------------------------------------------
        // Luz, cámara, partículas, viñeta
        // ------------------------------------------------------------------

        static void ConfigureLighting()
        {
            // Ambiente frío, claro y uniforme: la visibilidad no depende de las luces puntuales.
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
            global.color = GlobalLightColor;
            global.intensity = GlobalLightIntensity;
            ApplyLightToAllSortingLayers(global);

            // Acentos cálidos (#E8A84A) muy localizados; sin ellos la escena sigue siendo legible.
            var root = new GameObject("WarmLights").transform;
            Color warm = AtmospherePalette.Warm;
            CreateWarmPoint(root, "Warm_Station", new Vector3(4.5f, 1.8f, 0f), warm, 0.55f, 4.2f);
            CreateWarmPoint(root, "Warm_Platform02", new Vector3(-3.2f, 3.8f, 0f), warm, 0.22f, 2.8f);
            CreateWarmPoint(root, "Warm_Clearing", new Vector3(-12.5f, 1.2f, 0f), warm, 0.2f, 3f);
            CreateWarmPoint(root, "Warm_Shelf", new Vector3(14f, 1.4f, 0f), warm, 0.24f, 3f);
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
            light.pointLightInnerRadius = radius * 0.15f;
            light.pointLightOuterRadius = radius;
            light.falloffIntensity = 0.75f;
            ApplyLightToAllSortingLayers(light);
        }

        static void ConfigureCamera(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = AtmospherePalette.GreenGray;
        }

        static void BuildMotes(Transform cam)
        {
            var go = new GameObject("AmbientDust");
            go.transform.position = cam.position + new Vector3(0f, 1.2f, 5f);
            var follow = go.AddComponent<AmbientDustParticles>();
            follow.Configure(cam, new Vector3(0f, 1.2f, 5f));

            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = go.AddComponent<ParticleSystem>();
            }

            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(8f, 14f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.14f);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(0.73f, 0.70f, 0.63f, 0.28f),
                new Color(0.91f, 0.66f, 0.29f, 0.16f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 70;
            main.gravityModifier = 0f;

            var emission = ps.emission;
            emission.rateOverTime = 4f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(16f, 8f, 1f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            // Todos los ejes en TwoConstants (Unity exige el mismo modo en cada grupo).
            velocity.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
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
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(0.7f, 0.75f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = g;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sortingLayerName = LayerForeground;
            renderer.sortingOrder = 5;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material mat = EnsureMoteMaterial();
            if (mat != null)
            {
                renderer.sharedMaterial = mat;
            }
            else
            {
                Debug.LogWarning("[Días Prestados] No se pudo crear el material de motas; pueden verse magenta.");
            }

            var sheet = ps.textureSheetAnimation;
            sheet.enabled = false;
            var collision = ps.collision;
            collision.enabled = false;
            var trigger = ps.trigger;
            trigger.enabled = false;
        }

        /// <summary>Material translúcido (Sprite-Unlit) con textura de mota suave: sin cuadrados ni magenta.</summary>
        static Material EnsureMoteMaterial()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(ParticleMaterialPath);
            if (mat != null)
            {
                return mat;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                Material unlit = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitPath);
                shader = unlit != null ? unlit.shader : null;
            }

            if (shader == null)
            {
                return null;
            }

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(P.Mote);
            mat = new Material(shader) { name = "Mote_Particle" };
            if (tex != null)
            {
                mat.mainTexture = tex;
            }

            AssetDatabase.CreateAsset(mat, ParticleMaterialPath);
            return mat;
        }

        /// <summary>Viñeta mínima como sprite hijo de la cámara. Ajusta el alfa del SpriteRenderer o desactiva el objeto.</summary>
        static void BuildVignette(Transform cam)
        {
            Sprite vignette = Load(P.Vignette);
            if (vignette == null || cam == null)
            {
                return;
            }

            var go = new GameObject(VignetteName);
            go.transform.SetParent(cam, false);
            go.transform.localPosition = new Vector3(0f, 0f, 10f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = vignette;
            sr.color = new Color(1f, 1f, 1f, VignetteAlpha);
            // Se ajusta al aspecto real de la cámara (Free Aspect, 16:9, ultra-wide...).
            var fit = go.AddComponent<AtmosphereVignette>();
            fit.Configure(cam.GetComponent<Camera>(), VignetteAlpha);
            ApplySorting(sr, LayerForeground, 50);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitPath);
            if (mat != null)
            {
                sr.sharedMaterial = mat;
            }
        }

        // ------------------------------------------------------------------
        // Utilidades
        // ------------------------------------------------------------------

        static Sprite Load(string path)
        {
            return ProceduralSpriteUtil.LoadSprite(path);
        }

        static void ApplySorting(SpriteRenderer sr, string layerName, int order)
        {
            if (sr == null)
            {
                return;
            }

            sr.sortingLayerName = layerName;
            int id = SortingLayer.NameToID(layerName);
            if (id != 0)
            {
                sr.sortingLayerID = id;
            }

            sr.sortingOrder = order;
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
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(SpriteUnlitPath);
                if (mat != null)
                {
                    sr.sharedMaterial = mat;
                }
            }
        }

        /// <summary>Decoración posicionada en fracciones del collider. No añade Collider2D.</summary>
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

        static Transform FindByName(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Exclude))
            {
                if (t.name == name && t.parent == null)
                {
                    return t;
                }
            }

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

    /// <summary>
    /// Ejecuta la generación una sola vez si existe Temp/BuildForestAtmosphere.flag (sale de Play si hace falta).
    /// </summary>
    [InitializeOnLoad]
    static class ForestAtmosphereAutoBuildOnce
    {
        const string FlagPath = "Temp/BuildForestAtmosphere.flag";
        static bool waitingForEditMode;

        static ForestAtmosphereAutoBuildOnce()
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
                if (!waitingForEditMode)
                {
                    waitingForEditMode = true;
                    EditorApplication.playModeStateChanged += OnPlayModeChanged;
                    Debug.Log("[Días Prestados] Atmosphere auto-build: saliendo de Play Mode...");
                    EditorApplication.ExitPlaymode();
                }

                return;
            }

            waitingForEditMode = false;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;

            try
            {
                File.Delete(FlagPath);
            }
            catch
            {
                return;
            }

            // Obsoleto: ForestAtmosphereDemo.unity ya no existe. No regenerar.
            Debug.LogWarning(
                "[Días Prestados] Atmosphere auto-build desactivado. Usa Assets/Scenes/ForestOrganicDemo.unity.");
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                waitingForEditMode = false;
                EditorApplication.delayCall += TryRun;
            }
        }
    }
}
