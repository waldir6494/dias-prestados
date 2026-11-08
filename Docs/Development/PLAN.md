# Plan — Animaciones del protagonista

> **PLAN HISTÓRICO SUSTITUIDO — 2026-11-08.** No ejecutar la regeneración descrita debajo. La instrucción vigente protege Walk/pelo y las animaciones que funcionan; cualquier cambio requiere petición específica. Idle ya tiene dos PNG propios, no un hold fijo de walk_01. Ver [WorldArt](../WorldArt.md) y [ProjectStatus](../ProjectStatus.md). Se conserva el diagnóstico anterior como historial, no como autorización ni estado visual actual.

## Diagnóstico (post Play Mode)

Los 8 Walk del zip son la fuente de cortes/baja definición/estilo distinto al PNG aprobado. Extracción OK (byte-idénticos). Tight mesh + Idle painterly empeoraban el resultado en juego.

## Correcciones técnicas (hecho)

Full Rect + Point; Idle=`walk_01`; escala única; `Grounded`; sin root motion / sin escalas por estado.

## Bloqueo artístico

Regenerar `walk_01..08` (+ idle/jump/fall/land) al estilo `ProtagonistApproved`. Detalle de entrega en `PROGRESS.md` §3.

## No hacer

Marioneta; HD falso; escalas distintas por estado; estados Jump/Fall/Landing inventados; tocar física/cámara; regenerar escena.
