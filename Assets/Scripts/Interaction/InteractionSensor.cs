using UnityEngine;
using UnityEngine.InputSystem;

namespace DiasPrestados.Interaction
{
    /// <summary>
    /// Detecta el Interactable más cercano al protagonista, muestra el indicador y lanza la interacción (acción Interact).
    /// </summary>
    public class InteractionSensor : MonoBehaviour
    {
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] string actionMapName = "Player";
        [SerializeField] string interactActionName = "Interact";
        [SerializeField] InteractionPrompt prompt;

        InputAction interactAction;
        Interactable current;

        public Interactable Current => current;

        void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError($"{nameof(InteractionSensor)} en '{name}' no tiene InputActionAsset.", this);
                return;
            }

            InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
            interactAction = map?.FindAction(interactActionName, throwIfNotFound: false);
            if (interactAction == null)
            {
                Debug.LogError($"Falta la acción '{interactActionName}' en el mapa '{actionMapName}'.", this);
            }
        }

        void OnEnable()
        {
            interactAction?.Enable();
        }

        void Update()
        {
            current = FindClosest();

            if (prompt != null)
            {
                if (current != null)
                {
                    prompt.Show(current.PromptAnchor, current.PromptPosition);
                }
                else
                {
                    prompt.Hide();
                }
            }

            if (current != null && interactAction != null && interactAction.WasPressedThisFrame())
            {
                current.Interact(gameObject);
            }
        }

        Interactable FindClosest()
        {
            Interactable best = null;
            float bestSqr = float.MaxValue;
            Vector2 origin = transform.position;
            var all = Interactable.All;

            for (int i = 0; i < all.Count; i++)
            {
                Interactable candidate = all[i];
                if (candidate == null || !candidate.CanInteract)
                {
                    continue;
                }

                float sqr = ((Vector2)candidate.PromptPosition - origin).sqrMagnitude;
                float radius = candidate.InteractionRadius;
                if (sqr <= radius * radius && sqr < bestSqr)
                {
                    best = candidate;
                    bestSqr = sqr;
                }
            }

            return best;
        }
    }
}
