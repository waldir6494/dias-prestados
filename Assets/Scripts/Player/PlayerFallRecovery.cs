using UnityEngine;

namespace DiasPrestados
{
    /// <summary>
    /// Devuelve al protagonista a la última posición segura si cae fuera del escenario (barranco, etc.).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerController2D))]
    public class PlayerFallRecovery : MonoBehaviour
    {
        [SerializeField] float killY = -5.5f;
        [SerializeField] float safeGroundTime = 0.35f;

        Rigidbody2D rb;
        PlayerController2D controller;
        Vector2 lastSafe;
        float groundedFor;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController2D>();
            lastSafe = rb.position;
        }

        void Update()
        {
            if (controller.IsGrounded)
            {
                groundedFor += Time.deltaTime;
                if (groundedFor >= safeGroundTime)
                {
                    lastSafe = rb.position;
                }
            }
            else
            {
                groundedFor = 0f;
            }

            if (rb.position.y < killY)
            {
                rb.position = lastSafe + Vector2.up * 0.3f;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
