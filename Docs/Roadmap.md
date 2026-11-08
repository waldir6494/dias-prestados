# YESTERFALL — Progresión del desarrollo

Actualizado: 2026-11-08. Estado y evidencias: [ProjectStatus](ProjectStatus.md). Alcance de producto: [GameDesign](GameDesign.md).

## DECIDIDO

- Hitos pequeños y verificables. Conservar y estabilizar lo que ya existe; calidad y reutilización frente a cantidad.
- No avanzar automáticamente ni comenzar el Hito 2 sin aprobación explícita. No implementar sistemas enteros por aparecer en esta hoja de ruta.
- Probar mecánicas individualmente antes de combinarlas. Un hito necesita revisión de comportamiento y calidad cuando sea posible; compilar no basta.
- Primera meta: demo pequeña de calidad, orientativamente ~10 minutos. Debe demostrar progresivamente inicio narrativo, desaparición temprana del pequeño, movimiento, exploración, combate básico con apuntado, un puzle interesante, escenario cuidado, audio y objetivos visibles. Cierre con recompensa/descubrimiento; minijefe sujeto a viabilidad.
- Antes de integrar todo, validar una escena pequeña de prueba con movimiento, combate y puzles. Reutilizar las escenas existentes según el encargo; no crear demos por feature.

## Estado del plan previo

Los hitos 0–10 se conservan en [DEMO_MASTER_PLAN](DEMO_MASTER_PLAN.md). [DEMO_PROGRESS](DEMO_PROGRESS.md) guarda los informes previos de Hito 1, no su aprobación. Ningún hito posterior queda autorizado por esta actualización.

| Tramo previo | Situación actual |
|---|---|
| Hito 0: auditoría/plan | Documentación ampliada; revisión del usuario pendiente. |
| Hito 1: primera prueba jugable | Código, recursos y escena presentes; comportamiento y aceptación visual pendientes de comprobación actual. |
| Hitos 2–10 | PENDIENTES, sin iniciar por esta tarea. El plan anterior requiere ajuste al metroidvania antes de su ejecución. |

El Hito 2 antiguo prolongaba la relación a ~2 minutos y la separación llegaba en Hito 3. El objetivo narrativo actual es separarlos tras ~60–90 segundos. No resolver esta diferencia construyendo tramos: acordar primero el alcance.

## PROPUESTO — secuencia para acordar, no autorización

1. Revisar ambos personajes en Unity: curva posterior/nuca, coherencia Walk e Idle localizado, comparación con el hermano, cambios de estado y pies estables.
2. Revalidar movimiento, salto, colisiones, caja/ayuda y recuperación del Hito 1; registrar resultados y aceptación.
3. Acordar cómo encajan combate, puzles y objetivos en hitos revisados, conservando el límite de una demo pequeña.
4. Si se autoriza, realizar pruebas aisladas de apuntado/combate y un puzle; evaluar respuesta visual/sonora y controles antes de integrar.
5. Integrar narrativa, exploración, recompensas, audio/UI y eventualmente un minijefe solo mediante encargos delimitados.

## PENDIENTE

Revisión manual del Hito 1, aprobación visual de correcciones, reconciliar la duración de la introducción y redefinir el orden futuro de hitos. Después: alcance de armas, habilidades, regiones, guardado, menús, audio, licencias, optimización y builds PC/Steam. No construir capítulos ni sistemas completos para anticiparse.

## IMPLEMENTADO / NO VERIFICADO

Hay prueba de base y herramientas de comprobación en el repositorio; no hay demo completa de metroidvania acreditada. Los antiguos «TODO CORRECTO» no se vuelven a certificar en esta auditoría documental. No se ejecutaron compilaciones, autotests ni builds.
