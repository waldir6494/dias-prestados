# YESTERFALL — Estado real del proyecto

Auditoría documental: **2026-11-08**, inspección selectiva y de solo lectura de archivos Unity. No se escanearon carpetas generadas ni se ejecutaron compilaciones, pruebas o builds en esta tarea.

## Clasificación compartida

| Etiqueta | Significado |
|---|---|
| DECIDIDO | Requisito o decisión expresamente vigente; no implica código ni autorización para implementarlo ahora. |
| PROPUESTO | Idea provisional para evaluar; no convertirla en diseño definitivo. |
| PENDIENTE | Decisión, implementación, prueba o aprobación todavía por resolver; especificar cuál. |
| IMPLEMENTADO | Existencia técnica comprobada en el repositorio, con evidencia concreta y alcance. No equivale a calidad jugable, aprobación ni prueba en Unity. |
| NO VERIFICADO | Información histórica o comportamiento/resultado sin confirmación actual suficiente. |

**Excepción de rigor visual:** las correcciones del pelo y la respiración no se documentan como IMPLEMENTADAS/terminadas sin comprobar su resultado real en Unity. Se puede acreditar la presencia de sus archivos sin certificar la corrección visual. Registrar por separado inspección, compilación, Play Mode, revisión visual y aprobación del usuario.

## DECIDIDO — alcance y proceso

YESTERFALL es la identidad de producto actual; nombres técnicos `DiasPrestados` se conservan. Diseño vigente distribuido desde [AGENTS](../AGENTS.md). Esta tarea solo documenta. No autoriza Hito 2, nuevas mecánicas ni retoques artísticos.

## IMPLEMENTADO — evidencia técnica actual

| Área | Archivos o configuración comprobados | Límite |
|---|---|---|
| Motor | `ProjectSettings/ProjectVersion.txt`: Unity `6000.6.8f1`. | No demuestra apertura correcta de escenas. |
| Paquetes | `Packages/manifest.json`: URP `17.6.0`, Input System `1.20.1`, Test Framework `1.8.0`. | No reinstalados ni ejecutados. |
| Configuración | `ProjectSettings/ProjectSettings.asset`: `productName: DiasPrestados`, entrada moderna; 1920×1080. | Identidad técnica antigua; no se renombra aquí. |
| Escenas | `Assets/Scenes/ForestOrganicDemo.unity` y `DiasPrestadosDemo.unity`, con sus `.meta`. | Existen dos escenas de juego, no una sola. |
| Build | `ProjectSettings/EditorBuildSettings.asset`: solo ForestOrganicDemo habilitada. | DiasPrestadosDemo no está habilitada; no se cambió. No acredita build exportada. |
| Movimiento | `Assets/Scripts/Player/PlayerController2D.cs`: Input System, velocidad 2D, suelo, coyote time/jump buffer y giro visual. | Fluidez, salto y colisiones pendientes de prueba actual. |
| Entrada | `Assets/Input/PrototypeControls.inputactions`: Move, Jump, Interact; teclado, ↑/Espacio y E. | Mando/apuntado no acreditados por este asset. |
| Cámara/ambiente | `CameraFollow2D.cs`, `ParallaxLayer.cs`, `AmbientDustParticles.cs`, `AtmosphereVignette.cs`; Light2D y cámara URP serializados en demo. | Calidad/render final no verificados. |
| Personajes | Sets PNG del protagonista y `LittleBrother_Png`; `Player.controller` y `CompanionSpriteAnimator.cs`. | Presencia y configuración, no aceptación artística. |
| Compañero | `CompanionFollower.cs`: Following/Asking/Boosted/Cheering, sondeos, salto y recuperación antiatasco; componentes serializados en demo. | Es base de introducción, no IA permanente del juego completo. |
| Interacción/minipuzle | `Interactable`, `InteractionSensor`, `InteractionPrompt`, `CompanionHelpZone`, `CompanionBoostInteractable`, `PushableCrate`; caja y componentes en demo. | Resolución y ausencia de bloqueos sin prueba actual. |
| Recuperación | `PlayerFallRecovery.cs`: posición segura en memoria; caja con restitución al origen. | No son guardado persistente ni sistema completo de muerte. |
| UI provisional | `DemoHudOverlay.cs`, `DemoMessageZone.cs`, componentes en demo. | Controles/mensaje por X; no sistema completo de objetivos. |
| Herramientas | Menús de abrir escenas, ruta Forest, colliders y autotest Hito 1; `Assets/Scripts/Testing/DemoAutoTestRunner.cs`. | Existencia de herramientas no es ejecución reciente. |
| Cabra Chronoram | `CabraWalker.cs`, `CabraSpriteSet.cs`, `CabraChronoram.asset` y objeto `Cabra` en `DiasPrestadosDemo.unity`. Fotogramas de `walking`, `turn head` y `turn around`. | 2026-11-21: cuernos a la altura del hombro del hermano mayor (escala 0,48; paso ~666 px/u, `turn head` ~662, `turn around` ~792). En el giro el pivote sujeta el torso, no el casco: antes el cuerpo se iba hasta 0,36 u y un píxel suelto de `turn head` 0004 lo sacaba medio cuerpo. Collider 0,91 × 0,55. Sin fotogramas interpolados. No probado en Play Mode. |

