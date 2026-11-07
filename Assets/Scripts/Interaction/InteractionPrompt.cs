using UnityEngine;

namespace DiasPrestados.Interaction
{
    /// <summary>
    /// Indicador discreto (tecla E) que flota sobre el objeto interactivo más cercano.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class InteractionPrompt : MonoBehaviour
    {
        [SerializeField] Vector3 worldOffset = new Vector3(0f, 0.9f, 0f);
        [SerializeField] float bobAmplitude = 0.07f;
        [SerializeField] float bobSpeed = 3f;
        [SerializeField] float fadeSpeed = 7f;
        [SerializeField] float maxAlpha = 0.95f;

        SpriteRenderer sr;
        Transform anchor;
        Vector3 anchorOffset;
        float targetAlpha;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            SetAlpha(0f);
        }

        public void Show(Transform newAnchor, Vector3 position)
        {
            anchor = newAnchor;
            anchorOffset = position - (newAnchor != null ? newAnchor.position : Vector3.zero);
            targetAlpha = maxAlpha;
        }

        public void Hide()
        {
            targetAlpha = 0f;
        }

        void LateUpdate()
        {
            float a = Mathf.MoveTowards(sr.color.a, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(a);

            if (anchor != null)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
                transform.position = anchor.position + anchorOffset + worldOffset + new Vector3(0f, bob, 0f);
            }
        }

        void SetAlpha(float a)
        {
            Color c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
