using UnityEngine;
using DiasPrestados.Interaction;

namespace DiasPrestados.Companions
{
    /// <summary>
    /// Permite al hermano mayor ayudar al pequeño (tecla E) cuando éste pide ayuda ante un obstáculo alto.
    /// </summary>
    [RequireComponent(typeof(CompanionFollower))]
    public class CompanionBoostInteractable : Interactable
    {
        CompanionFollower follower;

        public override bool CanInteract => isActiveAndEnabled && follower != null && follower.IsWaitingForHelp;

        void Awake()
        {
            follower = GetComponent<CompanionFollower>();
        }

        public override void Interact(GameObject actor)
        {
            follower.BeginBoost();
        }
    }
}
