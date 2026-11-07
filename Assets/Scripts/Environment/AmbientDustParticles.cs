using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Partículas ambientales sutiles (polvo/motas) ancladas a la cámara.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class AmbientDustParticles : MonoBehaviour
    {
        [SerializeField] Transform followTarget;
        [SerializeField] Vector3 localOffset = new Vector3(0f, 1.5f, 5f);

        ParticleSystem particles;

        void Awake()
        {
            particles = GetComponent<ParticleSystem>();
            if (followTarget == null && Camera.main != null)
            {
                followTarget = Camera.main.transform;
            }
        }

        void LateUpdate()
        {
            if (followTarget == null)
            {
                return;
            }

            transform.position = followTarget.position + localOffset;
        }

        public void Configure(Transform cam, Vector3 offset)
        {
            followTarget = cam;
            localOffset = offset;
        }
    }
}
