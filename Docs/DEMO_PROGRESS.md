# Días Prestados — Progreso de la demo

| Hito | Nombre | Estado |
|---|---|---|
| 0 | Auditoría y plan | IMPLEMENTADO — PENDIENTE DE REVISIÓN |
| 1 | Primera prueba jugable | IMPLEMENTADO — PENDIENTE DE REVISIÓN |
| 2 | Bosque y relación entre hermanos | PENDIENTE |
| 3 | Separación temporal | PENDIENTE |
| 4 | Descubrimiento del artefacto | PENDIENTE |
| 5 | Puzles temporales | PENDIENTE |
| 6 | Puzle físico independiente | PENDIENTE |
| 7 | Peligro y persecución | PENDIENTE |
| 8 | Final emocional | PENDIENTE |
| 9 | Presentación y pulido | PENDIENTE |
| 10 | Validación final | PENDIENTE |

## Hito 1 — detalle de validación

- Implementado: escena `Assets/Scenes/DiasPrestadosDemo.unity`, personajes, terreno, parallax, compañero, interacción, HUD, autotest.
- Compilado: sí (Unity Editor; solo warnings ajenos o de obsolescencia).
- Validado automáticamente: Autotest en Play Mode (`Tools → Dias Prestados → Demo → Autotest Hito 1 (Play Mode)`), resultado "TODO CORRECTO": avance, salto (~2,7 u), recorrido completo hasta x≈189, el pequeño pide ayuda, E lo impulsa al risco y sigue cerca al final. Informe en `Temp/DemoAutotest/report.txt` (no versionado).
- Pendiente de prueba manual: sensación de control con teclado real, lectura visual en tu monitor, ritmo, y comparación con las referencias.
- Sin commit realizado.

## Correcciones Hito 1 (revisi�n)
- Roca: escala reducida, orden de dibujo -3 y sin collider (decoraci�n); el recorrido no tiene otros obst�culos solapados.
- Protagonista: animaci�n de caminar sincronizada con el desplazamiento y el input (toques cortos mueven las piernas).
- Hermano peque�o: sprite redise�ado (gorro, bufanda, mochila, cara expresiva) con animaciones idle/walk/cheer m�s naturales.
- Minipuzle: caja empujable (Crate_Pushable) para llegar al risco de x=100; reaparece si cae o se sale del tramo (sin softlock).
- Autotest: TODO CORRECTO (toques cortos, caja + ayuda, impulso, final). Pendiente: revisi�n del usuario.
