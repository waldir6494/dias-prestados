# YESTERFALL — Diseño general

Actualizado: 2026-11-08. Estado técnico: [ProjectStatus](ProjectStatus.md). Hitos: [Roadmap](Roadmap.md).

## DECIDIDO

- Metroidvania 2D para un jugador: combate, exploración, secretos, puzles, progresión por habilidades y misterio narrativo emocional. Lanzamiento inicial en PC mediante Steam; consolas fuera del alcance actual.
- Identidad propia. Hollow Knight inspira exploración, habilidades y encuentros; INSIDE, atmósfera y puzles ambientales; Ori, profundidad y fluidez visual. No copiar personajes, composiciones, música ni recursos.
- Bucle: **Explorar → Descubrir obstáculos y enemigos → Resolver puzles o combatir → Conseguir recompensas o habilidades → Desbloquear caminos → Avanzar en la historia.**
- Debe ser divertido jugar sin acontecimientos narrativos. No limitar el juego a caminar y saltar ni a obstáculos lineales. Combate y habilidades interactúan con exploración y puzles; secretos y revisitas tienen recompensa.
- Sin voces ni doblaje; textos breves y contextuales permitidos. No cooperativo ni acompañante IA permanente. Detalles en [Story](Story.md).
- Primera meta: demo pequeña de calidad, orientativamente 10 minutos. Antes de ampliarla, estabilizar una prueba acotada y validar movimiento, combate y puzles individualmente; integrarlos solo con autorización.

## Calidad y aceptación — DECIDIDO

| Área | Criterio |
|---|---|
| Movimiento | Fluido y preciso; sensación comprobada con controles reales. |
| Salto y colisiones | Suelo fiable, trayectorias controlables y recorrido sin atascos; decoración no empuja. |
| Combate | Respuesta visual y sonora clara, impactos comprensibles y patrones comunicados. |
| Animaciones | Naturales, coherentes entre frames; Idle sin rebote ni pies flotantes. |
| Jefes | Desafiantes pero justos, con señales y oportunidades de reacción reconocibles. |
| Puzles | Razonamiento, variedad, controles precisos y reinicio seguro. |
| Objetivos | Comprensibles sin resolver automáticamente el desafío. |
| Escenarios | Coherencia artística, detalle, profundidad y lectura clara de rutas. |

Compilar o disponer de una función no completa un hito. Priorizar calidad y reutilización; probar piezas antes de combinarlas y recoger revisión del usuario.

## PROPUESTO

Un enfrentamiento especial o minijefe en la demo si resulta viable, y una recompensa o descubrimiento final que invite a continuar. La inclusión concreta requiere revisar el alcance.

## PENDIENTE

Duración del juego completo, alcance de la demo ampliada, orden de introducción de combate/habilidades/puzles, balance y estilo artístico de producción. Las listas de sistemas no autorizan su implementación conjunta.

## IMPLEMENTADO / NO VERIFICADO

Existe una base de movimiento, entorno y minipuzle de caja/ayuda en el repositorio. No se comprobó su calidad en Play Mode en esta auditoría. Combate, progresión metroidvania y bucle completo no están acreditados como implementados; ver evidencias y límites en ProjectStatus.

## Historial breve

- 2026-11-08: YESTERFALL sustituye a Días Prestados como identidad de producto. El diseño se amplía a metroidvania con combate y textos breves. No implica renombrar archivos ni aprobar nuevos hitos.
