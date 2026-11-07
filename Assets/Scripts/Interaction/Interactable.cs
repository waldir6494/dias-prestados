using System.Collections.Generic;
using UnityEngine;

namespace DiasPrestados.Interaction
{
    /// <summary>
    /// Base de objetos/personajes con los que el protagonista puede interactuar (tecla E).
    /// Se registran en una lista estática para evitar búsquedas globales por frame.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        static readonly List<Interactable> Registry = new List<Interactable>();

        [SerializeField] Transform promptAnchor;
        [SerializeField] float interactionRadius = 3.2f;

        public static IReadOnlyList<Interactable> All => Registry;

        public float InteractionRadius => interactionRadius;
        public Vector3 PromptPosition => promptAnchor != null ? promptAnchor.position : transform.position;
        public Transform PromptAnchor => promptAnchor != null ? promptAnchor : transform;

        /// <summary>Si es false no se ofrece la interacción ni se muestra el indicador.</summary>
        public virtual bool CanInteract => isActiveAndEnabled;

        public abstract void Interact(GameObject actor);

        protected virtual void OnEnable()
        {
            if (!Registry.Contains(this))
            {
                Registry.Add(this);
            }
        }

        protected virtual void OnDisable()
        {
            Registry.Remove(this);
        }
    }
}
