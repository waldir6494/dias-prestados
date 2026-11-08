using UnityEngine;

namespace DiasPrestados.Interaction
{
    /// <summary>
    /// Caja empujable del primer minipuzle. Se empuja caminando contra ella; se queda quieta al soltarla
    /// (los colliders no tienen fricción) y vuelve a su sitio si cae al barranco o queda fuera de la zona.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableCrate : MonoBehaviour
    {
        [SerializeField] float restSpeed = 0.5f;
        [SerializeField] float killY = -5.5f;
        [SerializeField] float minX = 86.5f;
        [SerializeField] float maxX = 101f;

        Rigidbody2D rb;
        Vector2 startPosition;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            startPosition = rb.position;
        }

        void FixedUpdate()
        {
            Vector2 p = rb.position;
            if (p.y < killY || p.x < minX || p.x > maxX)
            {
                ResetCrate();
                return;
            }

            Vector2 v = rb.linearVelocity;
            if (Mathf.Abs(v.x) < restSpeed && v.y > -0.5f)
            {
                rb.linearVelocity = new Vector2(0f, v.y);
            }
        }

        public void ResetCrate()
        {
            rb.position = startPosition;
            rb.linearVelocity = Vector2.zero;
        }
    }
}
