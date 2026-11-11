# YESTERFALL — Historia

Actualizado: 2026-11-08. Relacionado: [GameDesign](GameDesign.md), [WorldArt](WorldArt.md), [UI](UI.md).

## DECIDIDO

- Protagonistas: Dos hermanos. El mayor (jugador) y el pequeño (con gorro de pompón y chaqueta mostaza).
- El detonante (Hito 1): Los hermanos se dirigen a la estación de tren por el bosque, como siempre. Ese día hay una niebla inédita y se escuchan murmullos inquietantes. El hermano mayor lleva en su mochila un candado, regalo de un anciano cerrajero que llegó al pueblo una tarde nublada y siempre fue amable con él. Al entrar en la niebla, el candado brilla y crece. A medida que avanzan ocurre un destello de luz, el hermano menor desaparece.
- El desarrollo: El protagonista avanza y llega a un pequeño pueblo vacío, infestado de criaturas hostiles (ratas y avispas extrañas), donde descubre que el candado envejece su entorno para abrirle paso pero solo en lugares determinados.
- PENDIENTE DESARROLLO DE HISTORIA EN HITOS 2 - 6. 
- El causante (Hito 7): El protagonista se enfrenta a un jefe con poderes temporales. Este enemigo es el responsable de haber secuestrado al hermano menor, y posteriormente enviarlo al pasado. Además, le revela al protagonista que el uso del candado tiene un alto precio.
- El giro final y resolución (Hito 8): Al final de la aventura, el protagonista ve a lo lejos una silueta envejecida que lleva la misma ropa de su hermano, incluyendo la chaqueta mostaza. Al girarse, descubre que es el anciano cerrajero del pueblo. El abuelo/hermano siempre supo quién era el protagonista, se dedicó a la cerrajería y le entregó el candado sabiendo que lo necesitaría para sobrevivir a este bucle. El juego termina con el protagonista abrazando fuertemente al anciano mientras suena música triste.
- Narrativa: Ambiental, con textos breves. Sin diálogos doblados ni voces.

- El mayor encuentra o utiliza un artefacto que acelera el envejecimiento de determinados objetos para transformar el escenario. Reglas jugables en [Puzzles](Puzzles.md).
- Introducción conjunta de aproximadamente 60–90 segundos; el pequeño desaparece pronto. Después la aventura es individual. La IA existente sirve a la introducción, no implica compañero permanente ni cooperativo.
- Sin voces ni doblaje. Narrativa visual con introducción escrita, textos breves, mensajes contextuales, notas, recuerdos o documentos. Evitar interrupciones y textos extensos.

## PROPUESTO

Giro provisional: el tiempo exterior continúa mientras el mayor permanece atrapado. El pequeño crece y envejece sin dejar de esperarlo; el reencuentro final debe ser emotivo. No es un guion cerrado. Las láminas antiguas del anciano son antecedentes de esta propuesta.
- Diseño de las "ratas y avispas extrañas" con toques temporales o mutaciones causadas por la misma anomalía.
- Dejar pistas ambientales sutiles en el pueblo que sugieran la presencia de un cerrajero (el hermano) preparándose durante años para este momento.

## PENDIENTE

- Redacción de los textos breves y mensajes del HUD.
- Concept art para los enemigos (ratas/avispas) y el jefe del Hito 7.
- Bloqueo y coreografía de la cinemática o secuencia interactiva del abrazo final.

## IMPLEMENTADO

El repositorio contiene ambos personajes, seguimiento/ayuda del pequeño en `DiasPrestadosDemo.unity` y mensajes de controles/fin de prueba. Esto no implementa por sí mismo la introducción narrativa ni la separación.

## NO VERIFICADO

No se ha visto en Unity la separación ni un reencuentro. No se encontraron sistemas narrativos equivalentes en los scripts runtime revisados. El final de la primera prueba no es el final argumental de la demo.

## Historial breve

- 2026-11-08: «sin diálogos» se precisa como ausencia de voces/doblaje, admitiendo textos breves. El tramo conjunto de ~2 minutos del plan anterior queda sujeto al objetivo actual de 60–90 segundos, sin editar escenas en esta tarea.
