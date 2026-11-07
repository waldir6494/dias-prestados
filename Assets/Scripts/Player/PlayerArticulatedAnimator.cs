using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Rig articulado 2D del protagonista: Idle, Walk/Run, Jump, Fall, Landing.
    /// Anima piezas reales (no un sprite monolítico estático). El arte de partes
    /// es placeholder funcional; el arte definitivo queda pendiente.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerArticulatedAnimator : MonoBehaviour
    {
        public enum AnimState
        {
            Idle,
            Walk,
            Jump,
            Fall,
            Landing
        }

        const string ResourcesFolder = "ProtagonistParts";
        const string RigRootName = "ArticulatedRig";

        [Header("Referencias")]
        [SerializeField] Transform visualRoot;
        [SerializeField] PlayerController2D controller;
        [SerializeField] Rigidbody2D body;

        [Header("Umbrales")]
        [SerializeField] float walkSpeedThreshold = 0.35f;
        [SerializeField] float jumpVelocityThreshold = 0.6f;
        [SerializeField] float fallVelocityThreshold = -0.15f;
        [SerializeField] float landingDuration = 0.18f;

        [Header("Walk")]
        [SerializeField] float walkCycleSpeed = 9.5f;
        [SerializeField] float legSwingDegrees = 28f;
        [SerializeField] float armSwingDegrees = 22f;
        [SerializeField] float torsoBobDegrees = 3.5f;
        [SerializeField] float headBobDegrees = 2.5f;

        [Header("Idle")]
        [SerializeField] float idleBreathSpeed = 2.2f;
        [SerializeField] float idleBreathDegrees = 2f;

        [Header("Aire")]
        [SerializeField] float jumpArmDegrees = -28f;
        [SerializeField] float jumpLegDegrees = -12f;
        [SerializeField] float fallArmDegrees = 18f;
        [SerializeField] float fallLegDegrees = 22f;

        [Header("Landing")]
        [SerializeField] float landingSquashDegrees = 16f;

        [Header("Sorting")]
        [SerializeField] string sortingLayerName = "Gameplay";
        [SerializeField] int sortingOrderBase = 20;

        AnimState state = AnimState.Idle;
        float cycle;
        float landingTimer;
        bool wasGrounded = true;

        Transform hip;
        Transform torso;
        Transform head;
        Transform scarf;
        Transform backpack;
        Transform upperArmFront;
        Transform forearmFront;
        Transform upperArmBack;
        Transform forearmBack;
        Transform upperLegFront;
        Transform lowerLegFront;
        Transform bootFront;
        Transform upperLegBack;
        Transform lowerLegBack;
        Transform bootBack;

        SpriteRenderer staticVisualRenderer;
        bool rigReady;

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

            ResolveVisualRoot();
            EnsureRig();
        }

        void LateUpdate()
        {
            if (!rigReady || controller == null || body == null)
            {
                return;
            }

            UpdateState();
            ApplyPose(Time.deltaTime);
        }

        void ResolveVisualRoot()
        {
            if (visualRoot != null)
            {
                return;
            }

            Transform found = transform.Find("Visual");
            visualRoot = found != null ? found : transform;
        }

        void EnsureRig()
        {
            if (visualRoot == null)
            {
                return;
            }

            staticVisualRenderer = visualRoot.GetComponent<SpriteRenderer>();
            Transform existing = visualRoot.Find(RigRootName);
            if (existing != null)
            {
                CacheBones(existing);
                HideStaticSprite();
                rigReady = hip != null;
                return;
            }

            if (!TryLoadSprites(out var sprites))
            {
                Debug.LogError($"{nameof(PlayerArticulatedAnimator)}: no se pudieron cargar sprites en Resources/{ResourcesFolder}.", this);
                return;
            }

            var rig = new GameObject(RigRootName);
            rig.transform.SetParent(visualRoot, false);
            // El Visual ya está anclado a los pies (y ≈ -0.85). El hip queda cerca del suelo lógico.
            rig.transform.localPosition = Vector3.zero;
            // Escala aproximada al sprite de perfil (~1.7 u de alto con Visual en y=-0.85).
            rig.transform.localScale = Vector3.one * 1.05f;

            hip = CreatePart(rig.transform, "Hip", sprites.hip, new Vector3(0f, 0.78f, 0f), sortingOrderBase + 2);
            torso = CreatePart(hip, "Torso", sprites.torso, new Vector3(0.02f, 0.06f, 0f), sortingOrderBase + 4);
            head = CreatePart(torso, "Head", sprites.head, new Vector3(0.04f, 0.52f, 0f), sortingOrderBase + 8);
            scarf = CreatePart(torso, "Scarf", sprites.scarf, new Vector3(0.06f, 0.42f, 0f), sortingOrderBase + 7);
            backpack = CreatePart(torso, "Backpack", sprites.backpack, new Vector3(-0.22f, 0.22f, 0f), sortingOrderBase + 3);

            upperArmBack = CreatePart(torso, "UpperArmBack", sprites.upperArm, new Vector3(-0.02f, 0.38f, 0f), sortingOrderBase + 1);
            forearmBack = CreatePart(upperArmBack, "ForearmBack", sprites.forearm, new Vector3(0f, -0.24f, 0f), sortingOrderBase + 1);
            upperArmFront = CreatePart(torso, "UpperArmFront", sprites.upperArm, new Vector3(0.1f, 0.36f, 0f), sortingOrderBase + 6);
            forearmFront = CreatePart(upperArmFront, "ForearmFront", sprites.forearm, new Vector3(0f, -0.24f, 0f), sortingOrderBase + 6);

            upperLegBack = CreatePart(hip, "UpperLegBack", sprites.upperLeg, new Vector3(-0.04f, -0.02f, 0f), sortingOrderBase + 0);
            lowerLegBack = CreatePart(upperLegBack, "LowerLegBack", sprites.lowerLeg, new Vector3(0f, -0.28f, 0f), sortingOrderBase + 0);
            bootBack = CreatePart(lowerLegBack, "BootBack", sprites.boot, new Vector3(0.02f, -0.22f, 0f), sortingOrderBase + 0);

            upperLegFront = CreatePart(hip, "UpperLegFront", sprites.upperLeg, new Vector3(0.06f, -0.02f, 0f), sortingOrderBase + 5);
            lowerLegFront = CreatePart(upperLegFront, "LowerLegFront", sprites.lowerLeg, new Vector3(0f, -0.28f, 0f), sortingOrderBase + 5);
            bootFront = CreatePart(lowerLegFront, "BootFront", sprites.boot, new Vector3(0.02f, -0.22f, 0f), sortingOrderBase + 5);

            HideStaticSprite();
            rigReady = true;
        }

        void HideStaticSprite()
        {
            if (staticVisualRenderer != null)
            {
                staticVisualRenderer.enabled = false;
            }
        }

        void CacheBones(Transform rig)
        {
            hip = rig.Find("Hip");
            torso = hip != null ? hip.Find("Torso") : null;
            head = torso != null ? torso.Find("Head") : null;
            scarf = torso != null ? torso.Find("Scarf") : null;
            backpack = torso != null ? torso.Find("Backpack") : null;
            upperArmFront = torso != null ? torso.Find("UpperArmFront") : null;
            forearmFront = upperArmFront != null ? upperArmFront.Find("ForearmFront") : null;
            upperArmBack = torso != null ? torso.Find("UpperArmBack") : null;
            forearmBack = upperArmBack != null ? upperArmBack.Find("ForearmBack") : null;
            upperLegFront = hip != null ? hip.Find("UpperLegFront") : null;
            lowerLegFront = upperLegFront != null ? upperLegFront.Find("LowerLegFront") : null;
            bootFront = lowerLegFront != null ? lowerLegFront.Find("BootFront") : null;
            upperLegBack = hip != null ? hip.Find("UpperLegBack") : null;
            lowerLegBack = upperLegBack != null ? upperLegBack.Find("LowerLegBack") : null;
            bootBack = lowerLegBack != null ? lowerLegBack.Find("BootBack") : null;
        }

        struct PartSprites
        {
            public Sprite head;
            public Sprite torso;
            public Sprite scarf;
            public Sprite backpack;
            public Sprite upperArm;
            public Sprite forearm;
            public Sprite upperLeg;
            public Sprite lowerLeg;
            public Sprite boot;
            public Sprite hip;
        }

        static bool TryLoadSprites(out PartSprites sprites)
        {
            sprites = default;
            sprites.head = LoadPartSprite("part_head");
            sprites.torso = LoadPartSprite("part_torso");
            sprites.scarf = LoadPartSprite("part_scarf");
            sprites.backpack = LoadPartSprite("part_backpack");
            sprites.upperArm = LoadPartSprite("part_upper_arm");
            sprites.forearm = LoadPartSprite("part_forearm");
            sprites.upperLeg = LoadPartSprite("part_upper_leg");
            sprites.lowerLeg = LoadPartSprite("part_lower_leg");
            sprites.boot = LoadPartSprite("part_boot");
            sprites.hip = LoadPartSprite("part_hip");

            return sprites.head != null
                   && sprites.torso != null
                   && sprites.upperArm != null
                   && sprites.upperLeg != null
                   && sprites.boot != null;
        }

        static Sprite LoadPartSprite(string fileName)
        {
            string path = $"{ResourcesFolder}/{fileName}";
            Sprite direct = Resources.Load<Sprite>(path);
            if (direct != null)
            {
                return direct;
            }

            Sprite[] all = Resources.LoadAll<Sprite>(path);
            if (all != null && all.Length > 0)
            {
                return all[0];
            }

            return null;
        }

        Transform CreatePart(Transform parent, string name, Sprite sprite, Vector3 localPos, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;
            return go.transform;
        }

        void UpdateState()
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
                if (vy < fallVelocityThreshold)
                {
                    state = AnimState.Fall;
                }
            }
            else
            {
                state = speedX > walkSpeedThreshold ? AnimState.Walk : AnimState.Idle;
            }

            wasGrounded = grounded;
        }

        void ApplyPose(float dt)
        {
            float legF = 0f;
            float legB = 0f;
            float armF = 0f;
            float armB = 0f;
            float kneeF = 0f;
            float kneeB = 0f;
            float torsoZ = 0f;
            float headZ = 0f;
            const float hipRestY = 0.78f;
            float hipY = hipRestY;
            float scarfZ = 0f;

            switch (state)
            {
                case AnimState.Idle:
                {
                    float breath = Mathf.Sin(Time.time * idleBreathSpeed);
                    torsoZ = breath * idleBreathDegrees;
                    headZ = -breath * idleBreathDegrees * 0.6f;
                    scarfZ = breath * 4f;
                    armF = 6f + breath * 2f;
                    armB = -8f - breath * 2f;
                    legF = 4f;
                    legB = -4f;
                    break;
                }
                case AnimState.Walk:
                {
                    float speedFactor = Mathf.Clamp(Mathf.Abs(body.linearVelocity.x) / 7f, 0.45f, 1.35f);
                    cycle += dt * walkCycleSpeed * speedFactor;
                    float swing = Mathf.Sin(cycle);
                    float swingOpp = Mathf.Sin(cycle + Mathf.PI);
                    legF = swing * legSwingDegrees;
                    legB = swingOpp * legSwingDegrees;
                    kneeF = Mathf.Max(0f, -swing) * 28f;
                    kneeB = Mathf.Max(0f, -swingOpp) * 28f;
                    armF = swingOpp * armSwingDegrees;
                    armB = swing * armSwingDegrees;
                    torsoZ = swing * torsoBobDegrees;
                    headZ = -swing * headBobDegrees;
                    hipY = hipRestY + Mathf.Abs(swing) * 0.025f;
                    scarfZ = swing * 8f;
                    break;
                }
                case AnimState.Jump:
                {
                    armF = jumpArmDegrees;
                    armB = jumpArmDegrees * 0.75f;
                    legF = jumpLegDegrees;
                    legB = jumpLegDegrees * 1.2f;
                    kneeF = 18f;
                    kneeB = 10f;
                    torsoZ = -6f;
                    headZ = 4f;
                    scarfZ = -12f;
                    hipY = hipRestY + 0.02f;
                    break;
                }
                case AnimState.Fall:
                {
                    armF = fallArmDegrees;
                    armB = fallArmDegrees * 0.7f;
                    legF = fallLegDegrees * 0.4f;
                    legB = -fallLegDegrees * 0.35f;
                    kneeF = 8f;
                    kneeB = 14f;
                    torsoZ = 5f;
                    headZ = -3f;
                    scarfZ = 16f;
                    hipY = hipRestY - 0.01f;
                    break;
                }
                case AnimState.Landing:
                {
                    float t = 1f - Mathf.Clamp01(landingTimer / Mathf.Max(0.01f, landingDuration));
                    float squash = Mathf.Sin(t * Mathf.PI) * landingSquashDegrees;
                    legF = squash * 0.35f;
                    legB = squash * 0.25f;
                    kneeF = squash;
                    kneeB = squash * 0.8f;
                    armF = -squash * 0.5f;
                    armB = squash * 0.35f;
                    torsoZ = squash * 0.4f;
                    headZ = -squash * 0.25f;
                    hipY = hipRestY - squash * 0.004f;
                    scarfZ = squash * 0.5f;
                    break;
                }
            }

            SetLocalPosY(hip, hipY);
            SetLocalEuler(torso, torsoZ);
            SetLocalEuler(head, headZ);
            SetLocalEuler(scarf, scarfZ);
            SetLocalEuler(backpack, torsoZ * -0.25f);

            SetLocalEuler(upperArmFront, armF);
            SetLocalEuler(forearmFront, Mathf.Clamp(-armF * 0.35f, -20f, 25f));
            SetLocalEuler(upperArmBack, armB);
            SetLocalEuler(forearmBack, Mathf.Clamp(-armB * 0.35f, -20f, 25f));

            SetLocalEuler(upperLegFront, legF);
            SetLocalEuler(lowerLegFront, kneeF);
            SetLocalEuler(bootFront, Mathf.Clamp(-kneeF * 0.25f, -12f, 12f));
            SetLocalEuler(upperLegBack, legB);
            SetLocalEuler(lowerLegBack, kneeB);
            SetLocalEuler(bootBack, Mathf.Clamp(-kneeB * 0.25f, -12f, 12f));
        }

        static void SetLocalEuler(Transform t, float zDegrees)
        {
            if (t == null)
            {
                return;
            }

            t.localRotation = Quaternion.Euler(0f, 0f, zDegrees);
        }

        static void SetLocalPosY(Transform t, float y)
        {
            if (t == null)
            {
                return;
            }

            Vector3 p = t.localPosition;
            p.y = y;
            t.localPosition = p;
        }
    }
}
