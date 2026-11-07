# Plan — Animaciones del protagonista

## Hecho

- Diseño Idle = PNG aprobado (no rediseñar).
- Walk 8 fotogramas extraídos del zip en `Assets/Art/Characters/Protagonist/Walk/`.
- Integración **solo Walk**: clips + Animator + sync `|vx|`.
- Sin marioneta; sin root motion físico; física/cámara intactas.

## En curso / siguiente arte

| Set | Estado |
|-----|--------|
| Walk 8 | Integrado (pixel; inconsistente vs Idle painterly) |
| Idle animado ~4 | Pendiente — Idle actual = 1 frame aprobado |
| Run 8 | Pendiente |
| Jump / Fall / Landing | Pendiente |

## Criterio de calidad pendiente

Unificar estilo Walk con el PNG aprobado (o aceptar pixel como dirección y redibujar Idle). Mitigar jitter de pivote/silueta entre frames Walk.
