using System;
using UnityEngine;

namespace DiasPrestados.Companions
{
    public enum CompanionMode
    {
        Following,
        Asking,
        Boosted,
        Cheering
    }

    /// <summary>
    /// Hermano pequeño: sigue al protagonista, salta pequeños obstáculos y huecos, copia sus saltos,
    /// pide ayuda ante obstáculos altos (zona de ayuda) y nunca queda bloqueado de forma permanente
    /// (se recoloca junto al protagonista si cae, se queda muy atrás o permanece atascado).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class CompanionFollower : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] Transform leader;
        [SerializeField] Transform groundCheck;
        [Tooltip("Capas consideradas suelo/obstáculo (excluir la capa del propio compañero).")]
        [SerializeField] LayerMask groundLayers = ~0;

        [Header("Seguimiento")]
        [SerializeField] float followDistance = 2.2f;
        [SerializeField] float settleTolerance = 0.7f;
        [SerializeField] float walkSpeed = 6.2f;
        [SerializeField] float catchUpSpeed = 8.4f;
        [SerializeField] float catchUpGap = 5f;
        [SerializeField] float acceleration = 45f;
        [SerializeField] float deceleration = 60f;

        [Header("Salto")]
        [SerializeField] float jumpSpeed = 10.5f;
        [SerializeField] float maxStepHeight = 1.5f;
        [SerializeField] float jumpCooldown = 0.4f;
        [SerializeField] float copyLeaderJumpRange = 3.2f;

        [Header("Ayuda (obstáculos altos)")]
        [SerializeField] float helpBlockTime = 0.5f;
        [SerializeField] float boostDuration = 0.85f;
        [SerializeField] float cheerDuration = 1.1f;

        [Header("Recuperación (anti-bloqueo)")]
        [SerializeField] float teleportDistance = 18f;
        [SerializeField] float killY = -5.5f;
        [SerializeField] float blockedTeleportTime = 6f;

        Rigidbody2D rb;
        CapsuleCollider2D body;
        Collider2D leaderCollider;
        Rigidbody2D leaderBody;
        PlayerController2D leaderController;

        readonly RaycastHit2D[] rayHits = new RaycastHit2D[8];
        readonly Collider2D[] overlapHits = new Collider2D[8];
        ContactFilter2D filter;

        CompanionMode mode = CompanionMode.Following;
        CompanionHelpZone activeZone;
        float dirInput;
        float speedTarget;
        bool moving;
        bool facingRight = true;
        bool grounded;
        bool pendingJump;
        float pendingJumpSpeed;
        float jumpCooldownTimer;
        float blockedTimer;
        float copyJumpTimer = -1f;
        bool leaderWasGrounded = true;
        float cheerTimer;
        float baseGravity = 1f;

        Vector2 boostStart;
        Vector2 boostEnd;
        float boostT;
        float boostArc;
        Vector2 lastSafePosition;

        public event Action BoostStarted;
        public event Action BoostFinished;

        public CompanionMode Mode => mode;
        public bool IsWaitingForHelp => mode == CompanionMode.Asking;
        public bool FacingRight => facingRight;
        public bool IsGrounded => grounded;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public float HalfHeight => body != null ? body.size.y * 0.5f : 0.75f;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<CapsuleCollider2D>();

