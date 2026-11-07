# Progress — Animaciones del protagonista

## Estado: BLOQUEADO (fotogramas)

Diseño visual del PNG **aprobado** — no se rediseña.

El problema pendiente es el **deslizamiento**: hace falta animación 2D **por fotogramas** (Idle/Walk/Run/Jump/Fall/Landing).  
**No** hay marioneta. **No** hay clips `.anim` / Animator Controller. **No** hay spritesheet de producción.

Rama/PR: `cursor/animaciones-protagonista-2f38` / [#1](https://github.com/davidmartirio/dias-prestados/pull/1)  
Base: `feature/prototipo-jugable` — **sin merge**.

## Inspección

| Recurso | Hallazgo |
|---------|----------|
| Visual en escena | `ProtagonistApproved.png` (recorte del perfil aprobado, pivot suelas, h≈1.72) |
| PNG original | `Assets/Art/Characters/Protagonist/DiasPrestados_Protagonista_Perfil_PNG.png` — **1 pose estática** |
| Frames de animación en `Assets/` | **Ninguno** (no `.anim`, no `.controller`, no carpeta de frames Walk/Idle/…) |
| `PlayerProtagonistAnimator` | Transform sobrio (lean/squash) sobre **un solo** sprite — **no** es animación por fotogramas; David lo identifica como sprite estático que se desliza |
| `Docs/.../DiasPrestados_Hoja_Animaciones.png` | Referencia con etiquetas/fondo; **no** spritesheet importable |
| `Docs/.../04_protagonista_12_anos.png` | Concepto con strips; poses/estilo no homologados al PNG aprobado |
| Partes `Parts/` / `Resources/ProtagonistParts/` | Marioneta rechazada; no usar |

### Intento de producir Walk 8

Se intentó generar un strip Walk 8 con el PNG aprobado como referencia. Resultado: **8 copias idénticas de una sola pose** (sin ciclo de piernas/brazos). **No cumple** “ciclo completo” ni “NO copias del mismo sprite”. Descartado; no se integró.

## Sprites que faltan (bloqueo exacto)

Lienzo uniforme, alpha limpio, mismo diseño/proporciones/ropa que `ProtagonistApproved`, pivote/pies alineados, piernas y brazos **visibles y distintos** entre frames:

1. **Walk — 8 frames** (prioridad #1; ciclo completo) — **FALTA / BLOQUEA el resto**
2. **Run — 8 frames** — pendiente (solo tras Walk válido)
3. **Idle — ~4 frames** — pendiente
4. **Jump — 2–4 frames** — pendiente
5. **Fall — 1–2 frames** — pendiente
6. **Landing — 2–3 frames** — pendiente

Sin el ítem 1 no se montan Animator Controller, clips ni sync de zancada.

## Qué hay en el repo ahora (no es la solución de fotogramas)

- Visual fiel al PNG aprobado + pivot/altura/giro `flipX`
- `PlayerProtagonistAnimator` = interim transform; **no** sustituye Walk por frames
- Física / collider / GroundCheck / cámara **sin cambios**

## Integración Animator

**No hecha** (no hay frames válidos). Orden obligatorio cuando existan assets:

1. Importar Walk 8 → clips + controller → validar stride vs `linearVelocity.x`
2. Luego Run / Idle / aéreos solo con recursos reales

## Play Mode

**No ejecutado**: Unity Editor ausente en este entorno.

## Checklist Unity (cuando existan frames Walk 8)

1. Importar frames con pivote en pies, PPU coherente, alpha OK  
2. Crear `Walk` clip (loop) + Animator Controller  
3. Cablear en `Visual` sin regenerar escena; leer `IsGrounded` / velocidad existentes  
4. Play: piernas/brazos se mueven; sin desliz de pies; giro solo visual  
5. No alterar Rigidbody2D / CapsuleCollider2D / GroundCheck / salto / gravedad / CameraFollow2D  

## Limitación artística (honesta)

Este entorno **no puede** pintar un ciclo Walk 8 consistente con el PNG aprobado a calidad indie. Hace falta entrega de arte (o pipeline artístico humano) con los frames listados arriba.
