using UnityEngine;

namespace DiasPrestados.Creatures
{
    /// <summary>
    /// Cabra de Chronoram en el plano de juego. Camina de un lado a otro,
    /// gira la cabeza y luego el cuerpo, y empuja a quien choque con ella.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class CabraWalker : MonoBehaviour
    {
        enum Phase
        {
            Walk,
            TurnHead,
            TurnBody
        }

        [SerializeField] CabraSpriteSet sprites;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Collider2D hermanoMayor;
        [SerializeField] Collider2D hermanoPequeno;

        [Header("Recorrido")]
        [Tooltip("El largo del recorrido es este múltiplo de la suma de los anchos X de los dos niños.")]
        [SerializeField] float routeMultiplier = 7f;
        [SerializeField] float walkSpeed = 1.2f;

        [Header("Fotogramas")]
        [SerializeField] float walkFps = 24f;
        [SerializeField] float turnHeadFps = 24f;
        [SerializeField] float turnBodyFps = 24f;
        [Tooltip("Si está activo, turn head y turn around se leen del último archivo al primero.")]
        [SerializeField] bool playTurnClipsFromLastFrame = false;
        [Tooltip("Fotograma de turn around (desde 0) por el que empieza el giro del cuerpo. 0 empalma con el final de turn head.")]
        [SerializeField] int turnAroundStartFrame = 0;
        [Tooltip("Cruce breve del paso al primer fotograma de girar la cabeza. Las dos poses son el mismo perfil.")]
        [SerializeField] float poseBlendSeconds = 0.12f;

        [Header("Suelo y empuje")]
        [SerializeField] LayerMask groundLayers = ~0;
        [SerializeField] float pushMass = 28f;
        [SerializeField] float snapHeight = 4f;

        Rigidbody2D rb;
        Collider2D body;
        Phase phase = Phase.Walk;
        bool facingRight = true;
        float clock;
        float minX;
        float maxX;
        float baseGravity;
        bool ready;
        SpriteRenderer blendRenderer;
        bool blending;
        float blendClock;
        Sprite blendFrom;
        bool blendFromFlip;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            body = GetComponent<Collider2D>();
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            rb.mass = pushMass;
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            baseGravity = rb.gravityScale;
        }

        void Start()
        {
            ready = sprites != null
                && sprites.walking != null && sprites.walking.Length > 0
                && sprites.turnHead != null && sprites.turnHead.Length > 0
                && sprites.turnAround != null && sprites.turnAround.Length > 0
                && spriteRenderer != null;

            if (!ready)
            {
                Debug.LogError("Cabra: faltan fotogramas o el SpriteRenderer.", this);
                enabled = false;
                return;
            }

            BuildRoute();
            facingRight = true;
            phase = Phase.Walk;
            clock = 0f;
            Vector2 grounded = new Vector2(minX, rb.position.y);
            rb.position = SnapFeet(grounded);
            rb.linearVelocity = Vector2.zero;
            ApplyFlip();
            ShowCurrent();
        }

        void BuildRoute()
        {
            if (hermanoMayor == null || hermanoPequeno == null)
            {
                minX = rb.position.x;
                maxX = minX + 8.4f;
                return;
            }

            float occupied = hermanoMayor.bounds.size.x + hermanoPequeno.bounds.size.x;
            float mid = (hermanoMayor.bounds.center.x + hermanoPequeno.bounds.center.x) * 0.5f;
            float half = occupied * routeMultiplier * 0.5f;
            minX = mid - half;
            maxX = mid + half;
        }

        void Update()
        {
            if (!ready || spriteRenderer == null)
            {
                return;
            }

            Sprite[] clip = CurrentClip();
            float fps = CurrentFps();
            if (clip == null || clip.Length == 0 || fps <= 0.01f)
            {
                return;
            }

            if (blending)
            {
                BlendPose();
                return;
            }

            clock += Time.deltaTime * fps;

            if (phase == Phase.Walk)
            {
                if (clock >= clip.Length)
                {
                    clock %= clip.Length;
                }

                Show(clip[Mathf.FloorToInt(clock) % clip.Length]);
                return;
            }

            int step = Mathf.FloorToInt(clock);
            int count = PlaybackCount(clip);

            if (step >= count)
            {
                Show(clip[PlaybackIndex(clip, count - 1)]);
                OnTurnClipFinished();
                return;
            }

            Show(clip[PlaybackIndex(clip, step)]);
        }

        int PlaybackStart(Sprite[] clip)
        {
            if (phase != Phase.TurnBody || playTurnClipsFromLastFrame || clip == null || clip.Length == 0)
            {
                return 0;
            }

            return Mathf.Clamp(turnAroundStartFrame, 0, clip.Length - 1);
        }

        int PlaybackCount(Sprite[] clip)
        {
            if (playTurnClipsFromLastFrame)
            {
                return clip.Length;
            }

            return clip.Length - PlaybackStart(clip);
        }

        int PlaybackIndex(Sprite[] clip, int step)
        {
            if (playTurnClipsFromLastFrame)
            {
                return clip.Length - 1 - step;
            }

            return PlaybackStart(clip) + step;
        }

