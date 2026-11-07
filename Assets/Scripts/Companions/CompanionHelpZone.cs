using System.Collections.Generic;
using UnityEngine;

namespace DiasPrestados.Companions
{
    /// <summary>
    /// Zona del nivel (sin collider, para no interferir con la detección de suelo) donde el hermano pequeño
    /// puede quedar atascado ante un obstáculo demasiado alto. Define dónde aterriza al recibir ayuda.
    /// </summary>
    public class CompanionHelpZone : MonoBehaviour
    {
        static readonly List<CompanionHelpZone> Registry = new List<CompanionHelpZone>();

        [SerializeField] Vector2 size = new Vector2(3f, 3f);
        [Tooltip("Posición de los pies al terminar el impulso.")]
        [SerializeField] Transform landingPoint;

        public static IReadOnlyList<CompanionHelpZone> All => Registry;

        public Vector3 LandingPosition => landingPoint != null ? landingPoint.position : transform.position;

        public bool Contains(Vector2 point)
        {
            Vector2 c = transform.position;
            return Mathf.Abs(point.x - c.x) <= size.x * 0.5f && Mathf.Abs(point.y - c.y) <= size.y * 0.5f;
        }

        void OnEnable()
        {
            if (!Registry.Contains(this))
            {
                Registry.Add(this);
            }
        }

        void OnDisable()
        {
            Registry.Remove(this);
        }

        public static CompanionHelpZone Find(Vector2 point)
        {
            for (int i = 0; i < Registry.Count; i++)
            {
                if (Registry[i] != null && Registry[i].Contains(point))
                {
                    return Registry[i];
                }
            }

            return null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.3f, 0.6f);
            Gizmos.DrawWireCube(transform.position, size);
            if (landingPoint != null)
            {
                Gizmos.DrawLine(transform.position, landingPoint.position);
                Gizmos.DrawWireSphere(landingPoint.position, 0.25f);
            }
        }
    }
}
