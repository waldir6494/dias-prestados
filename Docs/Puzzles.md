# YESTERFALL — Puzles

Actualizado: 2026-11-08. Relacionado: [Combat](Combat.md), [Skills](Skills.md), [Story](Story.md).

## DECIDIDO

- Puzles centrales, inteligentes, variados, lógicos y satisfactorios; dificultad progresiva sin frustración excesiva. El reto viene del razonamiento, no de controles imprecisos.
- Familias previstas: manipulación temporal, mecanismos físicos, interruptores/puertas, rebotes y perforación de proyectiles, plataformas/estructuras móviles, observación y combinación de habilidades; pequeñas secuencias de sigilo o persecución.
- El artefacto envejece objetos compatibles hacia delante. No asumir rebobinado ni control global del tiempo; reglas e interacción exactas pendientes.
- Evitar repetir crecimiento de árboles/raíces o rotura de obstáculos. Pistas naturales y objetivos que orienten sin entregar la solución.
- Reinicio seguro: sin estados irrecuperables por orden de acciones, pérdida de objetos o munición. Señalar superficies y objetos compatibles con coherencia.

## PROPUESTO

Ejemplos previos: oxidar un candado, hacer crecer una planta como apoyo, deteriorar una cuerda para liberar contrapeso y resolver una vagoneta/palanca sin poder temporal. Son referencias para evaluar, no niveles cerrados ni autorización para implementarlos todos.

## PENDIENTE

Primer puzle de la demo ampliada, reglas/feedback del artefacto, combinación con armas, dificultad y variedad de soluciones. El diseño antiguo «sin texto» no impide pistas escritas breves según el criterio narrativo actual.

## IMPLEMENTADO

Existen `Assets/Scripts/Interaction/PushableCrate.cs` (caja y restitución al salir de zona), `Interactable.cs`, `InteractionSensor.cs`, `InteractionPrompt.cs` y componentes `CompanionHelpZone` / `CompanionBoostInteractable`. `DiasPrestadosDemo.unity` contiene `Crate_Pushable` y componentes de interacción/ayuda.

## NO VERIFICADO

El recorrido caja → risco → ayuda no se ha probado de nuevo en Unity. Los autotests antiguos son antecedentes, no validación actual. No se encontraron artefacto/objetos temporales ni puzles balísticos en los scripts runtime revisados.
