using UnityEngine;
using UnityEngine.Rendering.Universal;
using DiasPrestados.Companions;

namespace DiasPrestados.Environment
{
    /// <summary>
    /// Destello cálido breve sobre el hermano pequeño cuando recibe ayuda (feedback emocional sutil).
    /// </summary>
    public class CompanionJoyGlow : MonoBehaviour
    {
        [SerializeField] CompanionFollower follower;
        [SerializeField] Light2D glow;
        [SerializeField] float peakIntensity = 1.1f;
        [SerializeField] float duration = 1.6f;

        float timer;

        void OnEnable()
        {
            if (follower != null)
            {
                follower.BoostFinished += OnBoostFinished;
            }
        }

        void OnDisable()
        {
            if (follower != null)
            {
                follower.BoostFinished -= OnBoostFinished;
            }
        }

        void OnBoostFinished()
        {
            timer = duration;
        }

        void Update()
        {
            if (glow == null)
            {
                return;
            }

            timer = Mathf.Max(0f, timer - Time.deltaTime);
            float t = duration > 0f ? timer / duration : 0f;
            glow.intensity = peakIntensity * t * t;
        }
    }
}
