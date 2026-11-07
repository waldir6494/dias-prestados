# Días Prestados — Plan maestro de la demo (≈10 min)

Plan por hitos con parada obligatoria tras cada uno. Ningún hito empieza sin aprobación explícita del usuario.

## Principios

- Motor Unity 6.6 (`6000.6.8f1`), URP 2D, solo Input System moderno, sin diálogos.
- Referencias visuales en `Docs/VisualReferences/` (concept art, no sprites). Paleta: azul pizarra, verde grisáceo, ámbar cálido. Violeta **solo** para anomalías temporales.
- Hermano mayor (12): pelo castaño oscuro desordenado, ropa azul/gris, bufanda roja/ladrillo, mochila marrón. Hermano pequeño (8): pelo castaño, gorro de lana con pompón, chaqueta mostaza, mochila pequeña.
- Escena de la demo: `Assets/Scenes/DiasPrestadosDemo.unity` (independiente de `ForestOrganicDemo`).
- Arte provisional generado por código desde `Assets/Editor/Demo/*`, sustituible por arte final.

## Arquitectura (resumen)

| Sistema | Carpeta | Notas |
|---|---|---|
| Jugador | `Scripts/Player` | `PlayerController2D` (reutilizado), `PlayerFallRecovery` |
| Compañero | `Scripts/Companions` | `CompanionFollower` (máquina de estados Following/Asking/Boosted/Cheering), zonas de ayuda, animador de sprites |
| Interacción | `Scripts/Interaction` | `Interactable` + `InteractionSensor` (por distancia, sin triggers) + `InteractionPrompt` |
| Entorno | `Scripts/Environment` | Parallax, motas, viñeta, brillo de alegría |
| UI | `Scripts/UI` | HUD de controles, mensajes por zona |
| Pruebas | `Scripts/Testing`, `Editor/Demo` | Autotest en Play Mode con teclado virtual |
| Construcción de escena | `Editor/Demo` | `DemoTerrain` (perfil único), `DemoImg`, `DemoBrotherArt`, `DemoEnvironmentArt`, `DemoSceneBuilder` |

Arquitectura prevista para hitos posteriores:

- **Estados temporales (Hito 4–5):** componente `TemporalObject` con estados (Original/Transformado), eventos y un `TemporalArtifact` que selecciona objetos compatibles en un radio; feedback violeta; sin dependencias externas.
- **Puzles (Hito 5–6):** `Interactable` reutilizado; mecanismos con estado serializado y reinicio seguro (sin bloqueos por orden incorrecto).
- **Persecución (Hito 7):** `CheckpointService` + reinicio rápido; amenaza con velocidad predecible.
- **Secuencias (Hito 3, 8):** controlador de secuencias basado en corrutinas/Timeline, efectos con URP 2D (Light2D, partículas, post-proceso).

## Hitos y criterios de aceptación

### Hito 0 — Auditoría y plan
- Auditoría del proyecto y Git; cuatro documentos en `Docs/`.
- **Aceptación:** documentos creados; plan cubre Hitos 0–10.

### Hito 1 — Primera prueba jugable (45–90 s)
- Escena nueva con ambos hermanos reconocibles, movimiento, salto, cámara suave, bosque atmosférico con parallax, plataformas y colliders, el pequeño sigue al mayor, una interacción simple.
- **Aceptación:** recorrido completo A/D–↑–E sin quedar atascado; el pequeño sigue, salta obstáculos pequeños y pide ayuda ante el risco; con E es impulsado; sin Missing Scripts ni errores de consola.

### Hito 2 — Bosque y relación entre hermanos (~2 min)
- Tramo más largo, animaciones y reacciones, llegada a vías y estación, interacción emocional memorable.
- **Aceptación:** el usuario siente cariño por el pequeño (revisión subjetiva); tramo ≈ 2 min; sin bloqueos.

### Hito 3 — Separación temporal
- Anomalía, distorsión, reacción de personajes, separación, desconcierto y transición a la zona temporal con sistemas reales de Unity.
- **Aceptación:** secuencia legible sin vídeos externos; transición estable; violeta solo en la anomalía.

### Hito 4 — Descubrimiento del artefacto
- Exploración, recogida animada, introducción del poder, selección de objetos compatibles, arquitectura de estados temporales, primera transformación simple.
- **Aceptación:** el artefacto se entiende sin texto; una transformación funcional y satisfactoria.

### Hito 5 — Puzles temporales
- Al menos dos transformaciones distintas (oxidar, crecer, deteriorar…).
- **Aceptación:** lógica clara, feedback visual y mecánico, sin estados irrecuperables.

### Hito 6 — Puzle físico independiente
- Vagoneta, palanca, contrapeso o plataforma móvil.
- **Aceptación:** solución clara con señales visuales y consecuencias físicas fiables.

### Hito 7 — Peligro y persecución
- Amenaza, sigilo/escondite/huida; checkpoints y reinicio rápido.
- **Aceptación:** tensión sin frustración; reinicio < 3 s.

### Hito 8 — Final emocional
- Pista sobre el hermano (tiempo exterior distinto), escena evocadora y pantalla de fin de demo; sin revelar el giro completo.
- **Aceptación:** cierre claro y coherente.

### Hito 9 — Presentación y pulido
- Animaciones, transiciones, cámara, luz, sonido, partículas, UI, ritmo, claridad y coherencia.
- **Aceptación:** sin cambios artísticos radicales sin consulta; checklist de pulido completa.

### Hito 10 — Validación final
- Demo integrada de ~10 min, errores técnicos corregidos, versión estable de la escena.
- **Aceptación:** recorrido completo validado; sin build de distribución sin autorización.

## Reglas de proceso

- Informe "HITO COMPLETADO" de 8 secciones al terminar cada hito, distinguiendo implementado / compilado / validado automáticamente / validado en Play Mode / pendiente de prueba manual.
- Estados de `DEMO_PROGRESS.md`: PENDIENTE, EN DESARROLLO, IMPLEMENTADO — PENDIENTE DE REVISIÓN, CORRECCIONES SOLICITADAS, APROBADO.
- Sin commits salvo petición explícita; nunca `reset --hard`, `clean` ni `push`.
