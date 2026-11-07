using UnityEngine;

namespace DiasPrestados.UI
{
    /// <summary>
    /// Indicaciones mínimas en pantalla (controles al inicio y mensajes puntuales).
    /// Provisional para la primera prueba: se sustituirá por la interfaz definitiva (Hito 9).
    /// Usa IMGUI solo para dibujar; no lee entrada.
    /// </summary>
    public class DemoHudOverlay : MonoBehaviour
    {
        public static DemoHudOverlay Instance { get; private set; }

        [SerializeField] string controlsText = "A / D o flechas: mover        Flecha arriba o Espacio: saltar        E: ayudar / interactuar";
        [SerializeField] float controlsVisibleSeconds = 12f;
        [SerializeField] float fadeSeconds = 1.5f;

        string message;
        float messageTimer;
        float messageDuration;
        GUIStyle controlsStyle;
        GUIStyle messageStyle;

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void ShowMessage(string text, float seconds)
        {
            message = text;
            messageDuration = seconds;
            messageTimer = seconds;
        }

        void Update()
        {
            if (messageTimer > 0f)
            {
                messageTimer -= Time.deltaTime;
            }
        }

        void OnGUI()
        {
            if (controlsStyle == null)
            {
                controlsStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.LowerCenter, fontSize = 15 };
                controlsStyle.normal.textColor = new Color(0.92f, 0.94f, 0.9f, 1f);
                messageStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 26 };
                messageStyle.normal.textColor = new Color(1f, 0.93f, 0.78f, 1f);
            }

            float t = Time.timeSinceLevelLoad;
            float alpha = t < controlsVisibleSeconds ? 1f : Mathf.Clamp01(1f - (t - controlsVisibleSeconds) / fadeSeconds);
            if (alpha > 0f)
            {
                Color old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha * 0.9f);
                GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height - 24f), controlsText, controlsStyle);
                GUI.color = old;
            }

            if (messageTimer > 0f && !string.IsNullOrEmpty(message))
            {
                float a = Mathf.Clamp01(Mathf.Min(messageTimer, messageDuration - messageTimer) / 0.8f);
                Color old = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, a);
                GUI.Label(new Rect(0f, Screen.height * 0.18f, Screen.width, 60f), message, messageStyle);
                GUI.color = old;
            }
        }
    }
}
