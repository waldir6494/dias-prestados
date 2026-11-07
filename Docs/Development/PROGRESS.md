# Progress — Animaciones del protagonista

## Estado

Implementación **completa a nivel de código y recursos** en rama `cursor/animaciones-protagonista-2f38` (base `feature/prototipo-jugable` @ `0f4a8f1`).

Tipo de animación entregada: **rig articulado 2D por código** (piezas que rotan). No es un spritesheet frame-by-frame artístico. No es un sprite monolítico con bobbing.

## Tareas

| # | Tarea | Estado |
|---|-------|--------|
| 1 | Analizar personaje y sistema actual | Hecho |
| 2 | Diseñar solución mínima (`PLAN.md`) | Hecho |
| 3 | Preparar recursos visuales (partes PNG) | Hecho |
| 4 | Implementar animaciones | Hecho |
| 5 | Integrar en `ForestOrganicDemo` | Hecho |
| 6 | Validaciones técnicas disponibles | Hecho (estáticas; sin Unity) |
| 7 | Documentar resultado y pruebas manuales | Hecho (este archivo) |

## Qué se cambió

- `PlayerArticulatedAnimator`: monta `ArticulatedRig` bajo `Visual`, desactiva el `SpriteRenderer` estático (el PNG de perfil **se conserva**), anima Idle / Walk / Jump / Fall / Landing.
- `PlayerController2D`: propiedades de lectura (`MoveInput`, `FacingRight`, `Velocity`); giro por `localScale.x` del `Visual` (sin tocar física).
- Partes en `Assets/Art/Characters/Protagonist/Parts/` + carga runtime vía `Assets/Resources/ProtagonistParts/`.
- Escena `ForestOrganicDemo.unity`: componente añadido al `Player` existente (sin regenerar).

## Arte definitivo pendiente

Las partes son **placeholders procedurales** con paleta del diseño (cabello oscuro, chaqueta azul-gris, bufanda roja, mochila, botas). Sustituir por partes/frames pintados consistentes a partir de `Docs/ArtDirection/References/` cuando haya arte de producción.

## Validaciones hechas aquí

- Cableado YAML de escena: GUID del script, refs a `Visual` / controller / Rigidbody2D.
- Diff: no se modificaron valores de `moveSpeed`, `jumpForce`, gravedad, collider ni cámara.
- `SampleScene` intacta.
- **Unity Editor no está instalado** en este entorno → no hay compilación Unity ni prueba visual en Play Mode.

## Checklist manual en Unity 6.6 (pendiente)

1. Abrir proyecto en Unity `6000.6.8f1` y esperar import de `Resources/ProtagonistParts`.
2. Abrir `Assets/Scenes/ForestOrganicDemo.unity` (no regenerar).
3. Seleccionar `Player`: debe tener `PlayerArticulatedAnimator` habilitado.
4. Play Mode:
   - Idle: respiración / balanceo leve de torso y bufanda.
   - A/D o flechas: ciclo de caminata con piernas y brazos opuestos.
   - Espacio: pose de Jump al subir; Fall al bajar; Landing breve al tocar suelo.
   - Giro izquierda/derecha: el rig completo se espeja (no el collider).
5. Confirmar Sorting Layer `Gameplay` en las piezas.
6. Confirmar que movimiento/salto se sienten igual que antes.
7. Cerrar/reabrir: partes y escena siguen presentes; `Visual_Legacy` y PNG de perfil intactos.

## Limitaciones honestas

- Sin prueba visual ejecutada en esta máquina.
- El look no sustituye el concepto art del perfil; es jugable/funcional.
- Ajuste fino de pivotes/escala puede requerir un pase en Editor tras ver el personaje en contexto.
