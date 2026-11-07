using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using DiasPrestados.Companions;
using DiasPrestados.Environment;
using DiasPrestados.Interaction;
using DiasPrestados.UI;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Construye la escena independiente de la demo (Hito 1: primera prueba jugable).
    /// Solo escribe Assets/Scenes/DiasPrestadosDemo.unity: nunca toca ForestOrganicDemo ni otras escenas.
    /// </summary>
    public static class DemoSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/DiasPrestadosDemo.unity";
        const string InputPath = "Assets/Input/PrototypeControls.inputactions";
        const string ProtagonistFolder = "Assets/Art/Characters/Protagonist";
        const string LayerBackground = "Background";
        const string LayerMidground = "Midground";
        const string LayerGameplay = "Gameplay";
        const string LayerForeground = "Foreground";

        const float SpawnX = 6f;
        const float CameraOrthoSize = 5.2f;
        const float MaxCameraX = 197f;
        const float PlayerHalfHeight = 0.95f;
        const float BrotherHalfHeight = 0.75f;

        static Transform cameraTransform;
        static Vector2 cameraStart;
        static Material litMaterial;
        static Material unlitMaterial;
        static int companionLayer;
        static PhysicsMaterial2D frictionless;

        [MenuItem("Tools/Dias Prestados/Demo/Generar arte del Hito 1")]
        public static void GenerateArtMenu()
        {
            GenerateArt();
        }

        [MenuItem("Tools/Dias Prestados/Demo/Construir escena Hito 1 (DiasPrestadosDemo)")]
        public static void BuildMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (!System.IO.File.Exists(ProceduralSpriteUtil.ToAbsolutePath(DemoEnvironmentArt.P("terrain_main"))))
            {
                GenerateArt();
            }

            Build();
        }

        [MenuItem("Tools/Dias Prestados/Demo/Abrir DiasPrestadosDemo")]
        public static void OpenMenu()
        {
            if (!System.IO.File.Exists(ProceduralSpriteUtil.ToAbsolutePath(ScenePath)))
            {
                Debug.LogError($"[Días Prestados] No existe {ScenePath}. Ejecuta primero 'Construir escena Hito 1'.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }

        /// <summary>Punto de entrada para batchmode: -executeMethod DiasPrestados.Editor.Demo.DemoSceneBuilder.BuildBatch</summary>
        public static void BuildBatch()
        {
            GenerateArt();
            Build();
        }

        public static void GenerateArt()
        {
            DemoEnvironmentArt.Generate();
            DemoBrotherArt.Generate();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Días Prestados] Arte del Hito 1 generado en Assets/Art/Demo y Assets/Art/Characters/LittleBrother.");
        }

        // ------------------------------------------------------------------
        // Escena
        // ------------------------------------------------------------------

        public static void Build()
        {
            companionLayer = EnsureLayer("Companion");
            litMaterial = FindMaterial("Sprite-Lit-Default");
            unlitMaterial = FindMaterial("Sprite-Unlit-Default");
            frictionless = EnsureFrictionless();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            float spawnY = DemoTerrain.Height(SpawnX) + PlayerHalfHeight + 0.05f;
            cameraStart = new Vector2(SpawnX, Mathf.Max(spawnY + 1.6f, -3.5f + CameraOrthoSize));

            Camera cam = BuildCamera();
            BuildLighting();
            Transform world = new GameObject("World").transform;
            BuildBackdrop(world);
            BuildTerrain(world);
            BuildProps(world);
            GameObject player = BuildPlayer(spawnY, out InteractionPrompt prompt);
            BuildBrother(player.transform, out CompanionFollower follower);
            BuildGameplayExtras(world, player.transform);
            BuildParticles();
            BuildHud(player.transform);
            WireCamera(cam, player.transform);

            string folder = System.IO.Path.GetDirectoryName(ScenePath).Replace('\\', '/');
            ProceduralSpriteUtil.EnsureFolder(folder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Días Prestados] Escena creada: {ScenePath}");
        }

        // ------------------------------------------------------------------
        // Cámara, luz
        // ------------------------------------------------------------------

        static Camera BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(cameraStart.x, cameraStart.y, -10f);
            var cam = go.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = CameraOrthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.28f, 0.34f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 50f;
            go.AddComponent<UniversalAdditionalCameraData>();
            go.AddComponent<AudioListener>();
            go.AddComponent<CameraFollow2D>();
            cameraTransform = go.transform;

            Sprite vignette = LoadSprite("Assets/Art/ForestAtmosphere/vignette_soft.png");
            if (vignette != null)
            {
                var v = new GameObject("AtmosphereVignette");
                v.transform.SetParent(go.transform, false);
                v.transform.localPosition = new Vector3(0f, 0f, 10f);
                var sr = v.AddComponent<SpriteRenderer>();
                sr.sprite = vignette;
                sr.sharedMaterial = unlitMaterial;
                sr.color = new Color(0.9f, 0.95f, 1f, 0.55f);
                Sort(sr, LayerForeground, 50);
                v.AddComponent<AtmosphereVignette>().Configure(cam, 0.55f);
            }

            return cam;
        }

        static void WireCamera(Camera cam, Transform target)
        {
            var follow = cam.GetComponent<CameraFollow2D>();
            var so = new SerializedObject(follow);
            so.FindProperty("target").objectReferenceValue = target;
            so.FindProperty("offset").vector2Value = new Vector2(0f, 1.6f);
            so.FindProperty("smoothTime").floatValue = 0.22f;
            so.FindProperty("minBounds").vector2Value = new Vector2(-6f, -3.5f);
            so.FindProperty("maxBounds").vector2Value = new Vector2(206f, 40f);
            so.FindProperty("useBounds").boolValue = true;
            so.FindProperty("lookAheadDistance").floatValue = 1.6f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildLighting()
        {
            var g = new GameObject("Global Light 2D");
            var global = g.AddComponent<Light2D>();
            global.lightType = Light2D.LightType.Global;
            global.color = new Color(0.86f, 0.93f, 1f);
            global.intensity = 1f;
            AllSortingLayers(global);

            var root = new GameObject("WarmLights").transform;
            float[] xs = { 20f, 62f, 104f, 134f, 176f };
            foreach (float x in xs)
            {
                CreatePointLight(root, $"Sun_Patch_{x:0}", new Vector3(x, DemoTerrain.Height(x) + 3.2f, 0f),
                    new Color(1f, 0.80f, 0.50f), 0.55f, 8f);
            }
        }

        static Light2D CreatePointLight(Transform parent, string name, Vector3 pos, Color color, float intensity, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.pointLightInnerRadius = radius * 0.1f;
            l.pointLightOuterRadius = radius;
            l.falloffIntensity = 0.7f;
            AllSortingLayers(l);
            return l;
        }

        static void AllSortingLayers(Light2D light)
        {
            SortingLayer[] layers = SortingLayer.layers;
            var ids = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++)
            {
                ids[i] = layers[i].id;
            }

            light.targetSortingLayers = ids;
            light.shadowsEnabled = false;
        }

        // ------------------------------------------------------------------
        // Fondo con parallax
        // ------------------------------------------------------------------

        static float LayerX(float factor, float cameraX)
        {
            return cameraStart.x + (cameraX - cameraStart.x) * (1f - factor);
        }

        static Transform MakeLayer(Transform parent, string name, float factor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<ParallaxLayer>().Configure(cameraTransform, factor, factor);
            return go.transform;
        }

        static void BuildBackdrop(Transform world)
        {
            Transform back = new GameObject("Backdrop").transform;
            back.SetParent(world, false);
            var rng = new System.Random(2026);

            Sprite sky = LoadSprite(DemoEnvironmentArt.P("sky"));
            Sprite fog = LoadSprite(DemoEnvironmentArt.P("fog"));
            Sprite shaft = LoadSprite(DemoEnvironmentArt.P("sun_shaft"));
            Sprite glow = LoadSprite("Assets/Art/ForestAtmosphere/glow_soft.png");

            Sprite[] trees =
            {
                LoadSprite(DemoEnvironmentArt.P("tree_pine_a")), LoadSprite(DemoEnvironmentArt.P("tree_pine_b")),
                LoadSprite(DemoEnvironmentArt.P("tree_pine_c")), LoadSprite(DemoEnvironmentArt.P("tree_broad_a")),
                LoadSprite(DemoEnvironmentArt.P("tree_broad_b"))
            };

            // Cielo anclado a la cámara
            Transform skyLayer = MakeLayer(back, "Sky", 1f);
            var skySr = Spr(skyLayer, "SkyGradient", sky, new Vector2(cameraStart.x, cameraStart.y), LayerBackground, -100, Color.white, 1f, false);
            Vector2 skySize = sky.bounds.size;
            skySr.transform.localScale = new Vector3(80f / skySize.x, 30f / skySize.y, 1f);

            Transform glowLayer = MakeLayer(back, "SunGlow", 0.97f);
            if (glow != null)
            {
                float s = 20f / glow.bounds.size.x;
                Spr(glowLayer, "SunGlow", glow, new Vector2(cameraStart.x + 7f, cameraStart.y + 3.2f), LayerBackground, -98,
                    new Color(1f, 0.92f, 0.72f, 0.55f), s, false);
            }

            // Crestas lejanas (teselables)
            MakeRidge(back, "RidgeFar", "ridge_far", 0.92f, -1.0f, new Color(1f, 1f, 1f, 0.9f), -90);
            MakeRidge(back, "RidgeNear", "ridge_near", 0.85f, -2.6f, new Color(1f, 1f, 1f, 0.95f), -88);

            // Capas de árboles (de lejos a cerca)
            MakeForest(back, "Trees_Far", 0.75f, 1.4f, 2.4f, 8f, 10.5f, 0.4f, new Color(0.62f, 0.78f, 0.86f, 0.55f), trees, LayerBackground, -80, rng);
            MakeFog(back, "Fog_A", 0.72f, fog, 5f, 0.6f, 14f, 0.45f, LayerBackground, -75, rng);
            MakeForest(back, "Trees_MidFar", 0.55f, 2.2f, 3.4f, 10f, 13f, -0.4f, new Color(0.72f, 0.86f, 0.92f, 0.72f), trees, LayerBackground, -70, rng);
            MakeFog(back, "Fog_B", 0.52f, fog, 5f, 0.2f, 12f, 0.4f, LayerBackground, -62, rng);
            MakeForest(back, "Trees_Mid", 0.35f, 3.5f, 5.5f, 12f, 15f, -1.0f, new Color(0.85f, 0.95f, 1f, 0.9f), trees, LayerMidground, -40, rng);
            MakeFog(back, "Fog_C", 0.32f, fog, 6f, -0.6f, 11f, 0.35f, LayerMidground, -30, rng);

            // Suelo trasero oscuro (evita ver el cielo por el barranco)
            Transform floorLayer = MakeLayer(back, "BackFloor", 0.2f);
            SolidRect(floorLayer, "BackFloor", new Color(0.07f, 0.13f, 0.15f, 1f),
                new Vector2(cameraStart.x + 90f, -9f), new Vector2(400f, 16f), LayerMidground, -15);

            MakeForest(back, "Trees_Near", 0.18f, 5f, 8.5f, 14f, 18f, -1.6f, new Color(0.78f, 0.9f, 0.95f, 1f), trees, LayerMidground, -10, rng);

            // Haces de luz
            Transform shaftLayer = MakeLayer(back, "SunShafts", 0.12f);
            float spanMax = LayerX(0.12f, MaxCameraX) + 20f;
            for (float x = cameraStart.x - 18f; x < spanMax; x += 9f + (float)rng.NextDouble() * 8f)
            {
                float width = 1.8f + (float)rng.NextDouble() * 2.2f;
                var sr = Spr(shaftLayer, "Shaft", shaft, new Vector2(x, 4.5f), LayerMidground, -5,
                    new Color(1f, 0.93f, 0.74f, 0.10f + (float)rng.NextDouble() * 0.08f), 1f, false);
                sr.transform.localScale = new Vector3(width / shaft.bounds.size.x, 1.7f, 1f);
                sr.transform.rotation = Quaternion.Euler(0f, 0f, -17f);
            }

            // Estación lejana, hacia el final del recorrido
            Sprite station = LoadSprite("Assets/Art/ForestAtmosphere/station_silhouette.png");
            Sprite windows = LoadSprite("Assets/Art/ForestAtmosphere/station_windows.png");
            if (station != null)
            {
                Transform st = MakeLayer(back, "StationFar", 0.3f);
                float sx = LayerX(0.3f, 195f);
                float scale = 20f / station.bounds.size.x;
                Spr(st, "Station", station, new Vector2(sx, 1.8f), LayerMidground, -35, new Color(0.55f, 0.66f, 0.74f, 0.92f), scale, false);
                if (windows != null)
                {
                    Spr(st, "StationWindows", windows, new Vector2(sx, 1.8f), LayerMidground, -34, new Color(1f, 0.78f, 0.45f, 0.85f), scale, false);
                }

                if (glow != null)
                {
                    float gs = 11f / glow.bounds.size.x;
                    Spr(st, "StationGlow", glow, new Vector2(sx, 2.4f), LayerMidground, -36, new Color(1f, 0.72f, 0.38f, 0.38f), gs, false);
                }
            }

            // Niebla frontal muy suave
            MakeFog(back, "Fog_Front", -0.08f, fog, 7f, -0.3f, 24f, 0.22f, LayerForeground, 20, rng);

            // Troncos en primer plano
            Transform fg = MakeLayer(back, "ForegroundTrunks", -0.18f);
            Sprite trunkA = LoadSprite(DemoEnvironmentArt.P("trunk_fg_a"));
            Sprite trunkB = LoadSprite(DemoEnvironmentArt.P("trunk_fg_b"));
            float[] trunkCam = { 24f, 71f, 112f, 150f, 186f };
            for (int i = 0; i < trunkCam.Length; i++)
            {
                float x = cameraStart.x + (trunkCam[i] - cameraStart.x) * (1f + 0.18f) + (i % 2 == 0 ? -7.5f : 7.5f);
                Spr(fg, "FG_Trunk", i % 2 == 0 ? trunkA : trunkB, new Vector2(x, 4.5f), LayerForeground, 5,
                    new Color(0.7f, 0.82f, 0.9f, 0.97f), 1.15f, false);
            }
        }

        static void MakeRidge(Transform parent, string name, string spriteName, float factor, float bottomY, Color color, int order)
        {
            Sprite ridge = LoadSprite(DemoEnvironmentArt.P(spriteName));
            Transform layer = MakeLayer(parent, name, factor);
            float min = cameraStart.x - 40f;
            float max = LayerX(factor, MaxCameraX) + 40f;
            var go = new GameObject(name + "_Tiled");
            go.transform.SetParent(layer, false);
            go.transform.position = new Vector3((min + max) * 0.5f, bottomY + ridge.bounds.size.y * 0.5f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ridge;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.tileMode = SpriteTileMode.Continuous;
            sr.size = new Vector2(max - min, ridge.bounds.size.y);
            sr.color = color;
            sr.sharedMaterial = unlitMaterial;
            Sort(sr, LayerBackground, order);
        }

        static void MakeForest(Transform parent, string name, float factor, float minGap, float maxGap, float hMin, float hMax,
            float baseY, Color tint, Sprite[] trees, string sortLayer, int order, System.Random rng)
        {
            Transform layer = MakeLayer(parent, name, factor);
            float spanMin = cameraStart.x - 26f;
            float spanMax = LayerX(factor, MaxCameraX) + 26f;
            int i = 0;
            for (float x = spanMin; x < spanMax; x += minGap + (float)rng.NextDouble() * (maxGap - minGap))
            {
                double pick = rng.NextDouble();
                Sprite s = pick < 0.28 ? trees[0] : pick < 0.50 ? trees[1] : pick < 0.70 ? trees[2] : pick < 0.85 ? trees[3] : trees[4];
                float height = hMin + (float)rng.NextDouble() * (hMax - hMin);
                float scale = height / s.bounds.size.y;
                float tone = 0.88f + (float)rng.NextDouble() * 0.22f;
                Color c = new Color(Mathf.Clamp01(tint.r * tone), Mathf.Clamp01(tint.g * tone), Mathf.Clamp01(tint.b * tone), tint.a);
                var sr = Spr(layer, "Tree", s, new Vector2(x, baseY + height * 0.5f), sortLayer, order + (i % 5), c, scale, false);
                sr.flipX = rng.NextDouble() < 0.5;
                i++;
            }
        }

        static void MakeFog(Transform parent, string name, float factor, Sprite fog, float gapMin, float y, float width, float alpha,
            string sortLayer, int order, System.Random rng)
        {
            Transform layer = MakeLayer(parent, name, factor);
            float spanMin = cameraStart.x - 26f;
            float spanMax = LayerX(factor, MaxCameraX) + 26f;
            for (float x = spanMin; x < spanMax; x += gapMin + (float)rng.NextDouble() * gapMin * 2f)
            {
                float w = width * (0.7f + (float)rng.NextDouble() * 0.6f);
                var sr = Spr(layer, "Fog", fog, new Vector2(x, y + (float)rng.NextDouble() * 1.2f), sortLayer, order, new Color(1f, 1f, 1f, alpha), 1f, false);
                sr.transform.localScale = new Vector3(w / fog.bounds.size.x, 1.6f, 1f);
            }
        }

        // ------------------------------------------------------------------
        // Terreno y props
        // ------------------------------------------------------------------

        static void BuildTerrain(Transform world)
        {
            var go = new GameObject("Terrain_Main");
            go.transform.SetParent(world, false);
            Sprite terrain = LoadSprite(DemoEnvironmentArt.P("terrain_main"));
            go.transform.position = new Vector3((DemoTerrain.MinX + DemoTerrain.MaxX) * 0.5f, (DemoTerrain.BottomY + DemoTerrain.TopY) * 0.5f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = terrain;
            sr.sharedMaterial = litMaterial;
            Sort(sr, LayerGameplay, 0);

            // Collider independiente del sprite: mismo perfil que la textura.
            var colGo = new GameObject("Terrain_Collider");
            colGo.transform.SetParent(world, false);
            var pc = colGo.AddComponent<PolygonCollider2D>();
            pc.sharedMaterial = frictionless;
            pc.pathCount = 1;
            pc.SetPath(0, DemoTerrain.CollisionPolygon());

            CreateWall(world, "Wall_Left", DemoTerrain.MinX + 1.5f);
            CreateWall(world, "Wall_Right", DemoTerrain.MaxX - 1.5f);
        }

        static void CreateWall(Transform parent, string name, float x)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, 8f, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 40f);
            box.sharedMaterial = frictionless;
        }

        static void BuildProps(Transform world)
        {
            var props = new GameObject("Props").transform;
            props.SetParent(world, false);
            var rng = new System.Random(77);

            Sprite[] rocks =
            {
                LoadSprite(DemoEnvironmentArt.P("rock_a")), LoadSprite(DemoEnvironmentArt.P("rock_b")), LoadSprite(DemoEnvironmentArt.P("rock_c"))
            };
            Sprite fern = LoadSprite(DemoEnvironmentArt.P("fern"));
            Sprite tuft = LoadSprite(DemoEnvironmentArt.P("grass_tuft"));
            Sprite bush = LoadSprite(DemoEnvironmentArt.P("bush"));
            Sprite log = LoadSprite(DemoEnvironmentArt.P("log_fallen"));

            // Tronco caído sólido sobre la meseta (obstáculo bajo que el pequeño salta solo)
            float logX = 49f;
            float logY = DemoTerrain.Height(logX) + 0.46f;
            var logGo = Spr(props, "Log_Fallen_Solid", log, new Vector2(logX, logY), LayerGameplay, 4, Color.white, 1f, true).gameObject;
            var logCol = logGo.AddComponent<BoxCollider2D>();
            logCol.size = new Vector2(5.0f, 0.78f);
            logCol.offset = new Vector2(0f, -0.04f);
            logCol.sharedMaterial = frictionless;

            // Rocas decorativas (sin collider)
            float[] rockX = { 14f, 27f, 44f, 66f, 78f, 92f, 104f, 112f, 132f, 152f, 168f, 186f };
            for (int i = 0; i < rockX.Length; i++)
            {
                float x = rockX[i];
                float scale = (i == 7 ? 0.95f : 0.45f + (float)rng.NextDouble() * 0.3f);
                Sprite r = i == 7 ? rocks[0] : rocks[rng.Next(0, 3)];
                var sr = Spr(props, "Rock_Decor", r, new Vector2(x, DemoTerrain.Height(x) - 0.12f), LayerGameplay, 2, Color.white, scale, true);
                sr.flipX = rng.NextDouble() < 0.5;
            }

            // Arbustos tras los personajes, helechos y matas delante
            for (float x = 6f; x < 202f; x += 6f + (float)rng.NextDouble() * 7f)
            {
                if (DemoTerrain.InRavine(x) || DemoTerrain.InRavine(x - 2f) || DemoTerrain.InRavine(x + 2f) || Mathf.Abs(x - DemoTerrain.CliffX) < 2f)
                {
                    continue;
                }

                float scale = 0.8f + (float)rng.NextDouble() * 0.5f;
                Spr(props, "Bush", bush, new Vector2(x, DemoTerrain.Height(x) - 0.1f), LayerGameplay, -2,
                    new Color(0.85f, 0.95f, 1f, 1f), scale, true).flipX = rng.NextDouble() < 0.5;
            }

            for (float x = 3f; x < 204f; x += 4f + (float)rng.NextDouble() * 6f)
            {
                if (DemoTerrain.InRavine(x) || DemoTerrain.InRavine(x - 1f) || DemoTerrain.InRavine(x + 1f))
                {
                    continue;
                }

                bool isFern = rng.NextDouble() < 0.55;
                float scale = isFern ? 0.7f + (float)rng.NextDouble() * 0.5f : 0.6f + (float)rng.NextDouble() * 0.4f;
                float y = DemoTerrain.Height(x) - 0.05f;
                bool front = rng.NextDouble() < 0.35;
                var sr = Spr(props, isFern ? "Fern" : "GrassTuft", isFern ? fern : tuft, new Vector2(x, y),
                    front ? LayerForeground : LayerGameplay, front ? 3 : -1, front ? new Color(0.75f, 0.88f, 0.92f, 1f) : Color.white, scale, true);
                sr.flipX = rng.NextDouble() < 0.5;
            }

            // Vías oxidadas hacia el final (pista visual de la estación)
            Sprite rail = LoadSprite("Assets/Art/ForestOrganic/rail_track_ground.png");
            if (rail != null)
            {
                float scale = 14f / rail.bounds.size.x;
                for (float x = 172f; x < 206f; x += 13.6f)
                {
                    Spr(props, "Rails", rail, new Vector2(x, DemoTerrain.Height(x) + 0.03f), LayerGameplay, 1, new Color(0.8f, 0.78f, 0.74f, 1f), scale, true);
                }
            }
        }

        // ------------------------------------------------------------------
        // Personajes
        // ------------------------------------------------------------------

        static GameObject BuildPlayer(float spawnY, out InteractionPrompt prompt)
        {
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var go = new GameObject("Protagonist_HermanoMayor") { tag = "Player" };
            go.transform.position = new Vector3(SpawnX, spawnY, 0f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.7f, PlayerHalfHeight * 2f);
            col.sharedMaterial = frictionless;

            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform, false);
            gc.transform.localPosition = new Vector3(0f, -PlayerHalfHeight, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, -PlayerHalfHeight, 0f);
            visual.transform.localScale = new Vector3(0.76f, 0.76f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite($"{ProtagonistFolder}/Walk/walk_01.png");
            sr.sharedMaterial = litMaterial;
            Sort(sr, LayerGameplay, 12);
            var animator = visual.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{ProtagonistFolder}/Animations/Player.controller");

            var controller = go.AddComponent<PlayerController2D>();
            var so = new SerializedObject(controller);
            so.FindProperty("inputActions").objectReferenceValue = inputAsset;
            so.FindProperty("groundCheck").objectReferenceValue = gc.transform;
            so.FindProperty("groundLayers").FindPropertyRelative("m_Bits").intValue = 0x7FFFFFFF & ~(1 << companionLayer);
            so.FindProperty("holdOnSlopesWhenIdle").boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<PlayerProtagonistAnimator>();
            go.AddComponent<PlayerFallRecovery>();

            // Indicador de interacción
            var promptGo = new GameObject("InteractionPrompt");
            var psr = promptGo.AddComponent<SpriteRenderer>();
            psr.sprite = LoadSprite(DemoEnvironmentArt.P("prompt_key_e"));
            psr.sharedMaterial = unlitMaterial;
            psr.color = new Color(1f, 1f, 1f, 0f);
            Sort(psr, LayerGameplay, 30);
            promptGo.transform.localScale = Vector3.one * 0.75f;
            prompt = promptGo.AddComponent<InteractionPrompt>();

            var sensor = go.AddComponent<InteractionSensor>();
            var sso = new SerializedObject(sensor);
            sso.FindProperty("inputActions").objectReferenceValue = inputAsset;
            sso.FindProperty("prompt").objectReferenceValue = prompt;
            sso.ApplyModifiedPropertiesWithoutUndo();

            return go;
        }

        static void BuildBrother(Transform leader, out CompanionFollower follower)
        {
            float x = SpawnX - 2.4f;
            var go = new GameObject("HermanoPequeno") { layer = companionLayer };
            go.transform.position = new Vector3(x, DemoTerrain.Height(x) + BrotherHalfHeight + 0.05f, 0f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.5f, BrotherHalfHeight * 2f);
            col.sharedMaterial = frictionless;

            var gc = new GameObject("GroundCheck");
            gc.transform.SetParent(go.transform, false);
            gc.transform.localPosition = new Vector3(0f, -BrotherHalfHeight, 0f);
            gc.layer = companionLayer;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.localPosition = new Vector3(0f, -BrotherHalfHeight, 0f);
            visual.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(DemoBrotherArt.FramePath("idle_0"));
            sr.sharedMaterial = litMaterial;
            Sort(sr, LayerGameplay, 10);

            var anchor = new GameObject("PromptAnchor");
            anchor.transform.SetParent(go.transform, false);
            anchor.transform.localPosition = new Vector3(0f, BrotherHalfHeight + 0.55f, 0f);

            follower = go.AddComponent<CompanionFollower>();
            var fso = new SerializedObject(follower);
            fso.FindProperty("leader").objectReferenceValue = leader;
            fso.FindProperty("groundCheck").objectReferenceValue = gc.transform;
            fso.FindProperty("groundLayers").FindPropertyRelative("m_Bits").intValue = 0x7FFFFFFF & ~(1 << companionLayer);
            fso.ApplyModifiedPropertiesWithoutUndo();

            var anim = go.AddComponent<CompanionSpriteAnimator>();
            var aso = new SerializedObject(anim);
            aso.FindProperty("follower").objectReferenceValue = follower;
            aso.FindProperty("spriteRenderer").objectReferenceValue = sr;
            aso.FindProperty("visualScale").floatValue = 0.7f;
            SetSprites(aso, "idle", "idle_0", "idle_1");
            SetSprites(aso, "walk", "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5");
            SetSprites(aso, "jump", "jump");
            SetSprites(aso, "fall", "fall");
            SetSprites(aso, "ask", "ask_0", "ask_1");
            SetSprites(aso, "cheer", "cheer_0", "cheer_1");
            aso.ApplyModifiedPropertiesWithoutUndo();

            var interact = go.AddComponent<CompanionBoostInteractable>();
            var iso = new SerializedObject(interact);
            iso.FindProperty("promptAnchor").objectReferenceValue = anchor.transform;
            iso.FindProperty("interactionRadius").floatValue = 3.6f;
            iso.ApplyModifiedPropertiesWithoutUndo();

            // Destello cálido al recibir ayuda
            Light2D glow = CreatePointLight(go.transform, "JoyGlow", go.transform.position + Vector3.up * 0.3f, new Color(1f, 0.82f, 0.5f), 0f, 4f);
            var joy = go.AddComponent<CompanionJoyGlow>();
            var jso = new SerializedObject(joy);
            jso.FindProperty("follower").objectReferenceValue = follower;
            jso.FindProperty("glow").objectReferenceValue = glow;
            jso.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetSprites(SerializedObject so, string property, params string[] frames)
        {
            SerializedProperty arr = so.FindProperty(property);
            arr.arraySize = frames.Length;
            for (int i = 0; i < frames.Length; i++)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = LoadSprite(DemoBrotherArt.FramePath(frames[i]));
            }
        }

        static void BuildGameplayExtras(Transform world, Transform player)
        {
            // Zona de ayuda al pie del risco: el pequeño no llega y pide ayuda al hermano mayor.
            float cx = DemoTerrain.CliffX - 2.4f;
            var zoneGo = new GameObject("HelpZone_Cliff");
            zoneGo.transform.SetParent(world, false);
            zoneGo.transform.position = new Vector3(cx, DemoTerrain.Height(cx) + 1.1f, 0f);
            var landing = new GameObject("Landing");
            landing.transform.SetParent(zoneGo.transform, false);
            float lx = DemoTerrain.CliffX + 1.7f;
            landing.transform.position = new Vector3(lx, DemoTerrain.Height(lx), 0f);

            var zone = zoneGo.AddComponent<CompanionHelpZone>();
            var zso = new SerializedObject(zone);
            zso.FindProperty("size").vector2Value = new Vector2(6f, 3.4f);
            zso.FindProperty("landingPoint").objectReferenceValue = landing.transform;
            zso.ApplyModifiedPropertiesWithoutUndo();

            // Mensaje de fin de prueba
            var end = new GameObject("EndMessageZone");
            end.transform.SetParent(world, false);
            var msg = end.AddComponent<DemoMessageZone>();
            var mso = new SerializedObject(msg);
            mso.FindProperty("target").objectReferenceValue = player;
            mso.FindProperty("triggerX").floatValue = 190f;
            mso.FindProperty("message").stringValue = "Fin de la primera prueba  ·  Hito 1";
            mso.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildHud(Transform player)
        {
            new GameObject("DemoHud").AddComponent<DemoHudOverlay>();
        }

        // ------------------------------------------------------------------
        // Partículas
        // ------------------------------------------------------------------

        static void BuildParticles()
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/ForestAtmosphere/Mote_Particle.mat");
            MakeParticles("AmbientDust", mat, new Color(0.80f, 0.84f, 0.80f, 0.30f), new Color(0.95f, 0.85f, 0.62f, 0.18f),
                0.05f, 0.14f, 4f, 70, new Vector3(0f, 1.2f, 5f), 10f, 2);
            MakeParticles("Fireflies", mat, new Color(1f, 0.78f, 0.38f, 0.95f), new Color(1f, 0.9f, 0.55f, 0.8f),
                0.10f, 0.20f, 1.3f, 26, new Vector3(0f, 0.5f, 5f), 8f, 6);
        }

        static void MakeParticles(string name, Material mat, Color c0, Color c1, float sMin, float sMax, float rate, int max,
            Vector3 offset, float drift, int order)
        {
            var go = new GameObject(name);
            go.transform.position = cameraTransform.position + offset;
            var ps = go.AddComponent<ParticleSystem>();
            go.AddComponent<AmbientDustParticles>().Configure(cameraTransform, offset);

            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 13f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(sMin, sMax);
            main.startColor = new ParticleSystem.MinMaxGradient(c0, c1);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = max;
            main.gravityModifier = 0f;
            main.prewarm = true;

            var emission = ps.emission;
            emission.rateOverTime = rate;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20f, 9f, 1f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            float d = drift * 0.012f;
            vel.x = new ParticleSystem.MinMaxCurve(-d * 10f, d * 10f);
            vel.y = new ParticleSystem.MinMaxCurve(0.02f, 0.14f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalY = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalOffsetX = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalOffsetY = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.orbitalOffsetZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.radial = new ParticleSystem.MinMaxCurve(0f, 0f);
            vel.speedModifier = new ParticleSystem.MinMaxCurve(1f, 1f);

            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f),
                    new GradientAlphaKey(0.7f, 0.75f), new GradientAlphaKey(0f, 1f)
                });
            col.color = g;

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sortingLayerName = LayerForeground;
            r.sortingOrder = order;
            if (mat != null)
            {
                r.sharedMaterial = mat;
            }
        }

        // ------------------------------------------------------------------
        // Utilidades
        // ------------------------------------------------------------------

        static Sprite LoadSprite(string path)
        {
            Sprite s = ProceduralSpriteUtil.LoadSprite(path);
            if (s == null)
            {
                Debug.LogWarning($"[Días Prestados] Sprite no encontrado: {path}");
            }

            return s;
        }

        static SpriteRenderer Spr(Transform parent, string name, Sprite sprite, Vector2 pos, string layer, int order,
            Color color, float scale, bool lit)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sharedMaterial = lit ? litMaterial : unlitMaterial;
            Sort(sr, layer, order);
            return sr;
        }

        static void SolidRect(Transform parent, string name, Color color, Vector2 center, Vector2 size, string layer, int order)
        {
            Sprite px = LoadSprite("Assets/Art/Prototype/white_pixel.png");
            var sr = Spr(parent, name, px, center, layer, order, color, 1f, false);
            Vector2 native = px.bounds.size;
            sr.transform.localScale = new Vector3(size.x / native.x, size.y / native.y, 1f);
        }

        static void Sort(SpriteRenderer sr, string layer, int order)
        {
            sr.sortingLayerName = layer;
            int id = SortingLayer.NameToID(layer);
            if (id != 0)
            {
                sr.sortingLayerID = id;
            }

            sr.sortingOrder = order;
        }

        static Material FindMaterial(string name)
        {
            foreach (string guid in AssetDatabase.FindAssets(name + " t:Material"))
            {
                var m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (m != null && m.name == name)
                {
                    return m;
                }
            }

            Debug.LogWarning($"[Días Prestados] Material {name} no encontrado; se usará el predeterminado.");
            return null;
        }

        static PhysicsMaterial2D EnsureFrictionless()
        {
            const string path = "Assets/Art/Demo/Frictionless.physicsMaterial2D";
            var m = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (m != null)
            {
                return m;
            }

            ProceduralSpriteUtil.EnsureFolder("Assets/Art/Demo");
            m = new PhysicsMaterial2D("Frictionless") { friction = 0f, bounciness = 0f };
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static int EnsureLayer(string layerName)
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            int firstEmpty = -1;
            for (int i = 8; i < layers.arraySize; i++)
            {
                string value = layers.GetArrayElementAtIndex(i).stringValue;
                if (value == layerName)
                {
                    return i;
                }

                if (firstEmpty < 0 && string.IsNullOrEmpty(value))
                {
                    firstEmpty = i;
                }
            }

            if (firstEmpty < 0)
            {
                Debug.LogError("[Días Prestados] No hay capas libres para 'Companion'.");
                return 0;
            }

            layers.GetArrayElementAtIndex(firstEmpty).stringValue = layerName;
            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return firstEmpty;
        }
    }
}
