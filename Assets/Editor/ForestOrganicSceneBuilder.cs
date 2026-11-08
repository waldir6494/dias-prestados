using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Escena principal única: Assets/Scenes/ForestOrganicDemo.unity.
    /// El menú solo la abre; no regenera arte, gameplay ni atmósfera.
    /// Si falta el archivo, muestra un error claro (no hay bootstrap desde escenas eliminadas).
    /// Menú: Tools > Dias Prestados > Abrir Forest Organic Demo
    /// </summary>
    public static class ForestOrganicSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/ForestOrganicDemo.unity";

        [MenuItem("Tools/Dias Prestados/Abrir Forest Organic Demo")]
        public static void OpenMainSceneMenu()
        {
            OpenMainScene(interactive: true);
        }

        /// <summary>Compatibilidad con flags/herramientas antiguas: solo abre, nunca regenera.</summary>
        public static void CreateOrUpdate(bool interactive)
        {
            OpenMainScene(interactive);
        }

        public static void OpenMainScene(bool interactive)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new System.InvalidOperationException(
                    "Sal de Play Mode antes de abrir ForestOrganicDemo.");
            }

            bool showUi = interactive && !Application.isBatchMode;

            if (!File.Exists(ScenePath))
            {
                string msg =
                    "Falta la escena principal Assets/Scenes/ForestOrganicDemo.unity.\n" +
                    "No se puede reconstruir: las escenas antiguas de bootstrap se eliminaron.\n" +
                    "Restaura ForestOrganicDemo desde control de versiones o un backup.";
                Debug.LogError("[Días Prestados] " + msg.Replace('\n', ' '));
                if (showUi)
                {
                    EditorUtility.DisplayDialog("Días Prestados", msg, "Aceptar");
                }

                return;
            }

            if (!PrepareToOpenScene(showUi))
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[Días Prestados] Escena principal abierta: " + ScenePath);

            if (showUi)
            {
                EditorUtility.DisplayDialog(
                    "Días Prestados",
                    "Escena principal:\n" + ScenePath +
                    "\n\nNo se ha regenerado contenido. Edita esta escena en el sitio.",
                    "Aceptar");
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
    }

    /// <summary>Si existe Temp/BuildForestOrganic.flag, solo abre la escena principal (nunca regenera).</summary>
    [InitializeOnLoad]
    static class ForestOrganicAutoBuildOnce
    {
        const string FlagPath = "Temp/BuildForestOrganic.flag";

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
                ForestOrganicSceneBuilder.OpenMainScene(interactive: false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Días Prestados] Abrir ForestOrganicDemo falló: {ex}");
            }
        }
    }
}
