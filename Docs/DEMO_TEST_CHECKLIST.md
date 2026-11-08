# Días Prestados — Checklist de pruebas

> **Estado de la auditoría 2026-11-08:** pruebas no ejecutadas en esta tarea documental; sin sesión de Unity accesible. Los resultados anteriores no se certifican para los archivos actuales. Fuente de estado: [ProjectStatus](ProjectStatus.md). Usar este listado en una revisión autorizada; no iniciar otro hito al completarlo automáticamente.

## Hito 1 (manual)

Abrir: **Tools → Dias Prestados → Demo → Abrir DiasPrestadosDemo** y pulsar Play.
Controles: A/D o ←/→, ↑ o Espacio para saltar, E para interactuar.

- [ ] La escena abre sin Missing Scripts ni errores en consola.
- [ ] Se reconocen ambos hermanos (mayor con bufanda roja; pequeño con gorro de pompón y chaqueta mostaza).
- [ ] Movimiento y salto fluidos; la cámara sigue con suavidad.
- [ ] Parallax, niebla, haces de luz y motas visibles; decoración sin colisión.
- [ ] El pequeño te sigue, se detiene a distancia razonable y salta el escalón y el tronco.
- [ ] Cruzas el barranco saltando; el pequeño lo cruza contigo.
- [ ] Ante el risco el pequeño pide ayuda y aparece el icono "E".
- [ ] Al pulsar E el pequeño sube al risco y celebra.
- [ ] Si caes al barranco reapareces en un punto seguro.
- [ ] Al llegar al final (x≈190) aparece el mensaje de fin de prueba.
- [ ] Duración total entre 45 y 90 s.

## Hito 1 (automática)

- [ ] Reejecutar, cuando corresponda, **Autotest Hito 1 (Play Mode)** y registrar fecha, versión y resultado. Existe un informe histórico "TODO CORRECTO", no revalidado aquí; no se leyó su archivo temporal en esta auditoría.

## Calidad visual de personajes (manual, pendiente)

- [ ] Mayor: curva posterior juvenil y natural en todos los Walk; sin pared vertical, casco, recorte hundido ni rojo incorrecto en nuca. Conservar la bufanda real.
- [ ] Comparar varios ciclos Idle de ambos personajes en Unity: ritmo y sutileza equivalentes, torso/hombros del mayor localizados, pies/piernas y posición general estables; sin rebotes.
- [ ] Probar Idle → Walk → Idle: respiración detenida al caminar, sin cambios de escala ni saltos de pose; salto/aterrizaje sin interferencias.
- [ ] Coherencia de silueta, proporciones, alineación y encuadre entre frames; registrar aprobación visual por separado de compilación/autotest.
- [ ] Revisar sin modificar al hermano ni retocar sprites protegidos salvo petición específica.

## Hitos 2–10

Se añadirán los criterios concretos de cada hito al iniciarlo, partiendo de los criterios de aceptación de `DEMO_MASTER_PLAN.md`.
