using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Parallax suave respecto a la cámara.
    /// factor 0 = anclado al mundo; factor 1 = se pega a la cámara (capa lejana/cielo).
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] Transform cameraTransform;
        [SerializeField] [Range(-0.25f, 1.2f)] float parallaxX = 0.5f;
        [SerializeField] [Range(-0.25f, 1.2f)] float parallaxY = 0.2f;
        [SerializeField] bool lockZ = true;

        Vector3 origin;
        Vector3 cameraOrigin;
        bool initialized;

        void Start()
        {
            CacheOrigins();
        }

        void LateUpdate()
        {
            if (cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    cameraTransform = Camera.main.transform;
                    CacheOrigins();
                }
                else
                {
                    return;
                }
            }

            if (!initialized)
            {
                CacheOrigins();
            }

            Vector3 camDelta = cameraTransform.position - cameraOrigin;
            Vector3 pos = origin + new Vector3(camDelta.x * parallaxX, camDelta.y * parallaxY, 0f);
            if (lockZ)
            {
                pos.z = origin.z;
            }

            transform.position = pos;
        }

        void CacheOrigins()
        {
            origin = transform.position;
            if (cameraTransform != null)
            {
                cameraOrigin = cameraTransform.position;
                initialized = true;
            }
        }

        public void Configure(Transform cam, float factorX, float factorY)
        {
            cameraTransform = cam;
            parallaxX = factorX;
            parallaxY = factorY;
            CacheOrigins();
        }
    }
}
