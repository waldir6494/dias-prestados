# AGENTS.md — Días Prestados

Guía para agentes y desarrolladores que trabajen en este repositorio.

## Qué es el proyecto

**Días Prestados** es un juego indie cinematográfico 2D de desplazamiento lateral. Dos hermanos quedan separados por un fenómeno temporal. El tono combina misterio, puzles, sigilo y narrativa visual **sin diálogos**.

- Motor: **Unity 6.6** (`6000.6.8f1`)
- Plantilla: **Universal 2D (URP)**
- Plataforma objetivo actual: **Windows**
- Idioma de código y comentarios de diseño: **C# / español** cuando aporte claridad

## Escena principal (única)

**`Assets/Scenes/ForestOrganicDemo.unity`**

- Es la **única** escena de desarrollo y la escena inicial de Build (Windows).
- Integrar nuevas funcionalidades **sobre esta escena**. No crear demos nuevas por feature.
- Controles: **A/D** o ←/→; salto: **↑** (`Assets/Input/PrototypeControls.inputactions`).
- Abrir: Project → `Assets/Scenes/ForestOrganicDemo`, o **Tools → Dias Prestados → Abrir Forest Organic Demo** (solo abre; **nunca** regenera).
- Si falta el archivo: error claro; no hay bootstrap desde escenas antiguas (ya eliminadas).
- Geometría de referencia del tramo: `Assets/Editor/ForestOrganicLayout.cs`.
- Comprobación geométrica (Edit Mode): **Tools → Dias Prestados → Comprobar ruta Forest Organic**.
- Colliders en Scene View: **Tools → Dias Prestados → Mostrar colliders del escenario**.

## Estado actual

- Controlador: `PlayerController2D` + Input System
- Cámara: `CameraFollow2D`
- Atmósfera "Memoria desvanecida", terreno orgánico, parallax, niebla, luces URP 2D
- Arte provisional en `Assets/Art` (`ForestOrganic/`, `ForestAtmosphere/`, etc.)
- **Aún no** hay puzles temporales, enemigos ni sistemas narrativos avanzados

## Estructura relevante

```
Assets/
  Art/
    Prototype/            # Sprites compartidos (p. ej. jugador)
    Forest/               # Capas / siluetas de bosque (legado, en uso visual)
    ForestAtmosphere/     # Capas / paleta / motas usadas por la escena principal
    ForestOrganic/        # Terreno, tronco, peñasco, pasarela…
  Editor/                 # Herramientas (abrir escena, colliders, checks)
  Input/                  # PrototypeControls.inputactions
  Scripts/
    Camera/
    Environment/          # ParallaxLayer, AmbientDustParticles, AtmosphereVignette
    Player/
  Scenes/
    ForestOrganicDemo.unity   # ÚNICA escena
  Settings/               # URP / Input System
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
- Acciones: `Assets/Input/PrototypeControls.inputactions`.

### Render / URP 2D

- Mantener configuración URP 2D en `Assets/Settings/`.
- Escena 2D: `Light2D` global + cámara con `UniversalAdditionalCameraData`.

### Organización de código

- Runtime: `Assets/Scripts/...`
- Editor: `Assets/Editor/...`
- Namespace: `DiasPrestados` / `DiasPrestados.Editor`
- No dependencias de pago ni paquetes innecesarios.
- No commits/pushes/merges salvo petición explícita del usuario.

### Escenas

- Solo `ForestOrganicDemo.unity` en `Assets/Scenes`.
- **Nunca** regenerar ni sobrescribir la escena principal con generadores.
- Menús históricos de Prototype / ForestDemo / ForestAtmosphere están desactivados.
- Antes de cambios fuertes: backup manual fuera del repo o commit local (no hay carpeta `Scenes/Backups`).

## Prioridades de diseño (producto)

1. Base jugable estable ← (ForestOrganicDemo)
2. Narrativa visual y atmósfera
3. Puzles temporales
4. Sigilo y tensión
5. Pulido de arte y audio

## Qué evitar

- Afirmar compilación/Play Mode sin verificarlo.
- Commits no pedidos.
- Sustituir Input System por legacy.
- Crear escenas nuevas por cada feature.
- Descargas externas / assets de pago no solicitados.

## Pruebas mínimas

- Abrir `Assets/Scenes/ForestOrganicDemo.unity` (sin Missing Scripts).
- Play: moverse, saltar (↑), recorrer loma → tronco → peñasco → pasarela.
- Parallax, niebla, luces y motas intactos; decoración sin empujar al jugador.
- File → Build Settings: solo `ForestOrganicDemo` habilitada.
- Depuración: **Tools → Dias Prestados → Mostrar colliders del escenario**.
