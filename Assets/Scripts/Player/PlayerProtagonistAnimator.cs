using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// INTERIM (no es animación por fotogramas).
    /// El diseño PNG está aprobado; faltan frames Walk/Idle/Run/Jump/Fall/Landing
    /// — ver Docs/Development/PROGRESS.md (BLOQUEADO). Este componente solo aplica
    /// lean/squash leve sobre un sprite único y se sustituirá por Animator+clips
    /// cuando existan fotogramas válidos. No es marioneta.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerProtagonistAnimator : MonoBehaviour
    {
        public enum AnimState
        {
            Idle,
            Walk,
            Jump,
            Fall,
            Landing
        }

        const string ArticulatedRigName = "ArticulatedRig";

        [Header("Referencias")]
        [SerializeField] Transform visualRoot;
        [SerializeField] SpriteRenderer visualRenderer;
        [SerializeField] Sprite approvedSprite;
        [SerializeField] PlayerController2D controller;
        [SerializeField] Rigidbody2D body;

        [Header("Umbrales")]
        [SerializeField] float walkSpeedThreshold = 0.35f;
        [SerializeField] float jumpVelocityThreshold = 0.55f;
        [SerializeField] float landingDuration = 0.16f;

        [Header("Idle (respiración, pies fijos)")]
        [SerializeField] float idleBreathSpeed = 1.6f;
        [SerializeField] float idleBreathScale = 0.012f;

        [Header("Walk (sin root bob; lean suave)")]
        [SerializeField] float walkCycleSpeed = 7.2f;
        [SerializeField] float walkLeanDegrees = 3.2f;
        [SerializeField] float walkSquash = 0.018f;
        [SerializeField] float walkScarfSwayDegrees = 1.4f;

        [Header("Aire")]
        [SerializeField] float jumpStretch = 0.04f;
        [SerializeField] float jumpLeanDegrees = -5f;
        [SerializeField] float fallLeanDegrees = 4.5f;
        [SerializeField] float fallSquash = 0.02f;

        [Header("Landing")]
        [SerializeField] float landingSquash = 0.06f;

        AnimState state = AnimState.Idle;
        float cycle;
        float landingTimer;
        bool wasGrounded = true;
        Vector3 restLocalPosition;
        bool restPoseCached;

        public AnimState CurrentState => state;

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
            RemoveArticulatedRigIfPresent();
            ApplyApprovedSprite();
            CacheRestPose();
        }

        void LateUpdate()
        {
            if (visualRoot == null || controller == null || body == null)
            {
                return;
            }

            UpdateStateMachine();
            ApplyPose(Time.deltaTime);
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
                Debug.LogError($"{nameof(PlayerProtagonistAnimator)}: no se encontró el hijo Visual.", this);
                return;
            }

            if (visualRenderer == null)
            {
                visualRenderer = visualRoot.GetComponent<SpriteRenderer>();
            }
        }

        void RemoveArticulatedRigIfPresent()
        {
            if (visualRoot == null)
            {
                return;
            }

            Transform rig = visualRoot.Find(ArticulatedRigName);
            if (rig != null)
            {
                Destroy(rig.gameObject);
            }

            if (visualRenderer != null)
            {
                visualRenderer.enabled = true;
            }
        }

        void ApplyApprovedSprite()
        {
            if (visualRenderer == null)
            {
                return;
            }

            if (approvedSprite != null)
            {
                visualRenderer.sprite = approvedSprite;
            }

            visualRenderer.sortingLayerName = "Gameplay";
            if (visualRenderer.sortingOrder < 20)
            {
                visualRenderer.sortingOrder = 20;
            }
        }

        void CacheRestPose()
        {
            if (visualRoot == null)
            {
                return;
            }

            restLocalPosition = visualRoot.localPosition;
            restPoseCached = true;
        }

        void UpdateStateMachine()
        {
            bool grounded = controller.IsGrounded;
            float vy = body.linearVelocity.y;
            float speedX = Mathf.Abs(body.linearVelocity.x);

            if (!wasGrounded && grounded)
            {
                state = AnimState.Landing;
                landingTimer = landingDuration;
            }
            else if (state == AnimState.Landing)
            {
                landingTimer -= Time.deltaTime;
                if (landingTimer <= 0f)
                {
                    state = grounded
                        ? (speedX > walkSpeedThreshold ? AnimState.Walk : AnimState.Idle)
                        : (vy > jumpVelocityThreshold ? AnimState.Jump : AnimState.Fall);
                }
            }
            else if (!grounded)
            {
                state = vy > jumpVelocityThreshold ? AnimState.Jump : AnimState.Fall;
            }
            else
            {
                state = speedX > walkSpeedThreshold ? AnimState.Walk : AnimState.Idle;
            }

            wasGrounded = grounded;
        }

        void ApplyPose(float dt)
        {
            // Pivot inferior: no traducimos Y en suelo (evita flotar / root motion visual).
            if (restPoseCached)
            {
                visualRoot.localPosition = restLocalPosition;
            }

            float scaleX = 1f;
            float scaleY = 1f;
            float angleZ = 0f;

            switch (state)
            {
                case AnimState.Idle:
                {
                    float breath = Mathf.Sin(Time.time * idleBreathSpeed);
                    // Crece hacia arriba desde los pies; volumen casi constante.
                    scaleY = 1f + breath * idleBreathScale;
                    scaleX = 1f - breath * idleBreathScale * 0.45f;
                    angleZ = breath * 0.35f;
                    break;
                }
                case AnimState.Walk:
                {
                    float speedFactor = Mathf.Clamp(Mathf.Abs(body.linearVelocity.x) / 7f, 0.5f, 1.25f);
                    cycle += dt * walkCycleSpeed * speedFactor;
                    float swing = Mathf.Sin(cycle);
                    float contact = Mathf.Abs(Mathf.Sin(cycle)); // apoyo: más squash al contacto
                    // Lean “hacia adelante” en espacio del arte (sprite mira a la derecha).
                    angleZ = -walkLeanDegrees * (0.55f + 0.45f * swing) + swing * walkScarfSwayDegrees * 0.25f;
                    scaleY = 1f - contact * walkSquash;
                    scaleX = 1f + contact * walkSquash * 0.55f;
                    break;
                }
                case AnimState.Jump:
                {
                    scaleY = 1f + jumpStretch;
                    scaleX = 1f - jumpStretch * 0.5f;
                    angleZ = jumpLeanDegrees;
                    break;
                }
                case AnimState.Fall:
                {
                    scaleY = 1f - fallSquash;
                    scaleX = 1f + fallSquash * 0.45f;
                    angleZ = fallLeanDegrees;
                    break;
                }
                case AnimState.Landing:
                {
                    float t = 1f - Mathf.Clamp01(landingTimer / Mathf.Max(0.01f, landingDuration));
                    float pulse = Mathf.Sin(t * Mathf.PI);
                    scaleY = 1f - pulse * landingSquash;
                    scaleX = 1f + pulse * landingSquash * 0.7f;
                    angleZ = pulse * 1.2f;
                    break;
                }
            }

            visualRoot.localRotation = Quaternion.Euler(0f, 0f, angleZ);
            // Facing = SpriteRenderer.flipX (controller). Aquí solo squash/stretch.
            visualRoot.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
