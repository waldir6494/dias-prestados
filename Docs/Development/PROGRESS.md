# Progress — Animaciones del protagonista

## Estado actual (revisión tras feedback de David)

La marioneta articulada de piezas rígidas **fue rechazada** y **retirada** del Player.

Solución vigente: **PNG aprobado** del protagonista (recorte + pivot en suelas) + animación **sobria** del sprite integrado (`PlayerProtagonistAnimator`). No es un cycle frame-by-frame artístico completo.

Rama/PR: `cursor/animaciones-protagonista-2f38` / [#1](https://github.com/davidmartirio/dias-prestados/pull/1)  
Base: `feature/prototipo-jugable` (sin merge a `main`).

## Cambios de esta revisión

| Antes (rechazado) | Ahora |
|-------------------|--------|
| Piezas procedurales tipo marioneta | Sprite único derivado del PNG aprobado |
| Walk con piernas rígidas | Lean + squash suave, **sin bob vertical** del root |
| Pies flotando / mal apoyo | Pivot en suelas, altura ≈ 1.72 (cápsula 1.7), `Visual` en y=-0.85 |
| Ocultaba el PNG | `SpriteRenderer` del `Visual` activo con `ProtagonistApproved.png` |

## Archivos clave

- `Assets/Art/Characters/Protagonist/ProtagonistApproved.png` — PNG aprobado recortado, pivot en pies, PPU ≈ 285
- `Assets/Scripts/Player/PlayerProtagonistAnimator.cs` — Idle / Walk / Jump / Fall / Landing sobrios
- `Assets/Scripts/Player/PlayerController2D.cs` — giro con `flipX` (no pelea con squash)
- `Assets/Scenes/ForestOrganicDemo.unity` — cableado mínimo del visual
- Eliminado del Player: `PlayerArticulatedAnimator` (script quitado)
- Las partes en `Parts/` y `Resources/ProtagonistParts/` **se conservan** pero **no se usan**

## Prioridades cubiertas

1. Aspecto fiel al PNG aprobado — sí (mismo arte, no siluetas placeholder)
2. Contacto con el suelo — pivot/altura/offset alineados a la cápsula; sin traslación Y en suelo
3. Coherencia con el PNG — sí
4. Naturalidad básica — lean/respiración/squash; **no** walk cycle pintado

## Validaciones

- Estáticas: escena apunta al nuevo GUID; física/collider/GroundCheck no tocados.
- Unity Play Mode: **no ejecutado aquí** (Editor ausente en el entorno).

## Checklist exacta en Unity 6.6

1. Abrir el proyecto (`6000.6.8f1`) y dejar importar `ProtagonistApproved.png`.
2. Abrir `Assets/Scenes/ForestOrganicDemo.unity` (**no** regenerar).
3. `Player` → componente `PlayerProtagonistAnimator` (no debe haber `PlayerArticulatedAnimator`).
4. Hijo `Visual`:
   - `SpriteRenderer` **enabled**
   - Sprite = `ProtagonistApproved`
   - Sorting Layer `Gameplay`, order ≥ 20
   - Local Position y = `-0.85`
   - Sin hijo `ArticulatedRig`
5. Play Mode — Idle: respiración leve; pies fijos en el suelo (no flotan).
6. Walk (A/D): lean/squash suave; **sin** rebote vertical raro; se ve el niño del PNG, no piezas.
7. Salto/caída/aterrizaje: poses sobrias; al aterrizar squash breve.
8. Giro izq/der: `flipX`; collider sin escalas negativas.
9. Confirmar que movimiento/salto/gravedad se sienten iguales.
10. Comparar visualmente con `Docs/ArtDirection/References/DiasPrestados_Protagonista_Perfil_PNG.png`.

## Limitaciones artísticas restantes (honestas)

- **No** hay spritesheet Idle/Walk/Jump pintado frame a frame.
- El walk sigue siendo el **mismo dibujo** con deformación transform (lean/squash); no mueve piernas dibujadas.
- La bufanda/mochila no tienen sim secundaria independiente.
- Para walk/jump definitivos hace falta arte de producción (o extracción cuidadosa y retocada de la hoja de concepto, hoy no homologada).
- Ajuste fino de pivot/PPU puede requerir un pase visual en Editor sobre el terreno real del bosque.
