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

            try
            {
                File.Delete(FlagPath);
            }
            catch
            {
                return;
            }

            // Obsoleto: ForestDemo.unity ya no existe. No regenerar.
            Debug.LogWarning(
                "[Días Prestados] Auto-build ForestDemo desactivado. Usa Assets/Scenes/ForestOrganicDemo.unity.");
            waitingForEditMode = false;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
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
