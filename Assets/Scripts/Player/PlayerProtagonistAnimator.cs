using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Driver de Animator del protagonista: Idle (PNG aprobado) ↔ Walk (8 fotogramas).
    /// Solo Walk está integrado por frames. Sin root motion físico.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerProtagonistAnimator : MonoBehaviour
    {
        const string SpeedParam = "Speed";
        const string WalkStateName = "Walk";

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

        [Header("Escala visual (Idle painterly vs Walk pixel)")]
        [SerializeField] float idleVisualScale = 1f;
        [Tooltip("Walk a PPU 48 queda ~2.25u; se escala para acercarse a la altura Idle ~1.72u.")]
        [SerializeField] float walkVisualScale = 0.76f;

        int speedHash;
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

            if (visualRoot != null)
            {
                restLocalPosition = visualRoot.localPosition;
                restPoseCached = true;
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

            // Sin root motion visual: posición del Visual fija (pies en y=-0.85).
            if (restPoseCached)
            {
                visualRoot.localPosition = restLocalPosition;
                visualRoot.localRotation = Quaternion.identity;
            }

            float absVx = Mathf.Abs(body.linearVelocity.x);
            bool grounded = controller.IsGrounded;
            float speedParam = grounded ? absVx : 0f;
            animator.SetFloat(speedHash, speedParam);

            bool walking = grounded && absVx > walkSpeedThreshold;
            float targetScale = walking ? walkVisualScale : idleVisualScale;
            visualRoot.localScale = new Vector3(targetScale, targetScale, 1f);

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
