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

## Solución mínima elegida

**Rig articulado 2D por código** (no frame-by-frame artístico):

1. Partes PNG procedurales con la paleta del diseño (head, torso, scarf, backpack, arm, leg, boot), persistentes en `Assets/Art/Characters/Protagonist/Parts/`.
2. Componente `PlayerArticulatedAnimator` que:
   - Monta el rig bajo `Visual` en runtime (si aún no existe).
   - Desactiva el `SpriteRenderer` monolítico de `Visual` (el asset se conserva).
   - Anima rotaciones locales de extremidades según velocidad / suelo / aterrizaje.
3. Extensión mínima de `PlayerController2D`:
   - Exponer lectura de input horizontal y velocidad (sin cambiar física).
   - Voltear el transform `Visual` con `localScale.x` (compatible con jerarquía articulada).
4. Integración en `ForestOrganicDemo.unity`: añadir el componente al `Player` existente. **No** regenerar la escena.

### Por qué no Animator + spritesheet

- La hoja artística no es un spritesheet válido.
- Sin Unity Editor no se pueden crear/validar `.controller` / clips de forma fiable.
- El rig por código produce movimiento **real** de piezas (no un sprite estático con bobbing).

## Arte definitivo pendiente

Los PNG de partes son **placeholders funcionales** alineados a la paleta. El arte final (frames o partes pintadas consistentes a partir de `04_protagonista_12_anos` / perfil) queda documentado como pendiente de dirección artística.

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
