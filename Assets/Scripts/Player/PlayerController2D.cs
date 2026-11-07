using UnityEngine;
using UnityEngine.InputSystem;

namespace DiasPrestados
{
    /// <summary>
    /// Controlador 2D de prototipo: movimiento suave, salto y detección de suelo.
    /// Usa exclusivamente el Input System y Rigidbody2D.linearVelocity (Unity 6).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] float moveSpeed = 7f;
        [SerializeField] float acceleration = 70f;
        [SerializeField] float deceleration = 85f;
        [SerializeField] float airControlMultiplier = 0.7f;

        [Header("Salto")]
        [SerializeField] float jumpForce = 13f;
        [SerializeField] float coyoteTime = 0.1f;
        [SerializeField] float jumpBufferTime = 0.12f;
        [SerializeField] float fallGravityMultiplier = 1.35f;
        [SerializeField] float lowJumpGravityMultiplier = 1.8f;

        [Header("Suelo")]
        [SerializeField] Transform groundCheck;
        [SerializeField] Vector2 groundCheckSize = new Vector2(0.42f, 0.12f);
        [SerializeField] LayerMask groundLayers = ~0;
        [SerializeField] float groundedSkin = 0.02f;

        [Header("Pendientes")]
        [Tooltip("Con colliders sin fricción, evita que el personaje resbale por pendientes cuando está parado.")]
        [SerializeField] bool holdOnSlopesWhenIdle = false;

