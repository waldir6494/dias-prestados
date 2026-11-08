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

        [Header("Sincronía por distancia")]
        [Tooltip("Distancia (u) que recorre el personaje durante un ciclo completo de caminar.")]
        [SerializeField] float strideLength = 1.6f;
        [SerializeField] float holdWalkTime = 0.12f;

        const string WalkState = "Walk";
        const string IdleState = "Idle";
        float phase;
        float holdTimer;
        bool wasWalking;

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

            float absVx = Mathf.Abs(body.linearVelocity.x);
            bool grounded = controller.IsGrounded;

            // Speed siempre refleja |vx| (también en aire) para no forzar Idle painterly.
            // Idle/Walk usan el mismo set pixel; Jump/Fall reales siguen pendientes de arte.
            animator.SetFloat(speedHash, absVx);
            animator.SetBool(groundedHash, grounded);

            // Con toques cortos de A/D la velocidad apenas supera el umbral unos frames: el ciclo no llegaba a
            // moverse. Se sincroniza por distancia recorrida (cada paso mueve las piernas) y la intención de
            // moverse mantiene la pose de caminar un instante.
            bool moving = absVx > walkSpeedThreshold || (grounded && Mathf.Abs(controller.MoveInput) > 0.1f);
            if (moving)
            {
                holdTimer = holdWalkTime;
            }
            else if (holdTimer > 0f)
            {
                holdTimer -= Time.deltaTime;
            }

            bool showWalk = holdTimer > 0f && grounded;
            if (showWalk)
            {
                // Mínimo de avance por frame para que un toque mínimo ya cambie de fotograma.
                phase += Mathf.Max(absVx, 1.2f) * Time.deltaTime / Mathf.Max(0.1f, strideLength);
                animator.speed = 0f;
                animator.Play(WalkState, 0, Mathf.Repeat(phase, 1f));
                wasWalking = true;
                if (restPoseCached)
                {
                    visualRoot.localPosition = restLocalPosition;
                    visualRoot.localRotation = Quaternion.identity;
                    visualRoot.localScale = new Vector3(visualScale, visualScale, 1f);
                }
            }
            else
            {
                animator.speed = 1f;
                if (wasWalking)
                {
                    animator.Play(IdleState, 0, 0f);
                    wasWalking = false;
                }

                // El clip Idle alterna dos poses del torso, como el hermano (3 + 2 muestras a 3 fps).
                // La respiración está en los píxeles superiores: no mover ni escalar piernas/pies.
                // En el aire se mantiene la pose neutra solo si el estado actual es Idle.
                if (!grounded && animator.GetCurrentAnimatorStateInfo(0).IsName(IdleState))
                {
                    animator.Play(IdleState, 0, 0f);
                    animator.speed = 0f;
                }

                if (restPoseCached)
                {
                    visualRoot.localRotation = Quaternion.identity;
                    visualRoot.localPosition = restLocalPosition;
                    visualRoot.localScale = new Vector3(visualScale, visualScale, 1f);
                }
            }
        }
    }
}
