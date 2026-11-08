# Hermano pequeño — referencias de rediseño (Hito 1)

Estas PNG son **referencias visuales** generadas para fijar la dirección artística, no sprite sheets listos para importar o recortar automáticamente.

## Prioridad

1. `00_DISENO_PRINCIPAL_Y_ESCALA.png`: dirección de diseño, proporciones y escala junto al hermano mayor. **Referencia principal**.
2. `01_EXPRESIONES_Y_MOVIMIENTOS.png`: ayuda para expresiones y gestos. **Referencia secundaria**.
3. `02_SECUENCIAS_ANIMACION.png`: referencia de acciones, pero puede variar el patrón del gorro.
4. `03_VISTAS_ADICIONALES.png`: referencia auxiliar de ropa/vistas, no prioritaria si difiere en colores.

## Reglas para Cursor

- Respeta el pixel art, sombreado y escala del protagonista **ya implementado**. Comparar directamente con el sprite real en Assets, no con personajes inventados de otras referencias.
- Usa siempre el diseño principal para evitar que cada animación muestre un niño distinto.
- El hermano menor debe medir menos y verse de 8 años, pero no caricaturizado con ojos saltones.
- Mantén intactos los sistemas existentes de seguimiento, interacción y físicas.
- No pegar imágenes completas al escenario ni cortar frames de referencias no uniformes suponiendo que son sprites listos para animar. Crear sprites coherentes adaptados a la resolución y pipeline actuales de Unity.
- Solicita revisión antes de ampliar el alcance a otro hito.
