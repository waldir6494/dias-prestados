using System.IO;
using DiasPrestados;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Genera el primer prototipo jugable: escenario, personaje, cámara, sprites y escena guardada.
    /// Menú: Tools > Dias Prestados > Crear prototipo
    /// </summary>
    public static class PrototypeSceneGenerator
    {
        const string ScenePath = "Assets/Scenes/Prototype.unity";
        const string ArtFolder = "Assets/Art/Prototype";
        const string InputAssetPath = "Assets/Input/PrototypeControls.inputactions";
        const string WhiteSpritePath = ArtFolder + "/white_pixel.png";
        const string PlayerSpritePath = ArtFolder + "/player_child.png";
        const string TreeSpritePath = ArtFolder + "/tree_silhouette.png";
        const string RockSpritePath = ArtFolder + "/rock.png";

        const float SceneMinX = -20f;
        const float SceneMaxX = 20f;
        const float GroundTopY = 0f;
        const float PlayerSpawnX = -16f;

        [MenuItem("Tools/Dias Prestados/Crear prototipo")]
        public static void CreatePrototype()
        {
            if (File.Exists(ScenePath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                if (Application.isBatchMode)
                {
                    Debug.Log("[Días Prestados] Prototype.unity ya existe. En batch mode se reemplaza.");
                }
                else
                {
                    bool replace = EditorUtility.DisplayDialog(
                        "Días Prestados",
                        "La escena Prototype.unity ya existe.\n\n¿Quieres reemplazarla?\nSampleScene no se modificará.",
                        "Reemplazar",
                        "Cancelar");

                    if (!replace)
                    {
                        return;
                    }
                }
            }

            try
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("Días Prestados", "Preparando carpetas y sprites...", 0.1f);
                }

                EnsureFolders();
                CreatePersistentSprites();

                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("Días Prestados", "Construyendo escena...", 0.45f);
                }

                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = "Prototype";

                BuildPrototypeScene();

                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayProgressBar("Días Prestados", "Guardando escena...", 0.85f);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? "Assets/Scenes");
                bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                if (!saved)
                {
                    Debug.LogError("[Días Prestados] No se pudo guardar Prototype.unity.");
                    if (!Application.isBatchMode)
                    {
                        EditorUtility.DisplayDialog("Días Prestados", "No se pudo guardar Prototype.unity.", "Aceptar");
                    }

                    return;
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
                if (sceneAsset != null && !Application.isBatchMode)
                {
                    Selection.activeObject = sceneAsset;
                    EditorGUIUtility.PingObject(sceneAsset);
                }

                Debug.Log($"[Días Prestados] Prototipo creado en {ScenePath}. Pulsa Play para probar (A/D o flechas + Espacio).");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog(
                        "Días Prestados",
                        "Prototipo creado.\n\nEscena: Assets/Scenes/Prototype.unity\nControles: A/D o flechas + Espacio.\n\nPulsa Play para jugar.",
                        "Aceptar");
                }
            }
            finally
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        static void EnsureFolders()
        {
            EnsureFolder("Assets/Art");
            EnsureFolder(ArtFolder);
            EnsureFolder("Assets/Scenes");
            EnsureFolder("Assets/Input");
            EnsureFolder("Assets/Scripts");
            EnsureFolder("Assets/Scripts/Player");
            EnsureFolder("Assets/Scripts/Camera");
            EnsureFolder("Assets/Editor");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        static void CreatePersistentSprites()
        {
            WritePngIfNeeded(WhiteSpritePath, CreateSolidTexture(8, 8, Color.white), 8f);
            WritePngIfNeeded(PlayerSpritePath, CreateChildSilhouetteTexture(64, 96), 48f);
            WritePngIfNeeded(TreeSpritePath, CreateTreeTexture(48, 96), 32f);
            WritePngIfNeeded(RockSpritePath, CreateRockTexture(48, 32), 32f);
            AssetDatabase.Refresh();
        }

        static void WritePngIfNeeded(string assetPath, Texture2D texture, float pixelsPerUnit)
        {
            string absolute = ToAbsolutePath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? absolute);

            byte[] png = texture.EncodeToPNG();
            Object.DestroyImmediate(texture);
            File.WriteAllBytes(absolute, png);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureSpriteImporter(assetPath, pixelsPerUnit);
        }

        static void ConfigureSpriteImporter(string assetPath, float pixelsPerUnit)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"No se pudo configurar importador de sprite: {assetPath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(projectRoot ?? Application.dataPath, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }

        static void BuildPrototypeScene()
        {
            Sprite white = LoadSprite(WhiteSpritePath);
            Sprite playerSprite = LoadSprite(PlayerSpritePath);
            Sprite treeSprite = LoadSprite(TreeSpritePath);
            Sprite rockSprite = LoadSprite(RockSpritePath);
            InputActionAsset inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);

            if (white == null || playerSprite == null)
            {
                throw new System.InvalidOperationException("No se pudieron cargar los sprites del prototipo.");
            }

            if (inputAsset == null)
            {
                throw new System.InvalidOperationException(
                    $"No se encontró {InputAssetPath}. Asegúrate de que el asset exista en el proyecto.");
            }

            CreateBackground(white, treeSprite);
            Transform environmentRoot = CreateEnvironment(white, rockSprite);
            GameObject player = CreatePlayer(playerSprite, inputAsset);
            CreateCamera(player.transform);
            CreateGlobalLight();

            // Evita warning de escena vacía de iluminación; mantiene raíces organizadas.
            environmentRoot.SetAsFirstSibling();
        }

        static Transform CreateEnvironment(Sprite white, Sprite rockSprite)
        {
            var root = new GameObject("Environment").transform;

            // Suelo principal (~40 unidades).
            CreateSolid(root, "Ground", white, new Color(0.33f, 0.45f, 0.28f),
                new Vector3(0f, -0.75f, 0f), new Vector2(40f, 1.5f), sortingOrder: 0, withCollider: true);

            // Desnivel elevado a la derecha.
            CreateSolid(root, "RaisedShelf", white, new Color(0.36f, 0.48f, 0.30f),
                new Vector3(14f, 0.5f, 0f), new Vector2(10f, 1f), sortingOrder: 1, withCollider: true);

            // 4 plataformas alcanzables (salto ~2.5u con jumpForce 12.5 / gravityScale 3).
            CreateSolid(root, "Platform_01", white, new Color(0.45f, 0.34f, 0.24f),
                new Vector3(-9f, 2.1f, 0f), new Vector2(3.2f, 0.35f), sortingOrder: 2, withCollider: true);
            CreateSolid(root, "Platform_02", white, new Color(0.45f, 0.34f, 0.24f),
                new Vector3(-3.2f, 3.4f, 0f), new Vector2(2.8f, 0.35f), sortingOrder: 2, withCollider: true);
            CreateSolid(root, "Platform_03", white, new Color(0.45f, 0.34f, 0.24f),
                new Vector3(3.5f, 2.4f, 0f), new Vector2(3.4f, 0.35f), sortingOrder: 2, withCollider: true);
            CreateSolid(root, "Platform_04", white, new Color(0.45f, 0.34f, 0.24f),
                new Vector3(9.5f, 3.2f, 0f), new Vector2(3f, 0.35f), sortingOrder: 2, withCollider: true);

            // Obstáculos saltables.
            CreateSolid(root, "Obstacle_Crate", white, new Color(0.55f, 0.38f, 0.22f),
                new Vector3(-12.5f, 0.55f, 0f), new Vector2(1.1f, 1.1f), sortingOrder: 3, withCollider: true);
            CreateSolid(root, "Obstacle_Crate_2", white, new Color(0.5f, 0.33f, 0.2f),
                new Vector3(1.2f, 0.45f, 0f), new Vector2(0.9f, 0.9f), sortingOrder: 3, withCollider: true);

            if (rockSprite != null)
            {
                CreateDecor(root, "Rock_A", rockSprite, new Color(0.55f, 0.55f, 0.58f),
                    new Vector3(-6.5f, 0.35f, 0f), new Vector2(1.4f, 0.9f), sortingOrder: 4, withCollider: true);
                CreateDecor(root, "Rock_B", rockSprite, new Color(0.5f, 0.52f, 0.55f),
                    new Vector3(6.8f, 0.35f, 0f), new Vector2(1.6f, 1f), sortingOrder: 4, withCollider: true);
            }

            // Límites invisibles para no caer fuera del prototipo.
            CreateSolid(root, "Wall_Left", white, new Color(1f, 1f, 1f, 0f),
                new Vector3(SceneMinX - 0.5f, 4f, 0f), new Vector2(1f, 12f), sortingOrder: 0, withCollider: true);
            CreateSolid(root, "Wall_Right", white, new Color(1f, 1f, 1f, 0f),
                new Vector3(SceneMaxX + 0.5f, 4f, 0f), new Vector2(1f, 12f), sortingOrder: 0, withCollider: true);

            return root;
        }

        static void CreateBackground(Sprite white, Sprite treeSprite)
        {
            var root = new GameObject("Background").transform;

            // Cielo
            CreateSolid(root, "Sky", white, new Color(0.55f, 0.72f, 0.88f),
                new Vector3(0f, 6f, 0f), new Vector2(60f, 20f), sortingOrder: -100, withCollider: false);

            // Montañas lejanas
            CreateSolid(root, "Mountains_Far", white, new Color(0.42f, 0.52f, 0.58f),
                new Vector3(-8f, 2.8f, 0f), new Vector2(22f, 6f), sortingOrder: -80, withCollider: false);
            CreateSolid(root, "Mountains_Near", white, new Color(0.35f, 0.45f, 0.42f),
                new Vector3(10f, 2.2f, 0f), new Vector2(26f, 5f), sortingOrder: -70, withCollider: false);

            // Franja de bosque
            CreateSolid(root, "ForestBand", white, new Color(0.22f, 0.34f, 0.24f),
                new Vector3(0f, 0.8f, 0f), new Vector2(50f, 2.2f), sortingOrder: -50, withCollider: false);

            if (treeSprite != null)
            {
                float[] treeXs = { -17f, -14f, -10f, -5f, 0f, 4f, 8f, 12f, 16f, 18.5f };
                for (int i = 0; i < treeXs.Length; i++)
                {
                    float scale = 1.4f + (i % 3) * 0.25f;
                    CreateDecor(root, $"Tree_{i:00}", treeSprite, new Color(0.18f, 0.30f, 0.20f),
                        new Vector3(treeXs[i], 1.1f + scale * 0.35f, 0f),
                        new Vector2(scale * 0.9f, scale * 1.6f),
                        sortingOrder: -40 + (i % 3),
                        withCollider: false);
                }
            }
        }

        static GameObject CreatePlayer(Sprite playerSprite, InputActionAsset inputAsset)
        {
            var player = new GameObject("Player");
            player.tag = "Player";
            player.layer = 0;
            player.transform.position = new Vector3(PlayerSpawnX, 1.1f, 0f);

            var rb = player.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;
            rb.mass = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

            var collider = player.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.55f, 1.7f);
            collider.offset = new Vector2(0f, 0f);
            collider.direction = CapsuleDirection2D.Vertical;

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = Vector3.zero;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = playerSprite;
            sr.color = Color.white;
            sr.sortingOrder = 20;
            // Sprite ~64x96 @ 48 PPU => ~1.33 x 2.0; escala para silueta infantil ~1.8u de alto.
            visual.transform.localScale = new Vector3(1.15f, 1.15f, 1f);

            var groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.SetParent(player.transform, false);
            groundCheck.transform.localPosition = new Vector3(0f, -0.88f, 0f);

            var controller = player.AddComponent<PlayerController2D>();
            var controllerSo = new SerializedObject(controller);
            controllerSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            controllerSo.FindProperty("groundCheck").objectReferenceValue = groundCheck.transform;
            controllerSo.FindProperty("groundLayers").intValue = ~0;
            controllerSo.ApplyModifiedPropertiesWithoutUndo();

            return player;
        }

        static void CreateCamera(Transform player)
        {
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";

            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(PlayerSpawnX, 2.2f, -10f);

            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<UniversalAdditionalCameraData>();

            var follow = cameraGo.AddComponent<CameraFollow2D>();
            var followSo = new SerializedObject(follow);
            followSo.FindProperty("target").objectReferenceValue = player;
            followSo.FindProperty("offset").vector2Value = new Vector2(0f, 1.2f);
            followSo.FindProperty("smoothTime").floatValue = 0.18f;
            followSo.FindProperty("minBounds").vector2Value = new Vector2(SceneMinX, -1.5f);
            followSo.FindProperty("maxBounds").vector2Value = new Vector2(SceneMaxX, 10f);
            followSo.FindProperty("useBounds").boolValue = true;
            followSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CreateGlobalLight()
        {
            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;
            light.color = Color.white;
        }

        static GameObject CreateSolid(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector3 position,
            Vector2 size,
            int sortingOrder,
            bool withCollider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            sr.drawMode = SpriteDrawMode.Simple;

            if (withCollider)
            {
                var box = go.AddComponent<BoxCollider2D>();
                box.size = Vector2.one;
            }

            return go;
        }

        static GameObject CreateDecor(
            Transform parent,
            string name,
            Sprite sprite,
            Color color,
            Vector3 position,
            Vector2 size,
            int sortingOrder,
            bool withCollider)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;

            if (withCollider)
            {
                var box = go.AddComponent<BoxCollider2D>();
                // Ajuste aproximado al silhouette del rock.
                box.size = new Vector2(0.9f, 0.7f);
                box.offset = new Vector2(0f, -0.05f);
            }

            return go;
        }

        static Sprite LoadSprite(string assetPath)
        {
            // Preferimos el sub-asset Sprite generado por el importador.
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            if (assets != null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is Sprite sprite)
                    {
                        return sprite;
                    }
                }
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        static Texture2D CreateSolidTexture(int width, int height, Color color)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        static Texture2D CreateChildSilhouetteTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Clear(tex, Color.clear);

            Color skin = new Color(0.93f, 0.78f, 0.62f);
            Color hair = new Color(0.35f, 0.22f, 0.14f);
            Color shirt = new Color(0.35f, 0.55f, 0.72f);
            Color pants = new Color(0.28f, 0.32f, 0.45f);
            Color shoe = new Color(0.22f, 0.18f, 0.16f);

            // Cabeza
            FillCircle(tex, width / 2, 74, 12, skin);
            // Pelo
            FillCircle(tex, width / 2, 80, 12, hair);
            FillRect(tex, 20, 72, 24, 8, hair);
            // Torso
            FillRect(tex, 22, 40, 20, 26, shirt);
            // Brazos
            FillRect(tex, 12, 44, 8, 18, skin);
            FillRect(tex, 44, 44, 8, 18, skin);
            // Piernas
            FillRect(tex, 24, 14, 7, 26, pants);
            FillRect(tex, 33, 14, 7, 26, pants);
            // Zapatos
            FillRect(tex, 22, 10, 10, 5, shoe);
            FillRect(tex, 32, 10, 10, 5, shoe);
            // Ojos sencillos
            SetPixelSafe(tex, 27, 74, new Color(0.15f, 0.12f, 0.1f));
            SetPixelSafe(tex, 28, 74, new Color(0.15f, 0.12f, 0.1f));
            SetPixelSafe(tex, 35, 74, new Color(0.15f, 0.12f, 0.1f));
            SetPixelSafe(tex, 36, 74, new Color(0.15f, 0.12f, 0.1f));

            tex.Apply();
            return tex;
        }

        static Texture2D CreateTreeTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Clear(tex, Color.clear);

            Color trunk = new Color(0.35f, 0.22f, 0.12f);
            Color foliage = new Color(0.2f, 0.42f, 0.22f);

            FillRect(tex, width / 2 - 3, 4, 6, 36, trunk);
            FillCircle(tex, width / 2, 58, 16, foliage);
            FillCircle(tex, width / 2 - 10, 48, 12, foliage);
            FillCircle(tex, width / 2 + 10, 48, 12, foliage);
            FillCircle(tex, width / 2, 70, 11, foliage);

            tex.Apply();
            return tex;
        }

        static Texture2D CreateRockTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Clear(tex, Color.clear);

            Color rock = new Color(0.55f, 0.55f, 0.58f);
            Color shade = new Color(0.4f, 0.4f, 0.43f);

            FillEllipse(tex, width / 2, height / 2 - 2, width / 2 - 2, height / 2 - 4, rock);
            FillEllipse(tex, width / 2 - 4, height / 2 - 4, width / 3, height / 3, shade);

            tex.Apply();
            return tex;
        }

        static void Clear(Texture2D tex, Color color)
        {
            var pixels = new Color[tex.width * tex.height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
        }

        static void FillRect(Texture2D tex, int x, int y, int w, int h, Color color)
        {
            for (int py = y; py < y + h; py++)
            {
                for (int px = x; px < x + w; px++)
                {
                    SetPixelSafe(tex, px, py, color);
                }
            }
        }

        static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
        {
            int r2 = radius * radius;
            for (int py = cy - radius; py <= cy + radius; py++)
            {
                for (int px = cx - radius; px <= cx + radius; px++)
                {
                    int dx = px - cx;
                    int dy = py - cy;
                    if (dx * dx + dy * dy <= r2)
                    {
                        SetPixelSafe(tex, px, py, color);
                    }
                }
            }
        }

        static void FillEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color color)
        {
            for (int py = cy - ry; py <= cy + ry; py++)
            {
                for (int px = cx - rx; px <= cx + rx; px++)
                {
                    float nx = (px - cx) / (float)Mathf.Max(1, rx);
                    float ny = (py - cy) / (float)Mathf.Max(1, ry);
                    if (nx * nx + ny * ny <= 1f)
                    {
                        SetPixelSafe(tex, px, py, color);
                    }
                }
            }
        }

        static void SetPixelSafe(Texture2D tex, int x, int y, Color color)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
            {
                return;
            }

            tex.SetPixel(x, y, color);
        }
    }
}
