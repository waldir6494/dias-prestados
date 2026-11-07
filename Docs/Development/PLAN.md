# Plan — Animaciones del protagonista

## Contexto (análisis)

- Escena activa: `Assets/Scenes/ForestOrganicDemo.unity`.
- El `Player` usa `PlayerController2D` + `Rigidbody2D` + `CapsuleCollider2D` (1.7 × 0.55).
- Jerarquía visual actual:
  - `Visual` (activo): sprite estático `DiasPrestados_Protagonista_Perfil_PNG` (perfil, PPU 214, pivot inferior).
  - `Visual_Legacy` (inactivo): silueta procedural antigua.
- El giro horizontal ya existe vía `SpriteRenderer.flipX` en el primer renderer hijo.
- Referencias en `Docs/ArtDirection/References/` (perfil + hoja de animaciones) **no** son spritesheets importables: proporciones/poses inconsistentes entre frames.
- No hay `Animator`, clips ni controller en el proyecto.
- Unity Editor **no** está disponible en este entorno cloud → no se puede generar clips `.anim` desde el Editor ni validar Play Mode aquí.

## Objetivo de animación

Estados jugables del protagonista:

| Estado   | Disparo |
|----------|---------|
| Idle     | En suelo, \|vx\| bajo |
| Walk/Run | En suelo, \|vx\| alto |
| Jump     | En aire, vy > umbral |
| Fall     | En aire, vy ≤ umbral |
| Landing  | Transición aire → suelo (corta) |
| Facing   | Escala X del root visual (no tocar colisión) |

Diseño visual objetivo: niño ~12 años, cabello oscuro, chaqueta azul-grisácea, bufanda roja, mochila, botas.

## Solución mínima elegida (revisada tras rechazo de la marioneta)

**Sprite integrado = PNG aprobado** + animación sobria por transform:

1. `ProtagonistApproved.png`: recorte del PNG de perfil aprobado, pivot en suelas, altura mundo ≈ 1.72 (cápsula 1.7).
2. `PlayerProtagonistAnimator`: Idle / Walk / Jump / Fall / Landing con respiración, lean y squash.
   - En suelo **no** hay traslación Y del root (evita flotar / root motion visual raro).
   - Se elimina la marioneta de piezas rígidas del Player.
3. `PlayerController2D`: giro con `SpriteRenderer.flipX` (sin pelear con el squash).
4. Integración mínima en `ForestOrganicDemo.unity`. **No** regenerar la escena.

### Por qué no marioneta / no spritesheet completo

- La marioneta procedural fue rechazada (feo, pies mal, no parece el PNG).
- La hoja artística no es un spritesheet válido para importar tal cual.
- Sin frames pintados, priorizamos **fidelidad al PNG** sobre “animación de piezas”.

## Arte definitivo pendiente

Walk/Jump frame-a-frame coherentes con el perfil aprobado (o retocado desde la hoja de concepto). La animación actual es transform sobria sobre el dibujo integrado.

## Restricciones respetadas

- No nuevas escenas; no regenerar `ForestOrganicDemo`.
- No alterar movimiento, gravedad, salto, `Rigidbody2D`, `CapsuleCollider2D` ni cámara.
- No destruir recursos existentes (`Visual_Legacy`, PNG de perfil).
- Solo protagonista.
- Controles teclado e Input System intactos; Sorting Layer `Gameplay` en renderers del rig.

## Orden de implementación

1. Análisis (este documento).
2. Recursos visuales de partes.
3. Scripts de animación + exposición de estado.
4. Cableado en `ForestOrganicDemo`.
5. Validación estática disponible + checklist manual Unity.
6. `PROGRESS.md` con resultado y limitaciones.
