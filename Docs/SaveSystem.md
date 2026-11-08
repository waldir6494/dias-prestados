# YESTERFALL — Guardado, checkpoints y reaparición

Actualizado: 2026-11-08. Relacionado: [Skills](Skills.md), [Puzzles](Puzzles.md), [UI](UI.md).

## DECIDIDO

- Planificar guardado/carga, checkpoints, progreso persistente de habilidades, muerte/reaparición y continuidad de exploración. No implementar todos estos sistemas ahora.
- Evitar bloqueos por gasto opcional, pérdida de objetos/munición o estados de puzle. Las habilidades imprescindibles tienen progresión garantizada o alternativa.
- Distinguir recuperación provisional en la sesión de guardado persistente y checkpoints de diseño.

## PROPUESTO

El plan anterior proponía un `CheckpointService` para persecución con reinicio rápido. Es una propuesta de arquitectura histórica, no una clase existente ni una decisión técnica definitiva.

## PENDIENTE

Formato/versionado, slots, frecuencia de guardado, datos persistentes, tratamiento de puzles y enemigos, checkpoints, muerte, recuperación ante archivos corruptos y persistencia de opciones. Compatibilidad con Steam no implica Steam Cloud ya decidido o implementado.

## IMPLEMENTADO

`Assets/Scripts/Player/PlayerFallRecovery.cs` conserva una posición segura en memoria y recoloca al jugador si cae bajo `killY`. `PushableCrate.cs` devuelve la caja al origen si sale de su zona. Ninguno guarda partidas entre sesiones.

## NO VERIFICADO

No se encontró guardado/carga persistente ni gestión completa de muerte en los scripts runtime revisados. Tampoco se revalidó en Unity la recuperación ante caídas ni la ausencia de bloqueos del minipuzle actual.
