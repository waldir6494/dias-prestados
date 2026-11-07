# Progress — Animaciones del protagonista

## Estado: BLOQUEADO — faltan los 8 PNG Walk en el repo/workspace

David indicó que añadió 8 PNG en `Assets/Art/Characters/Protagonist/Walk/`.  
**En esta máquina / remotos GitHub esa carpeta no existe.**

Comprobado (2026-11-07):

| Fuente | Resultado |
|--------|-----------|
| Working tree local | No hay `Assets/Art/Characters/Protagonist/Walk/` |
| `origin/feature/prototipo-jugable` (`0f4a8f1`) | Sin paths Walk |
| `origin/cursor/animaciones-protagonista-2f38` (`884c85f`) | Sin paths Walk |
| API GitHub contents de `…/Protagonist/Walk` | 404 |

Merge de `feature/prototipo-jugable` en la rama de trabajo: **ya estaba al día** (base `0f4a8f1`); nada nuevo que integrar; `ForestOrganicDemo` no se tocó.

Sin los PNG **no** se puede: validar distinct+alpha, pivotes, PPU~48, `Player_Walk.anim`, estado Walk del Animator, ni sync por velocidad.

## Qué hace falta empujar / sincronizar

Ruta esperada: `Assets/Art/Characters/Protagonist/Walk/`  
Idealmente 8 archivos PNG distintos (p. ej. `walk_01.png` … `walk_08.png`) + `.meta` si Unity ya los importó en local.

Cuando estén en `feature/prototipo-jugable` o en esta rama, el siguiente paso (solo Walk) será:

1. Validar 8 PNG distintos + alpha  
2. Pivote pies / import PPU ~48  
3. `Player_Walk.anim` 10–12 FPS  
4. Animator: estado Walk; Idle = sprite actual hasta mejor arte  
5. Activar Walk por `|linearVelocity.x|` real; sin root motion  
6. No tocar física / collider / GroundCheck / cámara  
7. Documentar inconsistencias de dibujo  

## Interim actual (sin cambios en este turno de integración)

- Visual: `ProtagonistApproved.png`  
- `PlayerProtagonistAnimator`: lean/squash temporal (no fotogramas)  
- PR [#1](https://github.com/davidmartirio/dias-prestados/pull/1) — sin merge  

## Play Mode

No aplicable hasta tener frames. Unity Editor sigue ausente aquí.