        void FixedUpdate()
        {
            if (!ready)
            {
                return;
            }

            if (phase != Phase.Walk)
            {
                // No recolocar el cuerpo: el solver lo separaba del suelo y el
                // siguiente paso lo volvía a hundir, y la cabra temblaba en Y.
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                return;
            }

            rb.gravityScale = baseGravity;
            bool reached = facingRight ? rb.position.x >= maxX : rb.position.x <= minX;
            if (reached)
            {
                float x = facingRight ? maxX : minX;
                rb.position = new Vector2(x, rb.position.y);
                rb.linearVelocity = Vector2.zero;
                BeginHeadTurn();
                return;
            }

            float dir = facingRight ? 1f : -1f;
            rb.linearVelocity = new Vector2(dir * walkSpeed, rb.linearVelocity.y);
        }

        void BeginHeadTurn()
        {
            Sprite from = spriteRenderer.sprite;
            bool fromFlip = spriteRenderer.flipX;
            phase = Phase.TurnHead;
            clock = 0f;
            HoldInPlace();
            ApplyFlip();
            ShowCurrent();
            BeginPoseBlend(from, fromFlip);
        }

        void BeginPoseBlend(Sprite from, bool fromFlip)
        {
            if (from == null || poseBlendSeconds <= 0.01f || spriteRenderer.sprite == null)
            {
                return;
            }

            if (blendRenderer == null)
            {
                GameObject go = new GameObject("CabraBlend");
                go.transform.SetParent(spriteRenderer.transform, false);
                blendRenderer = go.AddComponent<SpriteRenderer>();
                blendRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
                blendRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;
                blendRenderer.sharedMaterial = spriteRenderer.sharedMaterial;
            }

            blendFrom = from;
            blendFromFlip = fromFlip;
            blending = true;
            blendClock = 0f;
            blendRenderer.sprite = from;
            blendRenderer.flipX = fromFlip;
            blendRenderer.color = Color.white;
            spriteRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        void BlendPose()
        {
            blendClock += Time.deltaTime;
            float t = Mathf.Clamp01(blendClock / poseBlendSeconds);
            blendRenderer.sprite = blendFrom;
            blendRenderer.flipX = blendFromFlip;
            blendRenderer.color = new Color(1f, 1f, 1f, 1f - t);
            spriteRenderer.color = new Color(1f, 1f, 1f, t);
            ApplyFlip();
            if (t < 1f)
            {
                return;
            }

            blending = false;
            spriteRenderer.color = Color.white;
            blendRenderer.sprite = null;
            blendRenderer.color = new Color(1f, 1f, 1f, 0f);
        }

        void HoldInPlace()
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePosition;
        }

        void ReleaseHold()
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = baseGravity;
            rb.linearVelocity = Vector2.zero;
        }

        void OnTurnClipFinished()
        {
            if (phase == Phase.TurnHead)
            {
                phase = Phase.TurnBody;
                // El clip de cuerpo, leído al revés, sale del perfil contrario.
                // El espejo nuevo hace que el primer fotograma coincida con la dirección actual
                // y que el último quede mirando hacia el lado al que va a caminar.
                if (playTurnClipsFromLastFrame)
                {
                    facingRight = !facingRight;
                }
            }
            else
            {
                if (!playTurnClipsFromLastFrame)
                {
                    facingRight = !facingRight;
                }

                phase = Phase.Walk;
                ReleaseHold();
            }

            clock = 0f;
            ApplyFlip();
            ShowCurrent();
        }

        Sprite[] CurrentClip()
        {
            switch (phase)
            {
                case Phase.TurnHead:
                    return sprites.turnHead;
                case Phase.TurnBody:
                    return sprites.turnAround;
                default:
                    return sprites.walking;
            }
        }

        float CurrentFps()
        {
            switch (phase)
            {
                case Phase.TurnHead:
                    return turnHeadFps;
                case Phase.TurnBody:
                    return turnBodyFps;
                default:
                    return walkFps;
            }
        }

        void ShowCurrent()
        {
            Sprite[] clip = CurrentClip();
            if (clip == null || clip.Length == 0)
            {
                return;
            }

            Show(clip[PlaybackIndex(clip, 0)]);
        }

        void Show(Sprite sprite)
        {
            if (sprite == null || spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = sprite;
            ApplyFlip();
        }

        void ApplyFlip()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }
        }

        Vector2 SnapFeet(Vector2 feet)
        {
            Vector2 origin = feet + Vector2.up * snapHeight;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, snapHeight + 2f, groundLayers);
            float best = float.MaxValue;
            bool found = false;
            float groundY = feet.y;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hitBody = hits[i].collider;
                if (hitBody == null || hitBody == body || hitBody.isTrigger || hitBody.attachedRigidbody != null)
                {
                    continue;
                }

                if (hits[i].distance < best)
                {
                    best = hits[i].distance;
                    groundY = hits[i].point.y;
                    found = true;
                }
            }

            if (!found)
            {
                return feet;
            }

            return new Vector2(feet.x, groundY + 0.01f);
        }
    }
}
