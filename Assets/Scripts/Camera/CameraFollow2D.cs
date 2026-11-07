using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Cámara ortográfica con seguimiento suave y límites del escenario.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector2 offset = new Vector2(0f, 1.2f);
        [SerializeField] float smoothTime = 0.18f;
        [SerializeField] Vector2 minBounds = new Vector2(-16f, -1f);
        [SerializeField] Vector2 maxBounds = new Vector2(16f, 8f);
        [SerializeField] bool useBounds = true;
        [Tooltip("Adelanto horizontal en la dirección de movimiento (0 = desactivado).")]
        [SerializeField] float lookAheadDistance = 0f;
        [SerializeField] float lookAheadSpeed = 2.5f;

        Camera cam;
        Vector3 velocity;
        Rigidbody2D targetBody;
        float lookAhead;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (lookAheadDistance > 0f)
            {
                if (targetBody == null)
                {
                    targetBody = target.GetComponent<Rigidbody2D>();
                }

                float vx = targetBody != null ? targetBody.linearVelocity.x : 0f;
                float wanted = Mathf.Abs(vx) > 0.5f ? Mathf.Sign(vx) * lookAheadDistance : 0f;
                lookAhead = Mathf.MoveTowards(lookAhead, wanted, lookAheadSpeed * Time.deltaTime);
            }

            Vector3 desired = new Vector3(
                target.position.x + offset.x + lookAhead,
                target.position.y + offset.y,
                transform.position.z);

            if (useBounds)
            {
                float halfHeight = cam.orthographicSize;
                float halfWidth = halfHeight * cam.aspect;

                desired.x = Mathf.Clamp(desired.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
                desired.y = Mathf.Clamp(desired.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }

    }
}
