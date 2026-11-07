using System.IO;
using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor
{
    /// <summary>
    /// Aplica una actualización silenciosa de ForestDemo si existe Temp/BuildForestDemo.flag.
    /// Sale de Play Mode automáticamente si hace falta antes de guardar.
    /// </summary>
    [InitializeOnLoad]
    static class ForestDemoAutoBuildOnce
    {
        const string FlagPath = "Temp/BuildForestDemo.flag";
        static bool waitingForEditMode;

        static ForestDemoAutoBuildOnce()
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
                    Debug.Log("[Días Prestados] Auto-build ForestDemo: saliendo de Play Mode...");
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

            Debug.Log("[Días Prestados] Auto-build ForestDemo (legibilidad/sorting)…");
            try
            {
                ForestDemoSceneBuilder.CreateOrUpdateForestDemo(interactive: false);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Días Prestados] Auto-build ForestDemo falló: {ex}");
            }
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
