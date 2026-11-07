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
            EnsureCompatibleVelocityModes(particles);
            if (followTarget == null && Camera.main != null)
            {
                followTarget = Camera.main.transform;
            }
        }

        /// <summary>
        /// Evita "Particle Velocity curves must all be in the same mode"
        /// cuando algún eje quedó en Constant y otros en TwoConstants.
        /// </summary>
        static void EnsureCompatibleVelocityModes(ParticleSystem ps)
        {
            if (ps == null)
            {
                return;
            }

            var velocity = ps.velocityOverLifetime;
            if (!velocity.enabled)
            {
                return;
            }

            velocity.x = ToTwoConstants(velocity.x);
            velocity.y = ToTwoConstants(velocity.y);
            velocity.z = ToTwoConstants(velocity.z);
            velocity.orbitalX = ToTwoConstants(velocity.orbitalX);
            velocity.orbitalY = ToTwoConstants(velocity.orbitalY);
            velocity.orbitalZ = ToTwoConstants(velocity.orbitalZ);
            velocity.orbitalOffsetX = ToTwoConstants(velocity.orbitalOffsetX);
            velocity.orbitalOffsetY = ToTwoConstants(velocity.orbitalOffsetY);
            velocity.orbitalOffsetZ = ToTwoConstants(velocity.orbitalOffsetZ);
            velocity.radial = ToTwoConstants(velocity.radial);
            velocity.speedModifier = ToTwoConstants(velocity.speedModifier);
        }

        static ParticleSystem.MinMaxCurve ToTwoConstants(ParticleSystem.MinMaxCurve curve)
        {
            if (curve.mode == ParticleSystemCurveMode.TwoConstants)
            {
                return curve;
            }

            if (curve.mode == ParticleSystemCurveMode.Constant)
            {
                float value = curve.constant;
                return new ParticleSystem.MinMaxCurve(value, value);
            }

            return new ParticleSystem.MinMaxCurve(curve.constantMin, curve.constantMax);
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
