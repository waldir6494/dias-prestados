# AGENTS.md — Días Prestados

Guía para agentes y desarrolladores que trabajen en este repositorio.

## Qué es el proyecto

**Días Prestados** es un juego indie cinematográfico 2D de desplazamiento lateral. Dos hermanos quedan separados por un fenómeno temporal. El tono combina misterio, puzles, sigilo y narrativa visual **sin diálogos**.

- Motor: **Unity 6.6** (`6000.6.8f1`)
- Plantilla: **Universal 2D (URP)**
- Plataforma objetivo actual: **Windows**
- Idioma de código y comentarios de diseño: **C# / español** cuando aporte claridad

## Estado actual

Base jugable + demo atmosférica de bosque:

- Controlador 2D con Input System (`PlayerController2D`)
- Cámara con seguimiento (`CameraFollow2D`)
- Escena base: `Assets/Scenes/Prototype.unity`
- Escena atmosférica: `Assets/Scenes/ForestDemo.unity` (parallax, atardecer, estación, luces URP 2D)
- Arte provisional procedural (PNG persistentes en `Assets/Art`)
- **Aún no** hay puzles temporales, enemigos ni sistemas narrativos avanzados

## Cómo generar / abrir escenas

### Prototipo jugable

1. **Tools → Dias Prestados → Crear prototipo**
2. Abrir `Assets/Scenes/Prototype.unity`
3. Play — controles: **A/D** o flechas ←/→; salto: **↑**

### Forest Demo (bosque misterioso)

1. Debe existir `Prototype.unity` (no se destruye).
2. **Tools → Dias Prestados → Crear / actualizar Forest Demo**
3. Si `ForestDemo` no existe: copia Prototype → ForestDemo y aplica atmósfera.
4. Si ya existe: solo regenera nodos gestionados (`ForestVisuals`, estilo visual, luces cálidas, partículas). **No** sustituye el Player ni recrea colisiones desde cero.
5. Abrir `Assets/Scenes/ForestDemo.unity` y pulsar Play.

### Forest Atmosphere Demo (Fase 1 "Memoria desvanecida")

1. Debe existir `ForestDemo.unity` (no se modifica).
2. **Tools → Dias Prestados → Crear / actualizar Forest Atmosphere Demo**
3. Copia ForestDemo → `ForestAtmosphereDemo.unity` y aplica paleta fría, niebla por capas, luz global clara, acentos cálidos (#E8A84A), motas suaves y viñeta mínima (`AtmosphereVignette`).
4. No toca colliders, posiciones ni escalas de gameplay (los sprites nuevos conservan exactamente los bounds).
5. Paleta: `#1B2733`, `#3A4A4C`, `#7C8A8A`, `#B9B3A0`, acento `#E8A84A` (ver `AtmospherePalette`).
6. Código: `ForestAtmosphereArtGenerator.cs`, `ForestAtmosphereSceneBuilder.cs`, arte en `Assets/Art/ForestAtmosphere/`.

### Forest Organic Demo (tramo de prueba)

1. **Tools → Dias Prestados → Crear / actualizar Forest Organic Demo** (parte de `ForestAtmosphereDemo.unity`, que no se modifica).
2. Abrir `Assets/Scenes/ForestOrganicDemo.unity`. Geometría única en `Assets/Editor/ForestOrganicLayout.cs` (sprites y colliders salen de ella).
3. Comprobación geométrica en Edit Mode: **Tools → Dias Prestados → Comprobar ruta Forest Organic**.

## Estructura relevante

```
Assets/
  Art/
    Prototype/            # Sprites del prototipo
    Forest/               # Sprites del bosque (cielo, montañas, árboles, estación…)
  Editor/                 # Generadores de escena/arte
  Input/                  # PrototypeControls.inputactions
  Scripts/
    Camera/
    Environment/          # ParallaxLayer, AmbientDustParticles
    Player/
  Scenes/
    Prototype.unity       # Base jugable — conservar
    ForestDemo.unity      # Demo atmosférica — a partir de Prototype
    SampleScene.unity     # Plantilla — no sobrescribir
  Settings/               # URP / Input System plantilla
```

## Limitaciones artísticas (provisional)

- Sprites generados por código (siluetas de baja resolución, sin animación).
- No son arte final: sirven para tono, composición, parallax e iluminación.
- Sustituibles más adelante por assets de producción manteniendo sorting layers / factores de parallax.

## Convenciones técnicas

### Unity 6 / física 2D

- Usar `Rigidbody2D.linearVelocity` (no `velocity`).
- Preferir APIs Unity 6 renombradas (`linearDamping`, etc.) cuando existan.
- Personajes dinámicos: `FreezeRotation`, `CollisionDetectionMode2D.Continuous`, interpolación activada.
- Detección de suelo fiable (OverlapBox + coyote time + jump buffer).
- Decoración/fondos: **sin Collider2D** que interfiera con el jugador.

### Input

- **Solo** Input System moderno (`com.unity.inputsystem`).
- No usar `Input.GetAxis` / `Input.GetKey` legacy.
- Acciones del prototipo: `Assets/Input/PrototypeControls.inputactions`.

### Render / URP 2D

- Mantener configuración URP 2D en `Assets/Settings/`.
- Escenas 2D: `Light2D` global + cámara con `UniversalAdditionalCameraData`.
- ForestDemo: ambiente global ligeramente frío + puntos cálidos (`Light2D` Point).

### Organización de código

- Runtime: `Assets/Scripts/...`
- Editor: `Assets/Editor/...`
- Namespace: `DiasPrestados` / `DiasPrestados.Editor`
- No dependencias de pago ni paquetes innecesarios.
- No commits/pushes/merges salvo petición explícita del usuario.

### Escenas y contenido generado

- **Nunca** sobrescribir `SampleScene.unity`.
- Conservar `Prototype.unity` al crear demos derivadas.
- Nodos regenerables deben tener nombre estable (`ForestVisuals`, `WarmLights`, `AmbientDust`, hijos `_ForestStyle*`).
- Assets persistentes vía `AssetDatabase` / PNG / `EditorSceneManager.SaveScene`.

## Prioridades de diseño (producto)

1. Base jugable estable
2. Narrativa visual y atmósfera ← (ForestDemo)
3. Puzles temporales
4. Sigilo y tensión
5. Pulido de arte y audio

## Qué evitar

- Afirmar compilación/Play Mode sin verificarlo.
- Commits no pedidos.
- Sustituir Input System por legacy.
- Escenas vacías o reemplazar el controlador del jugador sin necesidad.
- Descargas externas / assets de pago no solicitados.

## Pruebas mínimas

### Prototype

- Generación sin errores; Play: correr, 4 plataformas, desnivel, obstáculos.

### ForestDemo

- Abrir `ForestDemo.unity` (no Prototype).
- Play: mismo control/fisica que Prototype.
- Comprobar parallax al caminar, cielo atardecer, estación/raíles al fondo, luces cálidas, motas, que la decoración no empuje al jugador.
- Legibilidad: suelo/plataformas (tapa clara), cajas de madera, rocas sólidas vs `FG_Rock_*` decorativas, arbustos de bajo contraste.
- Depuración: **Tools → Dias Prestados → Mostrar colliders del escenario** (solo Scene View).
- Backup previo a cambios fuertes: `Assets/Scenes/Backups/`.
