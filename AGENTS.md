# AGENTS.md — YESTERFALL

YESTERFALL (antes Días Prestados): metroidvania 2D para un jugador, con acción, exploración, puzles y misterio emocional. PC/Windows y Steam como objetivo inicial. Unity `6000.6.8f1`, URP 2D. Conservar nombres técnicos y namespace `DiasPrestados`; no renombrarlos por iniciativa propia.

## Consultar según la tarea

| Tema | Referencia |
|---|---|
| Estado real, evidencias y validación pendiente | [ProjectStatus](Docs/ProjectStatus.md) |
| Identidad, pilares, bucle y calidad | [GameDesign](Docs/GameDesign.md) |
| Historia e introducción | [Story](Docs/Story.md) |
| Armas, enemigos y jefes | [Combat](Docs/Combat.md) |
| Habilidades y progresión | [Skills](Docs/Skills.md) |
| Puzles y artefacto temporal | [Puzzles](Docs/Puzzles.md) |
| Mundo, arte, sprites e Idle | [WorldArt](Docs/WorldArt.md) |
| Menús, objetivos y accesibilidad | [UI](Docs/UI.md) |
| Música, efectos y licencias | [AudioDesign](Docs/AudioDesign.md) |
| Guardado y reaparición | [SaveSystem](Docs/SaveSystem.md) |
| Hitos y autorización | [Roadmap](Docs/Roadmap.md) |

Leer solo lo relevante. ProjectStatus define DECIDIDO, PROPUESTO, PENDIENTE, IMPLEMENTADO y NO VERIFICADO. Presencia de código no equivale a aprobación jugable o visual.

## Reglas permanentes

- Conservar lo existente; cambios pequeños y verificables. Sin reescrituras ni refactorizaciones ajenas al encargo. Calidad y reutilización antes que cantidad.
- No avanzar al Hito 2 ni a otro hito sin aprobación explícita. Documentar requisitos no autoriza implementar sistemas completos.
- No modificar proporciones, escalas, sprites ni animaciones que funcionan sin autorización. Walk y pelo del protagonista protegidos: no retocarlos sin petición específica. El hermano es la referencia principal para Idle; no modificarlo al ajustar al mayor.
- Respetar escenas, prefabs, referencias y `.meta`. No mover ni borrar assets sin comprobar referencias. No crear escenas por feature ni regenerar escenas existentes sin autorización específica.
- Existen `Assets/Scenes/DiasPrestadosDemo.unity` (demo por hitos) y `ForestOrganicDemo.unity` (escena anterior y única habilitada en Build al auditar). Elegir según el encargo; no cambiar Build Settings ni migrar funcionalidades automáticamente.
- Antes de cambios fuertes, copia manual fuera del repo. Sin commits, push ni merges salvo petición explícita. No limpiar trabajo ajeno con operaciones destructivas.
- Runtime en `Assets/Scripts`, editor en `Assets/Editor`; C# y español cuando aporte claridad. Solo Input System moderno (`Assets/Input/PrototypeControls.inputactions`); nunca entrada legacy.
- Física Unity 6: `linearVelocity`, `linearDamping`, rotación congelada, detección continua e interpolación. Conservar OverlapBox, coyote time y jump buffer. Decoración sin colliders que interfieran con el jugador.
- Mantener URP 2D en `Assets/Settings`, Light2D y datos URP de cámara; sorting layers Background/Midground/Gameplay/Foreground.
- No añadir paquetes, descargas ni recursos de pago sin autorización. Comprobar licencias comerciales antes de integrar assets.
- No afirmar compilación, Play Mode, calidad visual ni aceptación sin evidencia. Registrar fecha, alcance y límites de cada comprobación.
- Al implementar, actualizar ProjectStatus; al cambiar diseño, su documento. Separar ideas, requisitos y resultados verificados. Evitar duplicación y conservar un historial breve de decisiones sustituidas.
- Evitar subagentes, búsquedas repetidas y tareas innecesarias. No escanear Library/Temp/Logs/obj/Builds. Para documentación no compilar, regenerar ni ejecutar pruebas de gameplay.

## Demo y antecedentes

Abrir demo: **Tools → Dias Prestados → Demo → Abrir DiasPrestadosDemo**. Controles configurados: A/D o flechas, ↑/Espacio y E. Abrir Forest: **Tools → Dias Prestados → Abrir Forest Organic Demo** (solo abre). Hay menús de ruta Forest Organic, colliders y autotest Hito 1; su existencia no acredita una ejecución reciente.

[DEMO_MASTER_PLAN](Docs/DEMO_MASTER_PLAN.md), [DEMO_DECISIONS](Docs/DEMO_DECISIONS.md), [DEMO_PROGRESS](Docs/DEMO_PROGRESS.md), [DEMO_TEST_CHECKLIST](Docs/DEMO_TEST_CHECKLIST.md) y `Docs/Development/` conservan antecedentes. Las actualizaciones fechadas de los documentos maestros prevalecen sobre instrucciones antiguas incompatibles. Las referencias visuales son concept art, no sprites finales ni autorización para reemplazarlos.
