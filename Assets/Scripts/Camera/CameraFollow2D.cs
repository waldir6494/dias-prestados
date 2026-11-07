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

        Camera cam;
        Vector3 velocity;

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

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
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
