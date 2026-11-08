using DiasPrestados.Companions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Hermano pequeño oficial basado en PNG (derivado de 03_VISTAS_ADICIONALES.png).
    /// Fotogramas en Assets/Art/Characters/LittleBrother_Png (64x142 px, pivote en los pies, 60 ppu).
    /// Hermano oficial por PNG. El generador procedural antiguo y su carpeta LittleBrother fueron eliminados.
    /// </summary>
    public static class DemoBrotherPng
    {
        public const string Folder = "Assets/Art/Characters/LittleBrother_Png";
        const string ScenePath = "Assets/Scenes/DiasPrestadosDemo.unity";

        static readonly string[] Names =
        {
            "idle_0", "idle_1",
            "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7",
            "jump", "fall", "ask_0", "ask_1", "cheer_0", "cheer_1", "cheer_2", "cheer_3",
        };

        public static string FramePath(string name) => $"{Folder}/{name}.png";

        public static void ConfigureImporters()
        {
            foreach (string n in Names)
            {
                string path = FramePath(n);
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti == null)
                {
                    Debug.LogWarning("[Días Prestados] Falta el PNG del hermano: " + path);
                    continue;
                }

                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.spritePixelsPerUnit = 60f;
                ti.filterMode = FilterMode.Point;
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                ti.SetTextureSettings(settings);
                ti.spritePivot = new Vector2(0.5f, 2f / 142f);
                ti.SaveAndReimport();
            }
        }

        static Sprite Load(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(name));

        static Sprite[] Set(params string[] names)
        {
            var arr = new Sprite[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                arr[i] = Load(names[i]);
            }

            return arr;
        }

        static void Assign(SerializedObject so, string prop, params string[] names)
        {
            SerializedProperty p = so.FindProperty(prop);
            p.arraySize = names.Length;
            for (int i = 0; i < names.Length; i++)
            {
                p.GetArrayElementAtIndex(i).objectReferenceValue = Load(names[i]);
            }
        }

        /// <summary>Asigna los fotogramas PNG al CompanionSpriteAnimator indicado (usado por el builder y por el menú).</summary>
        public static void ApplyTo(CompanionSpriteAnimator anim, SpriteRenderer sr)
        {
            var so = new SerializedObject(anim);
            Assign(so, "idle", "idle_0", "idle_0", "idle_0", "idle_1", "idle_1");
            Assign(so, "walk", "walk_0", "walk_1", "walk_2", "walk_3", "walk_4", "walk_5", "walk_6", "walk_7");
            Assign(so, "jump", "jump");
            Assign(so, "fall", "fall");
            Assign(so, "ask", "ask_0", "ask_1");
            Assign(so, "cheer", "cheer_0", "cheer_1", "cheer_2", "cheer_3");
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.FindProperty("visualScale").floatValue = 0.7f;
            SerializedProperty stride = so.FindProperty("strideLength");
            if (stride != null)
            {
                stride.floatValue = 1.55f;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            sr.sprite = Load("idle_0");
        }

        [MenuItem("Tools/Dias Prestados/Demo/Aplicar hermano PNG a DiasPrestadosDemo")]
        public static void ApplyToScene()
        {
            ConfigureImporters();
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            var anim = Object.FindFirstObjectByType<CompanionSpriteAnimator>();
            if (anim == null)
            {
                Debug.LogError("[Días Prestados] No hay CompanionSpriteAnimator en la escena.");
                return;
            }

            var sr = anim.GetComponentInChildren<SpriteRenderer>();
            ApplyTo(anim, sr);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Días Prestados] Hermano PNG aplicado a DiasPrestadosDemo (solo sprites del hermano).");
        }
    }
}
