# YESTERFALL — Habilidades y progresión

Actualizado: 2026-11-08. Relacionado: [Combat](Combat.md), [Puzzles](Puzzles.md), [SaveSystem](SaveSystem.md).

## DECIDIDO

Tres ramas iniciales de diseño:

| Rama | Capacidades previstas |
|---|---|
| Movimiento | Rodar, esquivar, doble salto, impulso aéreo y mejoras de movilidad. |
| Pistola | Perforación, rebotes, capacidad de munición y disparos especiales. |
| Arma de piedra | Lanzamiento/retorno, alcance, impactos múltiples y técnicas especiales. |

Conectan combate, puzles y exploración. Las habilidades indispensables para completar la historia se obtienen por progresión garantizada o tienen una alternativa. Gastar puntos opcionales incorrectamente nunca debe bloquear al jugador.

## PROPUESTO

Jefes que otorgan puntos de habilidad. Las técnicas especiales y los disparos cargados son provisionales; no interpretar cada entrada como función cerrada.

## PENDIENTE

Costes, requisitos, cantidad final, orden de desbloqueo, balance, presentación del árbol, recompensas garantizadas frente a opcionales y necesidad de reasignación. Definir qué caminos requieren cada habilidad y cómo se evita cada bloqueo.

## IMPLEMENTADO

Existe salto básico en `Assets/Scripts/Player/PlayerController2D.cs`. No acredita doble salto, esquiva, impulso aéreo ni árbol de habilidades. No se encontró un sistema de progresión equivalente en los scripts runtime revisados.

## NO VERIFICADO

Sin comprobación de desbloqueos, gasto de puntos o persistencia de habilidades en Unity. No crear arquitectura completa de progresión sin un encargo delimitado.
