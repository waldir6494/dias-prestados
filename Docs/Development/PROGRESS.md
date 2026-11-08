# Progress — Animaciones del protagonista

> **INFORME HISTÓRICO — 2026-11-08.** Las conclusiones sobre regenerar el set, igualdad byte a byte con el zip e Idle fijo pertenecen a una versión anterior; no se certifican para los PNG actuales. No regenerar ni sustituir los Walk. El estado actual y las validaciones pendientes están en [ProjectStatus](../ProjectStatus.md); los criterios visuales vigentes en [WorldArt](../WorldArt.md). «Unity ausente» al final describe aquella ejecución, no un diagnóstico actual de instalación. No confundir el rechazo antiguo del arte con permiso para reemplazarlo ahora.

## Veredicto tras Play Mode de David

**Walk no es aceptable como arte final.** La causa principal de “partes cortadas / poca definición / acabado distinto” está en los **PNG originales del zip**, no en pérdida por extracción.

## 1) Causa de los recortes

| Hipótesis | Resultado |
|-----------|-----------|
| Píxeles perdidos al extraer el zip | **No.** `walk_01..08` en disco = **byte-idénticos** al zip. |
| Contenido fuera del lienzo 128×160 | **No** en bordes del canvas (márgenes L~31–36, R~40+, T~40, B~11–13; 0 opacos en borde). |
| Importer (Tight mesh / bilinear) | **Contribuye** a sensación de corte/blur. `spriteMeshType: Tight` + `filterMode: Bilinear` empeoran bordes blandos. |
| Clip anima Transform/escala | **No.** `Player_Walk.anim` solo curva `m_Sprite`. |
| Escala distinta Idle/Walk | **Contribuía** al salto visual. Eliminada. |
| Idle = `ProtagonistApproved` painterly | **Causa** del “reaparece el PNG estático / cambio inconsistente” y del PNG en Jump/Fall (Speed=0 → Idle painterly). |
| Arte fuente | **Sí.** Personaje ~50–55×105 px en canvas 128×160; bordes masticados / huecos (p.ej. cintura); estilo **pixel** ≠ PNG aprobado **painterly** ~149×486. |

**Conclusión:** hay que **regenerar** los frames Walk (y el resto) al estilo del PNG aprobado. No se “arregla” con upscale/filtros/HD falso.

## 2) Correcciones técnicas hechas

- Importer Walk: `spriteMeshType: Full Rect`, `filterMode: Point`, alpha OK, sin compresión, PPU 48, pivot pies.
- Idle del Animator = `walk_01` (mismo set), **no** `ProtagonistApproved`.
- Escala **única** `visualScale=0.76` en Visual (no por estado).
- Parámetro `Grounded`; Walk→Idle exige suelo (evita “Idle painterly” en aire).
- `Speed=|vx|` también en aire (misma familia visual).
- Default sprite del Visual = `walk_01`.
- Física / PlayerController2D / cámara / escena (salvo Visual) **sin tocar**.

## 3) Pendiente artístico (regenerar / entregar)

Reemplazar estos PNG (mismo diseño que `ProtagonistApproved`: painterly, pelo/bufanda/mochila/brazos/piernas/botas completos, alpha limpio, **sin** magenta bleed):

| Archivo | Lienzo sugerido | Pivot | Notas |
|---------|-----------------|-------|-------|
| `walk_01.png` … `walk_08.png` | Uniforme, p.ej. **256×384** o mayor (contenido ~≥180 px de alto) | Pies / bottom-center | Ciclo completo; sin huecos de cintura; stride sin truncar pies |
| `idle_01.png` … `idle_04.png` (aprox.) | Mismo lienzo/PPU que Walk | Pies | Sustituir el Idle provisional (`walk_01`) |
| `jump_01`…`jump_0N` (2–4) | Igual | Pies | Despegue/ascenso |
| `fall_01`…`fall_0N` (1–2) | Igual | Pies | Descenso |
| `land_01`…`land_0N` (2–3) | Igual | Pies | Aterrizaje |

Hasta entonces: **no** inventar Jump/Fall/Landing con estáticos.

## 4) Estado real por animación

| Estado | Estado real |
|--------|-------------|
| Idle | Provisional: hold `walk_01` (pixel). **No** es el PNG aprobado. |
| Walk | 8 frames del zip (pixel, baja def., artefactos de origen). Técnicamente cableado; **artísticamente rechazable**. |
| Jump | **Ausente** (sin frames). En aire no se fuerza el PNG painterly. |
| Fall | **Ausente**. |
| Landing | **Ausente**. |

## 5) Checklist Unity

1. Reimportar `Walk/walk_0*.png` (Full Rect, Point, PPU 48).
2. `ForestOrganicDemo` → `Visual`: sprite `walk_01`, scale 0.76, Animator `Player`, root motion off.
3. Idle quieto: se ve `walk_01` (pixel), **no** el perfil painterly.
4. Walk: ciclo 8; sin cambio a PNG aprobado al parar/saltar.
5. Confirmar cápsula/GroundCheck/física iguales.
6. Comparar Walk vs `ProtagonistApproved` — debe seguir viéndose el gap artístico hasta regenerar frames.

## Play Mode en este entorno

No re-ejecutado (Unity ausente). Correcciones basadas en inspección de assets + causa confirmada del Idle painterly en aire.
