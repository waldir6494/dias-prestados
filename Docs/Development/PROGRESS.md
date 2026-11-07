# Progress — Animaciones del protagonista

## Estado: Walk por fotogramas INTEGRADO (solo Walk)

Rama/PR: `cursor/animaciones-protagonista-2f38` / [#1](https://github.com/davidmartirio/dias-prestados/pull/1)  
Base mergeada: `origin/feature/prototipo-jugable` @ `5a38d45` (Walk zip). `ForestOrganicDemo` no regenerada.

## Hecho en esta entrega

1. Merge de la base con el zip Walk (sin conflictos de escena).
2. Extracción a PNG individuales:
   - `/agent/repos/dias-prestados/Assets/Art/Characters/Protagonist/Walk/walk_01.png` … `walk_08.png`
   - Zip de origen conservado.
3. Validación: **8 hashes distintos**, RGBA + alpha, lienzo uniforme 128×160.
4. Import: PPU **48**, pivot compartido en suelas (~0.496, 0.075).
5. `Player_Walk.anim` @ **12 FPS**, loop, 8 sprites.
6. `Player_Idle.anim` = 1 frame del PNG aprobado (Idle actual).
7. `Player.controller`: Idle ↔ Walk por parámetro `Speed`.
8. Animator en `Visual` (`applyRootMotion: 0`).
9. `PlayerProtagonistAnimator`: escribe `Speed` = `|vx|` en suelo; `animator.speed` ∝ `|vx|` / 3.5; escala Walk 0.76 para acercar altura al Idle ~1.72u.
10. Física / CapsuleCollider2D / GroundCheck / cámara / movimiento: **sin cambios**.

## Inconsistencias de dibujo (no ocultas)

| Tema | Detalle |
|------|---------|
| Estilo | Walk = **pixel art** 128×160; Idle (`ProtagonistApproved`) = **pintura** 153×490. No es el mismo pipeline artístico. |
| Proporciones | Walk a PPU 48 ≈ **2.25 u** de alto opaco; Idle ≈ **1.72 u**. Se usa `walkVisualScale=0.76` para mitigar el salto de tamaño al caminar — es un parche de integración, no arte unificado. |
| Silueta / detalle | Entre frames Walk hay jitter menor (pelo, bufanda, mochila). Documentado; no “arreglado” con procedural. |
| Pivot horizontal | `feet_cx` varía por frame (~0.45–0.55); pivot común promedio — puede haber micro-desliz lateral residual. |
| Aéreos / Run / Idle animado | **No** integrados (fuera de alcance). |

## Play Mode

**No ejecutado** aquí (Unity Editor ausente). Validación estática de assets/YAML sí.

## Checklist Unity 6.6

1. Abrir proyecto; dejar reimportar `Walk/walk_0*.png` (PPU 48, pivot pies).
2. Abrir `Assets/Scenes/ForestOrganicDemo.unity` (no regenerar).
3. `Player/Visual`: SpriteRenderer + **Animator** → controller `Player`.
4. `Player`: `PlayerProtagonistAnimator` con refs a Visual/Animator/Rigidbody.
5. Play — quieto: Idle = PNG aprobado.
6. Play — A/D en suelo: ciclo Walk 8 frames; piernas/brazos se mueven; sin root motion del Rigidbody.
7. Velocidad más alta → `animator.speed` sube (menos patinaje).
8. Giro: `flipX` del sprite; collider sin escala negativa.
9. Pies: Visual y=-0.85; comprobar que no flotan/hunden respecto al suelo del bosque.
10. Confirmar cápsula 0.55×1.7 y GroundCheck intactos.
11. Anotar en arte el salto Idle painterly ↔ Walk pixel.

## Pendiente (no este PR scope)

Run 8 · Idle animado ~4 · Jump · Fall · Landing · unificar estilo con el PNG aprobado.
