# YESTERFALL — Progreso de la demo

Actualizado: 2026-11-08. Estado central y criterios de evidencia en [ProjectStatus](ProjectStatus.md). Alcance futuro en [Roadmap](Roadmap.md). Esta actualización no ejecuta ni aprueba hitos.

| Hito previo | Nombre | Estado |
|---|---|---|
| 0 | Auditoría y plan | Documentación presente y ampliada; PENDIENTE de revisión. |
| 1 | Primera prueba jugable | Base presente en repositorio; PENDIENTE de revisión actual en Unity y aprobación. |
| 2 | Bosque y relación entre hermanos | PENDIENTE — no autorizado. |
| 3 | Separación temporal | PENDIENTE. |
| 4 | Descubrimiento del artefacto | PENDIENTE. |
| 5 | Puzles temporales | PENDIENTE. |
| 6 | Puzle físico independiente | PENDIENTE. |
| 7 | Peligro y persecución | PENDIENTE. |
| 8 | Final emocional | PENDIENTE. |
| 9 | Presentación y pulido | PENDIENTE. |
| 10 | Validación final | PENDIENTE. |

## IMPLEMENTADO — inventario del Hito 1

En repositorio: `Assets/Scenes/DiasPrestadosDemo.unity`, personajes PNG, controlador, cámara, terreno/parallax, compañero, interacción, caja empujable, mensajes HUD y código de autotest. Esto no certifica comportamiento, sensación jugable, Missing Scripts ni aprobación artística actuales. Evidencias detalladas sin duplicar en ProjectStatus.

## NO VERIFICADO — informes históricos conservados

La versión previa de este documento informaba:

- Compilación en Unity Editor con advertencias y autotest Play Mode «TODO CORRECTO»; avance, salto ~2,7 u, recorrido hasta x≈189, petición de ayuda, impulso con E y seguimiento al final.
- Informe en `Temp/DemoAutotest/report.txt`; no se consultó ni se reejecutó durante esta auditoría. No usarlo como prueba actual.
- Correcciones de roca decorativa (escala menor, orden -3, sin collider); Walk sincronizado por desplazamiento/input; rediseño del hermano y animaciones Idle/Walk/Cheer; caja del tramo de x≈100 con restitución y prueba de toques cortos/caja/ayuda/final.
- Revisión manual del usuario todavía pendiente. La indicación histórica de «sin commit» se refería a aquella ejecución; no describe todo el historial del repositorio.

Son antecedentes, no nuevas verificaciones. No se recertifican duraciones, medidas, colisiones ni resultado visual para la versión actual.

## DECIDIDO

Conservar lo existente. No avanzar al Hito 2. Proteger Walk/pelo y animaciones actuales; ajustes solo con petición específica. La corrección visual del pelo y la respiración exige comparación real en Unity antes de darse por IMPLEMENTADA/aceptada.

## PROPUESTO / PENDIENTE

Próxima revisión pequeña: comparar ambos Idle y la silueta del mayor en Unity, después comprobar el recorrido del Hito 1. Registrar resultados y aprobación por separado. Reconciliar los hitos antiguos con el metroidvania, el combate y la separación temprana antes de iniciar desarrollo nuevo.
