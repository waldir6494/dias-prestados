# YESTERFALL — Interfaz, objetivos y controles

Actualizado: 2026-11-08. Relacionado: [Story](Story.md), [Combat](Combat.md), [SaveSystem](SaveSystem.md).

## DECIDIDO

- Menú principal cinematográfico y panorámico, coherente con una estación ferroviaria misteriosa: iluminación atmosférica, niebla/partículas, música, título elegante y transiciones suaves. Opciones integradas sobre el escenario; evitar apariencia de aplicación móvil.
- Menú previsto: Continuar, Nueva partida, Opciones, Créditos y Salir. También pausa, HUD de vida/munición, mapa, inventario cuando sea necesario y tutoriales integrados.
- Objetivo principal arriba a la izquierda; notificación al actualizarlo y consulta desde pausa. Cambios por eventos reales de gameplay. Orientar sin flechas gigantes constantes ni revelar la solución de puzles.
- Ajustes previstos: resolución, pantalla completa/ventana, VSync, límite de FPS, calidad gráfica cuando corresponda, volúmenes general/música/efectos, controles y reasignación, mando, accesibilidad e idiomas.
- Teclado/ratón y mando desde el diseño. Priorizar español y contemplar inglés para Steam. Diseñar para 16:9, 16:10 y otras proporciones habituales; no asumir soporte ya existente.

## PROPUESTO

Ejemplos de objetivos: «Sigue a tu hermano», «Encuentra a tu hermano», «Busca una entrada a la estación», «Activa el mecanismo del acceso», «Enfréntate al guardián». No son misiones cerradas ni evidencia de un guardián implementado.

## PENDIENTE

Diseño concreto del HUD, pausa, navegación con mando, retícula, localización, accesibilidad, mapa y ajustes. Definir eventos de objetivos y qué información se guarda, sin implementar ahora un sistema completo.

## IMPLEMENTADO

`Assets/Scripts/UI/DemoHudOverlay.cs`: controles temporales y mensajes mediante IMGUI. `DemoMessageZone.cs`: mensaje al superar una coordenada X. `InteractionPrompt.cs`: indicación de interacción. La demo serializa estos componentes.

`Assets/Input/PrototypeControls.inputactions` contiene Move/Jump/Interact con teclado; ↑ y Espacio están configurados para salto y E para interacción. Hay esquema Keyboard&Mouse, pero eso no acredita apuntado con ratón. No se encontraron bindings de mando en ese archivo.

## NO VERIFICADO

El HUD actual no equivale a sistema de objetivos, pausa ni menú principal. No se ha validado en Unity navegación, reasignación, mando, idiomas o adaptación de aspecto. Resolución 1920×1080 en ProjectSettings no prueba compatibilidad con otras proporciones.