        [Header("Input")]
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] string actionMapName = "Player";
        [SerializeField] string moveActionName = "Move";
        [SerializeField] string jumpActionName = "Jump";

        Rigidbody2D rb;
        Collider2D bodyCollider;
        Transform visualRoot;
        SpriteRenderer visualRenderer;
        InputActionMap playerMap;
        InputAction moveAction;
        InputAction jumpAction;

        float moveInput;
        float coyoteTimer;
        float jumpBufferTimer;
        bool jumpHeld;
        bool facingRight = true;
        float baseGravityScale;

        public bool IsGrounded { get; private set; }
        public float MoveInput => moveInput;
        public bool FacingRight => facingRight;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<Collider2D>();
            ResolveVisualRoot();

            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            baseGravityScale = rb.gravityScale;

            ResolveGroundCheck();
            BindInputActions();
        }

        void ResolveVisualRoot()
        {
            visualRoot = transform.Find("Visual");
            if (visualRoot != null)
            {
                visualRenderer = visualRoot.GetComponent<SpriteRenderer>();
                return;
            }

            visualRenderer = GetComponentInChildren<SpriteRenderer>();
            if (visualRenderer != null)
            {
                visualRoot = visualRenderer.transform;
            }
        }

        void OnEnable()
        {
            if (playerMap == null)
            {
                BindInputActions();
            }

            if (playerMap != null)
            {
                playerMap.Enable();
            }

            if (jumpAction != null)
            {
                jumpAction.started += OnJumpStarted;
                jumpAction.canceled += OnJumpCanceled;
            }
        }

        void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.started -= OnJumpStarted;
                jumpAction.canceled -= OnJumpCanceled;
            }

            playerMap?.Disable();
        }

        void Update()
        {
            ReadMoveInput();
            UpdateGroundedState();
            UpdateTimers();
            TryJump();
            UpdateFacing();
            ApplyVariableGravity();
        }

        void FixedUpdate()
        {
            ApplyHorizontalMovement();
        }

        void BindInputActions()
        {
            if (inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerController2D)} en '{name}' no tiene InputActionAsset asignado.", this);
                return;
            }

            playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
            if (playerMap == null)
            {
                Debug.LogError($"No se encontró el Action Map '{actionMapName}'.", this);
                return;
            }

            moveAction = playerMap.FindAction(moveActionName, throwIfNotFound: false);
            jumpAction = playerMap.FindAction(jumpActionName, throwIfNotFound: false);

            if (moveAction == null || jumpAction == null)
            {
                Debug.LogError("Faltan acciones Move o Jump en el InputActionAsset.", this);
            }
        }

        void ResolveGroundCheck()
        {
            if (groundCheck != null)
            {
                return;
            }

            var existing = transform.Find("GroundCheck");
            if (existing != null)
            {
                groundCheck = existing;
                return;
            }

            var check = new GameObject("GroundCheck");
            check.transform.SetParent(transform, false);
            check.transform.localPosition = new Vector3(0f, -0.92f, 0f);
            groundCheck = check.transform;
        }

        void ReadMoveInput()
        {
            if (moveAction == null)
            {
                moveInput = 0f;
                return;
            }

            moveInput = moveAction.ReadValue<Vector2>().x;
            if (Mathf.Abs(moveInput) < 0.1f)
            {
                moveInput = 0f;
            }
        }

        void OnJumpStarted(InputAction.CallbackContext _)
        {
            jumpHeld = true;
            jumpBufferTimer = jumpBufferTime;
        }

        void OnJumpCanceled(InputAction.CallbackContext _)
        {
            jumpHeld = false;
        }

        void UpdateTimers()
        {
            if (IsGrounded)
            {
                coyoteTimer = coyoteTime;
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }

            jumpBufferTimer -= Time.deltaTime;
        }

        void UpdateGroundedState()
        {
            Vector2 origin = groundCheck != null
                ? (Vector2)groundCheck.position
                : (Vector2)bodyCollider.bounds.center + Vector2.down * (bodyCollider.bounds.extents.y - groundedSkin);

            IsGrounded = false;
            Collider2D[] hits = Physics2D.OverlapBoxAll(origin, groundCheckSize, 0f, groundLayers);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit == bodyCollider || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                IsGrounded = true;
                break;
            }

            // Evita marcar suelo mientras el salto todavía asciende con fuerza.
            if (IsGrounded && rb.linearVelocity.y > 0.5f)
            {
                IsGrounded = false;
            }
        }

        void TryJump()
        {
            if (jumpBufferTimer <= 0f || coyoteTimer <= 0f)
            {
                return;
            }

            Vector2 velocity = rb.linearVelocity;
            velocity.y = jumpForce;
            rb.linearVelocity = velocity;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            IsGrounded = false;
        }

        void ApplyHorizontalMovement()
        {
            float targetSpeed = moveInput * moveSpeed;
            float currentX = rb.linearVelocity.x;
            float accel = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

            if (!IsGrounded)
            {
                accel *= airControlMultiplier;
            }

            float newX = Mathf.MoveTowards(currentX, targetSpeed, accel * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

            if (holdOnSlopesWhenIdle && IsGrounded && moveInput == 0f
                && Mathf.Abs(newX) < 0.05f && rb.linearVelocity.y <= 0.5f)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }

        void ApplyVariableGravity()
        {
            if (IsGrounded)
            {
                rb.gravityScale = baseGravityScale;
                return;
            }

            if (rb.linearVelocity.y < 0f)
            {
                rb.gravityScale = baseGravityScale * fallGravityMultiplier;
            }
            else if (!jumpHeld)
            {
                rb.gravityScale = baseGravityScale * lowJumpGravityMultiplier;
            }
            else
            {
                rb.gravityScale = baseGravityScale;
            }
        }

        void UpdateFacing()
        {
            if (moveInput > 0.1f && !facingRight)
            {
                facingRight = true;
                SetFacing(1f);
            }
            else if (moveInput < -0.1f && facingRight)
            {
                facingRight = false;
                SetFacing(-1f);
            }
        }

        void SetFacing(float direction)
        {
            // flipX en el sprite: no toca Rigidbody/Collider y no pelea con el squash del animador.
            if (visualRenderer != null)
            {
                visualRenderer.flipX = direction < 0f;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

    }
}
