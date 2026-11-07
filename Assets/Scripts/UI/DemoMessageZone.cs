using UnityEngine;

namespace DiasPrestados.UI
{
    /// <summary>
    /// Muestra un mensaje una sola vez cuando el protagonista supera cierta X. Sin colliders.
    /// </summary>
    public class DemoMessageZone : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] float triggerX = 190f;
        [SerializeField] string message = "Fin de la primera prueba";
        [SerializeField] float seconds = 7f;

        bool shown;

        void Update()
        {
            if (shown || target == null)
            {
                return;
            }

            if (target.position.x >= triggerX && DemoHudOverlay.Instance != null)
            {
                shown = true;
                DemoHudOverlay.Instance.ShowMessage(message, seconds);
            }
        }
    }
}
