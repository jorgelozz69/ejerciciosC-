# E01 · Calculadora con el editor visual

Aplicación de calculadora de cuatro operaciones desarrollada en WPF con .NET 10 y C#, con diseño responsivo basado en `Grid` y maquetación limpia en XAML.

## El código que generó el diseñador

Al arrastrar un botón desde el Cuadro de herramientas y posicionarlo con el ratón sobre el lienzo, el diseñador visual de Visual Studio genera el siguiente código XAML:

```xaml
<Button Content="Button" HorizontalAlignment="Left" Margin="24,115,0,0" VerticalAlignment="Top" Width="76" Height="46"/>
```

### Análisis del código generado por el diseñador:
- **Márgenes calculados (`Margin="24,115,0,0"`)**: El diseñador emula una posición absoluta fijando los márgenes izquierdo y superior. Al cambiar el tamaño de la ventana, los controles no se reubican ni se adaptan, quedando fijos o desbordados.
- **Alineaciones fijas (`HorizontalAlignment="Left"`, `VerticalAlignment="Top"`)**: Forzar las alineaciones superior e izquierda impide que el control aproveche el espacio fluido que ofrece WPF.
- **Dimensiones fijas (`Width="76"`, `Height="46"`)**: Fijar el ancho y alto en cada botón impide que estos crezcan o encojan armónicamente con la ventana.
- **Ausencia de contenedor flexible**: Al no definir filas ni columnas en el `Grid`, cada control se posiciona independientemente mediante coordenadas visuales, rompiendo la adaptabilidad del diseño.

## Capturas

*(Pendiente de completar tras maquetación y pruebas)*

## Distribución

*(Pendiente de completar)*

## Pruebas realizadas

*(Pendiente de completar)*
