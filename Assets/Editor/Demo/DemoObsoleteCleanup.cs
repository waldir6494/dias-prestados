using UnityEditor;
using UnityEngine;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Elimina assets del hermano procedural y la vista previa temporal, ya sin referencias activas.
    /// Conserva LittleBrother_Png (oficial) y LittleBrother_Alt (fuente del perfil aprobado).
    /// </summary>
    public static class DemoObsoleteCleanup
    {
        static readonly string[] ObsoletePaths =
        {
            "Assets/Art/Characters/LittleBrother",
            "Assets/Editor/Demo/DemoBrotherArt.cs",
            "Assets/Editor/Demo/DemoBrotherAltPreview.cs",
        };

        [MenuItem("Tools/Dias Prestados/Demo/Limpiar hermano procedural obsoleto")]
        public static void Clean()
        {
            int removed = 0;
            foreach (string path in ObsoletePaths)
            {
                if (!AssetDatabase.LoadAssetAtPath<Object>(path) && !AssetDatabase.IsValidFolder(path))
                {
                    // Puede ser script aún no cargado como Object; comprobar por GUID/meta.
                    if (!System.IO.File.Exists(path) && !System.IO.Directory.Exists(path))
                    {
                        Debug.Log("[Días Prestados] Ya ausente: " + path);
                        continue;
                    }
                }

                if (AssetDatabase.DeleteAsset(path))
                {
                    removed++;
                    Debug.Log("[Días Prestados] Eliminado: " + path);
                }
                else
                {
                    Debug.LogWarning("[Días Prestados] No se pudo eliminar (¿referencias?): " + path);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Días Prestados] Limpieza terminada. Entradas eliminadas: {removed}.");
        }
    }
}
