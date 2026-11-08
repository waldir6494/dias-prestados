# YESTERFALL — Mundo, arte y animación

Actualizado: 2026-11-08. Evidencias y límites: [ProjectStatus](ProjectStatus.md).

## DECIDIDO

- Mundo 2D con regiones diferenciadas, conexiones, caminos secundarios, secretos y recompensas. Habilidades abren accesos y dan motivos para regresar. Bosque y estación ferroviaria como entorno inicial.
- Escenarios detallados de estética ilustrada, con capas, parallax, iluminación ambiental, niebla, partículas, profundidad, vegetación y objetos animados, ruinas, edificios y maquinaria. Son objetivos de calidad, no una lista de efectos ya verificados.
- Identidad propia «Memoria desvanecida»: azul pizarra, verde grisáceo y ámbar cálido; violeta reservado a anomalías temporales. Plataformas y objetos interactivos legibles frente a la decoración.
- Evitar mezclas indiscriminadas de pixel art, cartoon y realismo. Ajustar el estilo de producción a lo existente, sin sustituir arte que funciona por iniciativa propia.
- Recursos gratuitos o específicos del proyecto con licencia comercial comprobada. Asset Store, itch.io y otras fuentes pueden considerarse si los recursos están disponibles y autorizados. Registrar procedencia, licencia y atribución antes de integrar.
- `Docs/VisualReferences/` y `Docs/ArtDirection/References/` son concept art y antecedentes de dirección; no son sprites finales ni autorización para recortarlos como niveles.

## Personajes y protección visual — DECIDIDO

- Mayor de ~12 años: pelo marrón/castaño oscuro con cabeza juvenil y redondeada; curva posterior orgánica desde coronilla a nuca. Sin exceso de cráneo, casco, pared vertical ni recorte hundido. La transición pelo/nuca/bufanda debe ser clara, sin franja rojiza incorrecta; conservar la bufanda roja/ladrillo real bajo el cuello. Ropa azul/gris y mochila marrón.
- Pequeño de ~8 años: pelo castaño, gorro de lana con pompón, chaqueta mostaza y mochila pequeña. Mantener identidad y proporciones de ambos.
- Ya se trabajaron Walk y silueta del mayor: **no volver a modificar esos sprites sin petición específica**. No cambiar proporciones, escala, encuadre, alineación, timing ni animaciones que funcionan sin autorización.
- Idle del mayor: respiración natural y muy sutil, usando como referencia principal la respiración actual del pequeño. Movimiento localizado en parte superior, especialmente torso/hombros; pies, piernas y posición general estables. Sin desplazar o escalar todo el personaje, sin rebote; detener la respiración al caminar y no interferir con otros estados.
- Conservar coherencia entre frames. Al revisar al mayor no modificar al pequeño. Validar ambos juntos en Unity antes de declarar equivalencia visual o corrección terminada.

## IMPLEMENTADO — inventario, no aceptación visual

| Recurso presente | Evidencia |
|---|---|
| Protagonista | `Assets/Art/Characters/Protagonist/Walk/walk_01.png` … `walk_08.png`; `Animations/Player_Walk.anim`, `Player.controller`. |
| Idle del protagonista | `Protagonist/Idle/idle_0.png`, `idle_1.png`; `Animations/Player_Idle.anim`, con cinco muestras a 3 fps y duración configurada ~1,67 s. |
| Control de animación | `Assets/Scripts/Player/PlayerProtagonistAnimator.cs`: Walk por distancia y pose de Transform constante; ya no contiene el seno de traslación/escala de respiración anterior. |
| Hermano por PNG | `Assets/Art/Characters/LittleBrother_Png/`: Idle, Walk, Jump, Fall, Ask y Cheer; `CompanionSpriteAnimator.cs`. |
| Fuente alternativa del pequeño | `Assets/Art/Characters/LittleBrother_Alt/brother_alt_profile.png`; no confundir con el set animado actual. |
| Entorno | Carpetas `Forest`, `ForestAtmosphere`, `ForestOrganic`, `Demo`; componentes ParallaxLayer, AmbientDustParticles, AtmosphereVignette y Light2D en la escena demo. |

## PROPUESTO

Nuevas regiones con lenguaje visual propio, conexiones y jefes. El arte de producción y objetos ambientales animados se concretarán con tareas delimitadas. No inventar cinco capítulos cerrados.

## PENDIENTE

Cerrar técnica/estilo de producción, mapa de regiones y conexiones, necesidades de assets y licencias. La decisión de producir arte futuro no permite regenerar los Walk actuales.

## NO VERIFICADO — correcciones recientes

La inspección de archivos confirma recursos y configuración, **no confirma en Unity** que el pelo de todos los Walk esté aprobado ni que Idle del mayor sea visualmente equivalente al pequeño. En las revisiones previas se rechazó un recorte posterior; después se preparó una propuesta de `walk_01` para revisión individual. No hay aprobación explícita registrada aquí para extender ese criterio a los otros siete.

Los trabajos de silueta y respiración son antecedentes conocidos; su resultado visual final permanece NO VERIFICADO en esta auditoría. No marcarlos como correcciones IMPLEMENTADAS/aceptadas por la sola presencia de PNG o código. Tampoco usar la falta de validación como permiso para retocarlos.

## Historial breve

- Antes: documentación de animación pedía regenerar Walk con estilo painterly y describía Idle como `walk_01` fijo.
- 2026-11-08: se protege el set actual; Idle tiene dos PNG propios. Se conservan los diagnósticos antiguos como historial, retirando su carácter de orden de regeneración.
