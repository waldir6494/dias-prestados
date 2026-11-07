# Plan — Animaciones del protagonista

## Contexto

- Diseño visual del PNG de perfil: **APROBADO** (no rediseñar).
- Escena: `Assets/Scenes/ForestOrganicDemo.unity`.
- Problema actual: el personaje se **desliza** porque solo hay un fotograma estático (más un interim de lean/squash).
- Objetivo: animación 2D **por fotogramas** (indie cuidado). Prohibido: marioneta de piezas; sprite único deslizándose como “solución final”.

## Inspección (hecho)

- Un solo sprite jugable aprobado: `ProtagonistApproved.png`.
- No existen frames Idle/Walk/Run/Jump/Fall/Landing en `Assets/`.
- No hay Animator Controller ni clips.
- Hojas en `Docs/ArtDirection/References/` = referencia; **no** spritesheet válido (etiquetas, fondos, inconsistencias).
- Intento de generación automática de Walk 8: falló (frames idénticos). Ver `PROGRESS.md`.

## Estado del plan: BLOQUEADO — Walk 8 no están en el repo

David indicó PNG en `Assets/Art/Characters/Protagonist/Walk/`, pero **no aparecen** en local ni en `origin/feature/prototipo-jugable` / rama PR (verificado).  
Hasta que esos 8 archivos estén pusheados/sincronizados no se implementa Animator ni `Player_Walk.anim`.

### Entrega de arte requerida

| Set | Frames | Notas |
|-----|--------|-------|
| Walk | 8 | Prioridad 1; piernas/brazos visibles; ciclo completo |
| Run | 8 | Solo tras Walk |
| Idle | ~4 | Solo tras Walk |
| Jump | 2–4 | Solo tras Walk |
| Fall | 1–2 | Solo tras Walk |
| Landing | 2–3 | Solo tras Walk |

Criterios: mismo niño ~12 (pelo oscuro, chaqueta azul-gris, bufanda roja, mochila, botas); alpha; lienzo uniforme; pivote/pies alineados; **no** duplicar el mismo pose con offset.

## Integración (cuando haya frames)

1. Import sprites (Single o Multiple), pivot pies, PPU alineado a altura ~1.72 / Visual y=-0.85.
2. Clips Unity + Animator Controller; parámetros tipicos: `Speed`, `Grounded`, `VerticalVelocity`, `Landing`.
3. Sync: stride rate ∝ `|Rigidbody2D.linearVelocity.x|`; sin root motion físico.
4. Transiciones Idle ↔ Walk/Run ↔ Jump/Fall ↔ Landing.
5. Giro solo en visual (`flipX` / escala del Visual); no tocar física.
6. Escena: cableado mínimo en `Player`/`Visual`; **no** regenerar.

### No tocar

Rigidbody2D, CapsuleCollider2D, GroundCheck, salto, gravedad, CameraFollow2D.

## Interim actual (no cuenta como animación por frames)

- Visual = PNG aprobado (pivot/altura).
- `PlayerProtagonistAnimator` = lean/squash temporal.
- Se sustituye o desactiva al integrar el Animator con Walk 8.

## Orden de trabajo restante

1. **Arte:** Walk 8 → validación técnica en Unity.  
2. Arte Run / Idle / aéreos.  
3. Clips + Controller + sync.  
4. Play Mode checklist.
