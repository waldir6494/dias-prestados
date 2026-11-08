using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Viñeta mínima como sprite hijo de la cámara. Se ajusta sola al tamaño/aspecto de la cámara
    /// ortográfica para no dejar bordes visibles. Reduce "intensity" o desactiva el objeto para quitarla.
    /// No modifica el comportamiento de la cámara ni la jugabilidad.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class AtmosphereVignette : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField] [Range(0f, 1f)] float intensity = 0.35f;
        [SerializeField] [Range(1f, 2f)] float overshoot = 1.1f;

        SpriteRenderer spriteRenderer;

        void LateUpdate()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            Camera cam = targetCamera;
            if (cam == null && transform.parent != null)
            {
                cam = transform.parent.GetComponent<Camera>();
            }

            if (cam == null)
            {
                cam = Camera.main;
            }

            if (cam == null || !cam.orthographic || spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return;
            }

            float height = cam.orthographicSize * 2f * overshoot;
            float width = height * cam.aspect;
            Vector2 native = spriteRenderer.sprite.bounds.size;
            Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            transform.localScale = new Vector3(
                width / Mathf.Max(0.0001f, native.x) / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
                height / Mathf.Max(0.0001f, native.y) / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
                1f);

            Color c = spriteRenderer.color;
            c.a = intensity;
            spriteRenderer.color = c;
        }

        public void Configure(Camera cam, float vignetteIntensity)
        {
            targetCamera = cam;
            intensity = vignetteIntensity;
        }
    }
}