Sorting layers comprobadas en `ProjectSettings/TagManager.asset`: Background, Midground, Gameplay y Foreground (además de Default).

## NO VERIFICADO — pelo, Idle y calidad visual

| Elemento | Hecho comprobado en archivos | Resultado visual pendiente |
|---|---|---|
| Walk del mayor | Ocho PNG en `Assets/Art/Characters/Protagonist/Walk/`; clip Walk basado en sprites. | Cabeza juvenil, pelo posterior redondeado, nuca sin rojo incorrecto y ciclo sin saltos. No declarar todo el set aprobado. |
| Correcciones de pelo | Historial de ajustes y propuesta individual de walk_01; el usuario rechazó recortes previos y pidió aprobación antes de extender el criterio. | No consta aprobación explícita de la última propuesta ni de su extensión a los otros siete. No retocar para «completar» sin petición. |
| Idle del mayor | `Idle/idle_0.png`, `idle_1.png`; `Player_Idle.anim`: cinco muestras (0, 0, 0, 1, 1), 3 fps, ~1,67 s. `PlayerProtagonistAnimator.cs` mantiene Transform constante y detiene Idle en aire si ese es el estado activo. | Sutileza, torso/hombros, pies estables, ausencia de rebote y salida al caminar, observados en Unity. |
| Idle del pequeño | `LittleBrother_Png/idle_0.png`, `idle_1.png`; array de cinco entradas en demo (0, 0, 0, 1, 1), `idleFps: 3`. | Referencia principal. Coincidir en timing no prueba equivalencia visual. |

**Acceso efectivo:** el servicio de sesiones conectadas no devolvió ninguna sesión. No hubo acceso visual efectivo al Editor/Play Mode; no se afirma una comparación en Unity. Una vista HTML o PNG externa no sustituye esa prueba.

En una tarea anterior se compiló código con referencias instaladas y se compararon píxeles/poses fuera de Unity. Eso es antecedente técnico, no validación visual en juego ni aceptación del usuario, y no se ha vuelto a compilar para esta documentación.

## PENDIENTE — sistemas de producto

En los scripts runtime revisados no se encontraron combate/disparo/daño, enemigos/jefes, árbol de habilidades, artefacto/objetos temporales, separación narrativa, menú principal/pausa completo, objetivos por eventos, mapa, guardado/carga persistente ni integración de reproducción/mezcla de audio. Su ausencia en esta inspección selectiva no es una auditoría exhaustiva de todos los assets posibles; no acreditarlos como implementados.

Recursos de audio/licencias, soporte de mando, localización, accesibilidad, integración Steam y build distribuible: pendientes y sin validación actual. Ver documentos especializados.

## Problemas y discrepancias conocidos

- No hay validación visual reciente del pelo/Idle ni revisión jugable actual del Hito 1. No afirmar ausencia de Missing Scripts solo por leer YAML.
- La demo por hitos y la escena habilitada en Build son distintas. Determinar la escena del encargo antes de trabajar; no cambiarla automáticamente.
- El antiguo plan prolongaba el tramo de hermanos; el diseño actual pide separación a los 60–90 s. Replanificar con autorización.
- Los documentos anteriores pedían regenerar Walk, indicaban Idle fijo y afirmaban una escena única. Esas instrucciones quedaron sustituidas; sus diagnósticos se conservan como historia.
- El arte provisional y las referencias ilustradas no constituyen un estilo final aprobado. La calidad se revisa sin cambiar assets protegidos por iniciativa propia.
- Los documentos antiguos informan autotests «TODO CORRECTO» y compilaciones. No se inspeccionaron informes en Temp/Library ni se repitieron pruebas; se mantienen como NO VERIFICADO actualmente.

## PROPUESTO — próxima tarea pequeña

Revisión manual autorizada en `DiasPrestadosDemo.unity`: ambos personajes quietos durante varios ciclos, salida Idle→Walk→Idle, salto/aterrizaje y revisión de contorno/nuca. Registrar escena, versión, resultado y aprobación sin editar arte de entrada. Después, revisar movimiento/caja/ayuda antes de discutir nuevos hitos.

## Historial breve y mantenimiento

- 2026-11-21: cabra al hombro del hermano mayor. El giro ya no persigue el casco: el torso se queda quieto. Sin Play Mode.
- 2026-11-08: documentación maestra YESTERFALL, separación de diseño/evidencia y retirada de instrucciones históricas incompatibles. Hito 1 sigue pendiente de revisión; Hito 2 no iniciado por esta tarea.
- Al cambiar una función, actualizar su fila con archivos y pruebas concretas. Al cambiar una decisión, actualizar el documento de diseño y una nota breve de sustitución. No convertir propuestas ni informes antiguos en verificaciones actuales.
