# YESTERFALL — Puzles

Actualizado: 2026-11-08. Relacionado: [Combat](Combat.md), [Skills](Skills.md), [Story](Story.md).

## DECIDIDO

- Puzles centrales, inteligentes, variados, lógicos y satisfactorios; dificultad progresiva sin frustración excesiva. El reto viene del razonamiento, no de controles imprecisos.
- Familias previstas: manipulación temporal, mecanismos físicos, interruptores/puertas, rebotes y perforación de proyectiles, plataformas/estructuras móviles, observación y combinación de habilidades.
- El artefacto (candado temporal) envejece objetos compatibles hacia delante. El protagonista es estrictamente inmune a sus efectos, lo que genera una falsa percepción de control sobre el tiempo.
- Evitar repetir crecimiento de árboles/raíces o rotura de obstáculos. Pistas naturales y objetivos que orienten sin entregar la solución.
- Reinicio seguro: sin estados irrecuperables por orden de acciones, pérdida de objetos o munición. Señalar superficies y objetos compatibles con coherencia.

## PROPUESTO

- Registro de coste temporal: Implementar un contador invisible en el sistema de guardado (SaveSystem) que sume cada uso del candado. Este contador será el respaldo mecánico para justificar el paso del tiempo en el final del juego.
- Ejemplos previos: oxidar un candado, hacer crecer una planta como apoyo, deteriorar una cuerda para liberar contrapeso y resolver una vagoneta/palanca.

## PENDIENTE

Primer puzle de la demo ampliada, reglas/feedback del artefacto, combinación con armas, dificultad y variedad de soluciones. El diseño antiguo «sin texto» no impide pistas escritas breves según el criterio narrativo actual.

## IMPLEMENTADO

Existen `Assets/Scripts/Interaction/PushableCrate.cs` (caja y restitución al salir de zona), `Interactable.cs`, `InteractionSensor.cs`, `InteractionPrompt.cs` y componentes `CompanionHelpZone` / `CompanionBoostInteractable`. `DiasPrestadosDemo.unity` contiene `Crate_Pushable` y componentes de interacción/ayuda.

## NO VERIFICADO

El recorrido caja → risco → ayuda no se ha probado de nuevo en Unity. Los autotests antiguos son antecedentes, no validación actual. No se encontraron artefacto/objetos temporales ni puzles balísticos en los scripts runtime revisados.