            baseGravity = rb.gravityScale;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayers);

            if (groundCheck == null)
            {
                var gc = new GameObject("GroundCheck");
                gc.transform.SetParent(transform, false);
                gc.transform.localPosition = new Vector3(0f, -HalfHeight, 0f);
                groundCheck = gc.transform;
            }

            if (leader != null)
            {
                leaderCollider = leader.GetComponent<Collider2D>();
                leaderBody = leader.GetComponent<Rigidbody2D>();
                leaderController = leader.GetComponent<PlayerController2D>();

                // El compañero nunca empuja ni se sube al protagonista.
                if (leaderCollider != null)
                {
                    Physics2D.IgnoreCollision(body, leaderCollider, true);
                }
            }

            lastSafePosition = rb.position;
        }

        void Update()
        {
            if (leader == null || mode == CompanionMode.Boosted)
            {
                return;
            }

            jumpCooldownTimer -= Time.deltaTime;
            UpdateGrounded();

            switch (mode)
            {
                case CompanionMode.Following:
                    DecideFollow();
                    break;
                case CompanionMode.Asking:
                    DecideAsking();
                    break;
                case CompanionMode.Cheering:
                    dirInput = 0f;
                    cheerTimer -= Time.deltaTime;
                    if (cheerTimer <= 0f)
                    {
                        mode = CompanionMode.Following;
                    }

                    break;
            }

            if (grounded && mode == CompanionMode.Following)
            {
                lastSafePosition = rb.position;
            }

            CheckRecovery();
        }

        void FixedUpdate()
        {
            if (mode == CompanionMode.Boosted)
            {
                StepBoost(Time.fixedDeltaTime);
                return;
            }

            float target = mode == CompanionMode.Following ? dirInput * speedTarget : 0f;
            float current = rb.linearVelocity.x;
            float accel = Mathf.Abs(target) > 0.01f ? acceleration : deceleration;
            if (!grounded)
            {
                accel *= 0.7f;
            }

            float vx = Mathf.MoveTowards(current, target, accel * Time.fixedDeltaTime);
            float vy = rb.linearVelocity.y;

            if (pendingJump)
            {
                vy = pendingJumpSpeed;
                pendingJump = false;
                grounded = false;
            }

            // Sin fricción en los colliders: si está quieto sobre el suelo no debe resbalar por pendientes.
            bool hold = grounded && Mathf.Abs(target) < 0.01f && Mathf.Abs(vx) < 0.05f && vy <= 0.5f;
            if (hold)
            {
                vx = 0f;
                vy = 0f;
            }

            rb.gravityScale = hold ? 0f : baseGravity;
            rb.linearVelocity = new Vector2(vx, vy);
        }

        // ------------------------------------------------------------------
        // Decisión de seguimiento
        // ------------------------------------------------------------------

        void DecideFollow()
        {
            float dx = leader.position.x - transform.position.x;
            float absDx = Mathf.Abs(dx);
            float dir = dx >= 0f ? 1f : -1f;
            float gap = absDx - followDistance;

            if (!moving && gap > settleTolerance)
            {
                moving = true;
            }
            else if (moving && gap <= 0.1f)
            {
                moving = false;
            }

            dirInput = moving ? dir : 0f;
            float maxSpeed = gap > catchUpGap ? catchUpSpeed : walkSpeed;
            speedTarget = Mathf.Clamp(gap * 2.4f, 1.4f, maxSpeed);

            if (moving)
            {
                facingRight = dir > 0f;
            }
            else if (absDx > 0.4f)
            {
                facingRight = dx > 0f; // quieto: mira al hermano mayor
            }

            bool wallBlocked = false;
            if (grounded && jumpCooldownTimer <= 0f)
            {
                bool wantJump = false;
                float jumpScale = 1f;

                if (moving)
                {
                    if (ProbeWall(dir, out bool clearable))
                    {
                        if (clearable)
                        {
                            wantJump = true;
                        }
                        else
                        {
                            wallBlocked = true;
                        }
                    }
                    else if (ProbeGap(dir) && leader.position.y > transform.position.y - 2f)
                    {
                        wantJump = true;
                    }
                }

                UpdateCopyJump(absDx);
                if (!wantJump && copyJumpTimer >= 0f)
                {
                    copyJumpTimer -= Time.deltaTime;
                    if (copyJumpTimer < 0f)
                    {
                        wantJump = true;
                        jumpScale = 0.8f;
                        copyJumpTimer = -1f;
                    }
                }

                if (wantJump)
                {
                    RequestJump(jumpScale);
                }
            }
            else
            {
                UpdateCopyJump(absDx);
            }

            if (wallBlocked)
            {
                blockedTimer += Time.deltaTime;
                if (blockedTimer >= helpBlockTime)
                {
                    CompanionHelpZone zone = CompanionHelpZone.Find(transform.position);
                    if (zone != null)
                    {
                        EnterAsking(zone);
                    }
                }
            }
            else if (!moving || grounded)
            {
                blockedTimer = 0f;
            }
        }

        void UpdateCopyJump(float absDx)
        {
            bool leaderGrounded = leaderController != null ? leaderController.IsGrounded : true;
            if (leaderWasGrounded && !leaderGrounded && leaderBody != null && leaderBody.linearVelocity.y > 3f
                && absDx < copyLeaderJumpRange && grounded)
            {
                copyJumpTimer = 0.18f;
            }

            leaderWasGrounded = leaderGrounded;
        }

        void RequestJump(float scale)
        {
            pendingJump = true;
            pendingJumpSpeed = jumpSpeed * scale;
            jumpCooldownTimer = jumpCooldown;
        }

        void DecideAsking()
        {
            dirInput = 0f;
            moving = false;

            if (activeZone != null)
            {
                facingRight = activeZone.LandingPosition.x > transform.position.x;
            }

            float dx = leader.position.x - transform.position.x;
            bool leaderAtMyLevelAndAway = leader.position.y < transform.position.y + 0.3f
                                          && Mathf.Abs(dx) > followDistance + 3f;
            if (leaderAtMyLevelAndAway || activeZone == null)
            {
                mode = CompanionMode.Following;
                blockedTimer = 0f;
            }
        }

        void EnterAsking(CompanionHelpZone zone)
        {
            activeZone = zone;
            mode = CompanionMode.Asking;
            dirInput = 0f;
            moving = false;
        }

        // ------------------------------------------------------------------
        // Interacción con el hermano mayor: impulso hasta el borde
        // ------------------------------------------------------------------

        public bool BeginBoost()
        {
            if (mode != CompanionMode.Asking || activeZone == null)
            {
                return false;
            }

            boostStart = rb.position;
            Vector3 land = activeZone.LandingPosition;
            boostEnd = new Vector2(land.x, land.y + HalfHeight + 0.05f);
            boostT = 0f;
            boostArc = Mathf.Max(1.5f, (boostEnd.y - boostStart.y) * 0.9f + 0.8f);
            facingRight = boostEnd.x > boostStart.x;

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            mode = CompanionMode.Boosted;
            BoostStarted?.Invoke();
            return true;
        }

        void StepBoost(float dt)
        {
            boostT = Mathf.Min(1f, boostT + dt / Mathf.Max(0.1f, boostDuration));
            float eased = Mathf.SmoothStep(0f, 1f, boostT);
            Vector2 p = Vector2.Lerp(boostStart, boostEnd, eased);
            p.y += 4f * boostArc * boostT * (1f - boostT);
            rb.MovePosition(p);

            if (boostT >= 1f)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
                mode = CompanionMode.Cheering;
                cheerTimer = cheerDuration;
                activeZone = null;
                blockedTimer = 0f;
                lastSafePosition = boostEnd;
                BoostFinished?.Invoke();
            }
        }

        // ------------------------------------------------------------------
        // Sondeos de entorno
        // ------------------------------------------------------------------

        void UpdateGrounded()
        {
            grounded = false;
            Vector2 origin = groundCheck != null ? (Vector2)groundCheck.position : rb.position + Vector2.down * HalfHeight;
            int count = Physics2D.OverlapBox(origin, new Vector2(0.36f, 0.12f), 0f, filter, overlapHits);
            for (int i = 0; i < count; i++)
            {
                if (IsSelfOrLeader(overlapHits[i]))
                {
                    continue;
                }

                grounded = true;
                break;
            }

            if (grounded && rb.linearVelocity.y > 0.5f)
            {
                grounded = false;
            }
        }

        bool IsSelfOrLeader(Collider2D c)
        {
            return c == null || c == body || c.transform.IsChildOf(transform)
                   || (leader != null && c.transform.IsChildOf(leader));
        }

        bool Cast(Vector2 origin, Vector2 direction, float distance, out RaycastHit2D hit)
        {
            int count = Physics2D.Raycast(origin, direction, filter, rayHits, distance);
            for (int i = 0; i < count; i++)
            {
                if (!IsSelfOrLeader(rayHits[i].collider))
                {
                    hit = rayHits[i];
                    return true;
                }
            }

            hit = default;
            return false;
        }

        bool ProbeWall(float dir, out bool clearable)
        {
            clearable = false;
            float halfW = body.size.x * 0.5f;
            Vector2 pos = rb.position;
            Vector2 knee = pos + new Vector2(dir * halfW, -HalfHeight + 0.3f);
            if (!Cast(knee, new Vector2(dir, 0f), 0.35f, out RaycastHit2D hit) || Mathf.Abs(hit.normal.y) > 0.65f)
            {
                return false;
            }

            Vector2 high = pos + new Vector2(dir * halfW, -HalfHeight + maxStepHeight);
            clearable = !Cast(high, new Vector2(dir, 0f), 0.6f, out _);
            return true;
        }

        bool ProbeGap(float dir)
        {
            float halfW = body.size.x * 0.5f;
            Vector2 origin = rb.position + new Vector2(dir * (halfW + 0.45f), -HalfHeight + 0.15f);
            return !Cast(origin, Vector2.down, 1.5f, out _);
        }

        // ------------------------------------------------------------------
        // Recuperación
        // ------------------------------------------------------------------

        void CheckRecovery()
        {
            Vector2 pos = rb.position;
            float distance = Vector2.Distance(pos, leader.position);

            if (pos.y < killY)
            {
                TeleportBesideLeader();
                return;
            }

            if (distance > teleportDistance && !IsOnScreen(pos))
            {
                TeleportBesideLeader();
                return;
            }

            if (blockedTimer > blockedTeleportTime && mode == CompanionMode.Following
                && (!IsOnScreen(pos) || blockedTimer > blockedTeleportTime * 2f))
            {
                TeleportBesideLeader();
            }
        }

        static bool IsOnScreen(Vector2 world)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return false;
            }

            Vector3 vp = cam.WorldToViewportPoint(world);
            return vp.x > -0.05f && vp.x < 1.05f && vp.y > -0.1f && vp.y < 1.1f;
        }

        public void TeleportBesideLeader()
        {
            if (leader == null)
            {
                return;
            }

            bool leaderFacingRight = leaderController == null || leaderController.FacingRight;
            float x = leader.position.x - (leaderFacingRight ? 1f : -1f) * followDistance;
            Vector2 target = new Vector2(x, leader.position.y);

            if (Cast(new Vector2(x, leader.position.y + 3f), Vector2.down, 8f, out RaycastHit2D hit))
            {
                target = new Vector2(x, hit.point.y + HalfHeight + 0.05f);
            }

            if (rb.bodyType != RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            rb.position = target;
            rb.linearVelocity = Vector2.zero;
            mode = CompanionMode.Following;
            activeZone = null;
            blockedTimer = 0f;
            moving = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, followDistance);
        }
    }
}
