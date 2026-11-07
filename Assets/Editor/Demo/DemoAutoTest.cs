using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using DiasPrestados.Companions;
using DiasPrestados.Testing;

namespace DiasPrestados.Editor.Demo
{
    /// <summary>
    /// Prueba automática en Play Mode del Hito 1: un "piloto" virtual con teclado simulado recorre el nivel,
    /// comprueba movimiento, salto, seguimiento del hermano pequeño e interacción, y guarda capturas
    /// y un informe en Temp/DemoAutotest. Menú: Tools → Dias Prestados → Demo → Autotest Hito 1.
    /// </summary>
    [InitializeOnLoad]
    public static class DemoAutoTest
    {
        const string SessionKey = "DiasPrestados.DemoAutoTest";
        public const string OutputFolder = DemoAutoTestRunner.OutputFolder;

        static DemoAutoTest()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("Tools/Dias Prestados/Demo/Autotest Hito 1 (Play Mode)")]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Directory.CreateDirectory(OutputFolder);
            EditorSceneManager.OpenScene(DemoSceneBuilder.ScenePath);
            SessionState.SetBool(SessionKey, true);
            EditorApplication.EnterPlaymode();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(SessionKey, false))
            {
                SessionState.SetBool(SessionKey, false);
                var go = new GameObject("__DemoAutoTestRunner");
                go.AddComponent<DemoAutoTestRunner>();
            }
        }
    }
}
