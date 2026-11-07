# AGENTS.md — Días Prestados

Guía para agentes y desarrolladores que trabajen en este repositorio.

## Qué es el proyecto

**Días Prestados** es un juego indie cinematográfico 2D de desplazamiento lateral. Dos hermanos quedan separados por un fenómeno temporal. El tono combina misterio, puzles, sigilo y narrativa visual **sin diálogos**.

- Motor: **Unity 6.6** (`6000.6.8f1`)
- Plantilla: **Universal 2D (URP)**
- Plataforma objetivo actual: **Windows**
- Idioma de código y comentarios de diseño: **C# / español** cuando aporte claridad

## Estado actual

Base jugable de prototipo:

- Controlador 2D con Input System
- Escena generable `Assets/Scenes/Prototype.unity`
- Arte provisional procedural (sprites PNG persistentes)
- **Aún no** hay puzles temporales, enemigos ni sistemas narrativos avanzados

## Cómo generar el prototipo

1. Abrir el proyecto en Unity 6.6.
2. Menú: **Tools → Dias Prestados → Crear prototipo**.
3. Si `Prototype.unity` ya existe, confirmar el reemplazo (`SampleScene` no se toca).
4. Abrir `Assets/Scenes/Prototype.unity` (el generador la deja abierta/seleccionada).
5. Pulsar **Play**.
6. Controles: **A/D** o **flechas** para mover, **Espacio** para saltar.

## Estructura relevante

```
Assets/
  Art/Prototype/          # Sprites generados (persistentes)
  Editor/                 # Herramientas de editor (solo Editor)
  Input/                  # Input Actions del prototipo
  Scripts/
    Camera/               # Seguimiento de cámara
    Player/               # Controlador del personaje
  Scenes/
    Prototype.unity       # Escena jugable generada
    SampleScene.unity     # Escena plantilla — no sobrescribir
  Settings/               # URP / Input System plantilla
```

## Convenciones técnicas

### Unity 6 / física 2D

- Usar `Rigidbody2D.linearVelocity` (no `velocity`).
- Preferir APIs Unity 6 renombradas (`linearDamping`, etc.) cuando existan.
- Personajes dinámicos: `FreezeRotation`, `CollisionDetectionMode2D.Continuous`, interpolación activada.
- Detección de suelo fiable (OverlapBox + coyote time + jump buffer).

### Input

- **Solo** Input System moderno (`com.unity.inputsystem`).
- No usar `Input.GetAxis` / `Input.GetKey` legacy.
- Acciones del prototipo: `Assets/Input/PrototypeControls.inputactions`.
- El proyecto usa `activeInputHandler: Input System Package` (sin legacy).

### Render / URP 2D

- Mantener configuración URP 2D existente en `Assets/Settings/`.
- Escenas 2D deben incluir `Light2D` global y cámara con `UniversalAdditionalCameraData`.
- No romper ni sustituir el pipeline sin necesidad explícita.

### Organización de código

- Runtime: `Assets/Scripts/...`
- Editor: `Assets/Editor/...`
- Namespace raíz: `DiasPrestados` (editor: `DiasPrestados.Editor`)
- No añadir dependencias de pago ni paquetes innecesarios.
- No modificar archivos ajenos al objetivo de la tarea.
- Conservar `.meta` existentes; dejar que Unity genere los de archivos nuevos.

### Escenas y contenido generado

- **Nunca** sobrescribir `SampleScene.unity` como parte de herramientas internas.
- Si un generador puede reemplazar `Prototype.unity`, pedir confirmación.
- Los assets creados por herramientas deben persistir en disco (`AssetDatabase`, PNG, escenas guardadas con `EditorSceneManager`).

## Prioridades de diseño (producto)

1. Base jugable estable (movimiento, cámara, escenario).
2. Narrativa visual y atmósfera.
3. Puzles temporales / mecánica de “días prestados”.
4. Sigilo y tensión.
5. Pulido de arte y audio.

No implementar sistemas futuros (puzles temporales, enemigos, UI narrativa completa) hasta que la base pedida esté sólida.

## Qué evitar

- Afirmar que Unity compiló o que Play Mode funciona sin haberlo verificado.
- Commits no pedidos por el usuario.
- Refactors amplios no solicitados.
- Sustituir Input System por Input legacy.
- Introducir assets externos de pago o SDKs no solicitados.

## Pruebas mínimas recomendadas tras cambios de prototipo

- Menú de generación ejecuta sin errores en Console.
- Escena guarda en `Assets/Scenes/Prototype.unity`.
- Play Mode: correr, saltar a las 4 plataformas, subir el desnivel, superar obstáculos.
- Cámara sigue al jugador y respeta límites horizontales.
- Cerrar/reabrir Unity: sprites y escena siguen presentes.
