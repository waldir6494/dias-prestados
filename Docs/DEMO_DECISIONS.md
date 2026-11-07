# Días Prestados — Decisiones

## D1. Escena nueva `DiasPrestadosDemo` frente a la regla de "escena única"
`AGENTS.md` fijaba `ForestOrganicDemo` como única escena. El prompt maestro pide una escena nueva e independiente para la demo, que prevalece. `ForestOrganicDemo` no se toca; `AGENTS.md` se actualiza para documentar ambas.

## D2. Arte provisional generado por código
Sprites de personajes y entorno se generan desde `Assets/Editor/Demo` (sin descargas ni servicios de pago). Siguen las referencias de `Docs/VisualReferences`. Sustituibles por arte final conservando sorting layers y factores de parallax.

## D3. Capa "Companion"
El pequeño vive en una capa propia, excluida de la máscara de suelo del jugador, para que no cuente como suelo ni lo empuje. Además ignora la colisión con el protagonista.

## D4. Colliders sin fricción y retención en pendientes
Materiales sin fricción para evitar atascos en paredes; al estar quieto en suelo, el personaje retiene posición (`holdOnSlopesWhenIdle` / retención del compañero) para no resbalar.

## D5. Interacción por sensor
`InteractionSensor` busca `Interactable` por distancia en lugar de usar triggers, porque la detección de suelo del jugador trataría los triggers como suelo.

## D6. Compañero con máquina de estados
`CompanionFollower` (Following / Asking / Boosted / Cheering) con sondeos de pared y hueco, salto automático de obstáculos pequeños y teletransporte anti-atasco. Pide ayuda solo dentro de una `CompanionHelpZone`.

## D7. Risco casi vertical
El risco (2,3 u) tiene un frente casi vertical y el pequeño salta menos de lo necesario (`maxStepHeight` 1,5): así la interacción de ayuda es obligatoria y no se esquiva trepando por la pendiente.

## D8. Máscaras de capa en el builder
El valor de `LayerMask` serializado es `uint`; asignar un entero negativo lo dejaba a 0 (nada detectado como suelo). Se usa `0x7FFFFFFF & ~(1 << capa)`.

## D9. Autotest con Input System
Teclado virtual vía `InputSystem.QueueStateEvent`; la pulsación de E se deja procesar por el update dinámico para que `WasPressedThisFrame` la detecte. El runner está en `Assets/Scripts/Testing` bajo `#if UNITY_EDITOR` (un script de `Editor/` no puede añadirse como componente).

## D10. Git
Rama de trabajo `feature/prototipo-jugable`; sin commits ni push hasta que el usuario lo pida.
