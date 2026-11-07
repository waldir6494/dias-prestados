using UnityEngine;

namespace DiasPrestados.Companions
{
    /// <summary>
    /// Animación por fotogramas del hermano pequeño según el estado del CompanionFollower.
    /// Sin Animator: los fotogramas se asignan en el inspector (generados por el builder del Editor).
    /// </summary>
    public class CompanionSpriteAnimator : MonoBehaviour
    {
        [SerializeField] CompanionFollower follower;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] float visualScale = 0.7f;

        [Header("Fotogramas")]
        [SerializeField] Sprite[] idle;
        [SerializeField] Sprite[] walk;
        [SerializeField] Sprite[] jump;
        [SerializeField] Sprite[] fall;
        [SerializeField] Sprite[] ask;
        [SerializeField] Sprite[] cheer;

        [Header("Velocidades (fotogramas/s)")]
        [SerializeField] float idleFps = 1.6f;
        [SerializeField] float walkFpsMin = 8f;
        [SerializeField] float walkFpsMax = 15f;
        [SerializeField] float askFps = 4.5f;
        [SerializeField] float cheerFps = 6f;

        float clock;
        Sprite[] lastSet;

        void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (follower == null)
            {
                follower = GetComponentInParent<CompanionFollower>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.transform.localScale = new Vector3(visualScale, visualScale, 1f);
            }
        }

        void LateUpdate()
        {
            if (follower == null || spriteRenderer == null)
            {
                return;
            }

            Sprite[] set;
            float fps;
            Vector2 v = follower.Velocity;

            switch (follower.Mode)
            {
                case CompanionMode.Asking:
                    set = ask;
                    fps = askFps;
                    break;
                case CompanionMode.Boosted:
                case CompanionMode.Cheering:
                    set = cheer;
                    fps = cheerFps;
                    break;
                default:
                    if (!follower.IsGrounded)
                    {
                        set = v.y > 0.5f ? jump : fall;
                        fps = 1f;
                    }
                    else if (Mathf.Abs(v.x) > 0.5f)
                    {
                        set = walk;
                        fps = Mathf.Lerp(walkFpsMin, walkFpsMax, Mathf.Clamp01(Mathf.Abs(v.x) / 8f));
                    }
                    else
                    {
                        set = idle;
                        fps = idleFps;
                    }

                    break;
            }

            if (set == null || set.Length == 0)
            {
                return;
            }

            if (set != lastSet)
            {
                lastSet = set;
                clock = 0f;
            }

            clock += Time.deltaTime * fps;
            spriteRenderer.sprite = set[Mathf.FloorToInt(clock) % set.Length];
            spriteRenderer.flipX = !follower.FacingRight;
        }
    }
}
