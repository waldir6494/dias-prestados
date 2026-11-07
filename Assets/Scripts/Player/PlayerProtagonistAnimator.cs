using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Driver del Animator: Idle↔Walk del mismo set pixel (walk_01..08).
    /// No cambia a ProtagonistApproved (evita salto de estilo).
    /// Jump/Fall/Landing: sin clips válidos — no se inventan; ver Docs.
    /// Sin root motion; sin escalas distintas por estado.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerProtagonistAnimator : MonoBehaviour
    {
        const string SpeedParam = "Speed";
        const string GroundedParam = "Grounded";

        [Header("Referencias")]
        [SerializeField] Transform visualRoot;
        [SerializeField] SpriteRenderer visualRenderer;
        [SerializeField] Animator animator;
        [SerializeField] PlayerController2D controller;
        [SerializeField] Rigidbody2D body;

        [Header("Walk")]
        [SerializeField] float walkSpeedThreshold = 0.35f;
        [Tooltip("Velocidad horizontal (u/s) a la que el clip Walk se reproduce a speed=1.")]
        [SerializeField] float referenceWalkSpeed = 3.5f;
        [SerializeField] float minAnimSpeed = 0.55f;
        [SerializeField] float maxAnimSpeed = 1.6f;

        [Header("Escala única del Visual (todos los estados)")]
        [Tooltip("Una sola escala para el set Walk. No usar escalas distintas Idle/Walk.")]
        [SerializeField] float visualScale = 0.76f;

        int speedHash;
        int groundedHash;
        Vector3 restLocalPosition;
        bool restPoseCached;

        void Awake()
        {
            if (controller == null)
            {
                controller = GetComponent<PlayerController2D>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }

            ResolveVisual();
            speedHash = Animator.StringToHash(SpeedParam);
            groundedHash = Animator.StringToHash(GroundedParam);

            if (visualRoot != null)
            {
                restLocalPosition = visualRoot.localPosition;
                restPoseCached = true;
                // Escala única estable (no por estado).
                visualRoot.localScale = new Vector3(visualScale, visualScale, 1f);
            }

            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }

        void ResolveVisual()
        {
            if (visualRoot == null)
            {
                Transform found = transform.Find("Visual");
                visualRoot = found != null ? found : null;
            }

            if (visualRoot == null)
            {
                Debug.LogError($"{nameof(PlayerProtagonistAnimator)}: falta hijo Visual.", this);
                return;
            }

            if (visualRenderer == null)
            {
                visualRenderer = visualRoot.GetComponent<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = visualRoot.GetComponent<Animator>();
            }
        }

        void LateUpdate()
        {
            if (controller == null || body == null || animator == null)
            {
                return;
            }

            // Sin root motion visual: no tocar posición/rotación del Visual.
            if (restPoseCached)
            {
                visualRoot.localPosition = restLocalPosition;
                visualRoot.localRotation = Quaternion.identity;
                visualRoot.localScale = new Vector3(visualScale, visualScale, 1f);
            }

            float absVx = Mathf.Abs(body.linearVelocity.x);
            bool grounded = controller.IsGrounded;

            // Speed siempre refleja |vx| (también en aire) para no forzar Idle painterly.
            // Idle/Walk usan el mismo set pixel; Jump/Fall reales siguen pendientes de arte.
            animator.SetFloat(speedHash, absVx);
            animator.SetBool(groundedHash, grounded);

            bool walking = absVx > walkSpeedThreshold;
            if (walking)
            {
                float play = absVx / Mathf.Max(0.01f, referenceWalkSpeed);
                animator.speed = Mathf.Clamp(play, minAnimSpeed, maxAnimSpeed);
            }
            else
            {
                animator.speed = 1f;
            }
        }
    }
}
