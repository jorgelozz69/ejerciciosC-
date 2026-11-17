using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Autoevaluacion;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<Pregunta> _preguntas = BancoPreguntas.Crear();
    private RadioButton[] _opciones = null!;

    private int _indicePregunta = 0;
    private int _puntos = 0;
    private int _intentoActual = 1;
    private bool _preguntaResuelta = false;
    private int _preguntasRespondidas = 0;
    private bool _testTerminado = false;
    private bool _resultadoGuardado = false;

    public MainWindow()
    {
        InitializeComponent();
        _opciones = [rbOpcionA, rbOpcionB, rbOpcionC, rbOpcionD];
        MostrarPregunta(0);
    }

    /// <summary>
    /// Carga y muestra los datos de la pregunta indicada en la interfaz.
    /// </summary>
    private void MostrarPregunta(int indice)
    {
        _indicePregunta = indice;
        _intentoActual = 1;
        _preguntaResuelta = false;

        Pregunta p = _preguntas[indice];
        txtProgreso.Text = $"Pregunta {indice + 1} de {_preguntas.Count}";
        txtPuntos.Text = $"Puntos: {_puntos}";
        txtEnunciado.Text = p.Enunciado;

        char[] letras = ['A', 'B', 'C', 'D'];
        for (int i = 0; i < _opciones.Length; i++)
        {
            _opciones[i].Content = $"{letras[i]}) {p.Opciones[i]}";
            _opciones[i].IsChecked = false;
            _opciones[i].IsEnabled = true;
            _opciones[i].Opacity = 1.0;
        }

        txtAyuda.Text = string.Empty;
        txtResultado.Text = string.Empty;

        btnCorregir.IsEnabled = false;
        btnSiguiente.IsEnabled = false;
        btnSiguiente.Content = (indice == _preguntas.Count - 1) ? "_Ver resultado" : "_Siguiente";
    }

    /// <summary>
    /// Manejador burbujeado en el panel para activar el botón Corregir al marcar una opción.
    /// </summary>
    private void pnlOpciones_Checked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is RadioButton && !_preguntaResuelta)
        {
            btnCorregir.IsEnabled = true;
        }
    }

    /// <summary>
    /// Obtiene la letra asociada al RadioButton de la opción (A, B, C o D).
    /// </summary>
    private string ObtenerLetraOpcion(RadioButton rb)
    {
        if (rb.Tag is string s && !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }
        if (rb.Name.StartsWith("rbOpcion") && rb.Name.Length > 8)
        {
            return rb.Name[8..];
        }
        return string.Empty;
    }

    /// <summary>
    /// Manejador directo de clic derecho para descartar o recuperar una opción (atenúa la opacidad).
    /// </summary>
    private void rbOpcion_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is RadioButton rb && rb.IsEnabled)
        {
            string letra = ObtenerLetraOpcion(rb);
            if (rb.Opacity < 0.9)
            {
                rb.Opacity = 1.0;
                txtAyuda.Text = $"Clic derecho para descartar la opción {letra}";
            }
            else
            {
                rb.Opacity = 0.4;
                txtAyuda.Text = $"Clic derecho para recuperar la opción {letra}";
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// Manejador directo de entrada del ratón para mostrar ayuda contextual según si la opción está descartada.
    /// </summary>
    private void rbOpcion_MouseEnter(object sender, MouseEventArgs e)
    {
        if (sender is RadioButton rb && rb.IsEnabled)
        {
            string letra = ObtenerLetraOpcion(rb);
            string accion = rb.Opacity < 0.9 ? "recuperar" : "descartar";
            txtAyuda.Text = $"Clic derecho para {accion} la opción {letra}";
        }
    }

    /// <summary>
    /// Manejador directo de salida del ratón para limpiar el texto de ayuda.
    /// </summary>
    private void rbOpcion_MouseLeave(object sender, MouseEventArgs e)
    {
        txtAyuda.Text = string.Empty;
    }

    /// <summary>
    /// Manejador de evento túnel para capturar teclas (A-D, 1-4, teclado numérico) y marcar opciones.
    /// </summary>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Con Ctrl o Alt pulsadas no se hace nada, para no romper las teclas de acceso
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
        {
            return;
        }

        if (_preguntaResuelta)
        {
            return;
        }

        RadioButton? opcion = e.Key switch
        {
            Key.A or Key.D1 or Key.NumPad1 => rbOpcionA,
            Key.B or Key.D2 or Key.NumPad2 => rbOpcionB,
            Key.C or Key.D3 or Key.NumPad3 => rbOpcionC,
            Key.D or Key.D4 or Key.NumPad4 => rbOpcionD,
            _ => null
        };

        if (opcion != null)
        {
            e.Handled = true;
            if (opcion.IsEnabled)
            {
                opcion.IsChecked = true;
                opcion.Focus();
            }
        }
    }

    /// <summary>
    /// Corrige la opción marcada según el número de intento y actualiza puntuación y controles.
    /// </summary>
    private void btnCorregir_Click(object sender, RoutedEventArgs e)
    {
        int indiceSeleccionado = -1;
        for (int i = 0; i < _opciones.Length; i++)
        {
            if (_opciones[i].IsChecked == true)
            {
                indiceSeleccionado = i;
                break;
            }
        }

        if (indiceSeleccionado == -1)
        {
            return;
        }

        Pregunta p = _preguntas[_indicePregunta];
        char[] letras = ['A', 'B', 'C', 'D'];

        if (indiceSeleccionado == p.Correcta)
        {
            int puntosSumados = _intentoActual == 1 ? 2 : 1;
            _puntos += puntosSumados;
            txtPuntos.Text = $"Puntos: {_puntos}";
            _preguntaResuelta = true;
            _preguntasRespondidas++;

            txtResultado.Text = $"✓ ¡Correcto! {p.Explicacion}";

            // Desafío opcional: icono visual en la opción correcta
            _opciones[p.Correcta].Content = $"{letras[p.Correcta]}) {p.Opciones[p.Correcta]}  ✅";

            FinalizarPregunta();
        }
        else
        {
            if (_intentoActual == 1)
            {
                _opciones[indiceSeleccionado].IsChecked = false;
                _opciones[indiceSeleccionado].IsEnabled = false;
                _opciones[indiceSeleccionado].Content = $"{letras[indiceSeleccionado]}) {p.Opciones[indiceSeleccionado]}  ❌";
                _intentoActual = 2;
                txtResultado.Text = "Te queda 1 intento";
                btnCorregir.IsEnabled = false;
            }
            else
            {
                _preguntaResuelta = true;
                _preguntasRespondidas++;

                // Desafío opcional: marcar opción fallada y opción correcta
                _opciones[indiceSeleccionado].Content = $"{letras[indiceSeleccionado]}) {p.Opciones[indiceSeleccionado]}  ❌";
                _opciones[p.Correcta].Content = $"{letras[p.Correcta]}) {p.Opciones[p.Correcta]}  ✅";

                char letraCorrecta = letras[p.Correcta];
                txtResultado.Text = $"Incorrecto. La respuesta correcta era la {letraCorrecta}) {p.Opciones[p.Correcta]}.\n{p.Explicacion}";

                FinalizarPregunta();
            }
        }
    }

    /// <summary>
    /// Finaliza la pregunta actual desactivando opciones y dando foco al botón Siguiente.
    /// </summary>
    private void FinalizarPregunta()
    {
        foreach (var rb in _opciones)
        {
            rb.IsEnabled = false;
        }
        btnCorregir.IsEnabled = false;
        btnSiguiente.IsEnabled = true;
        btnSiguiente.Focus();
    }

    /// <summary>
    /// Avanza a la siguiente pregunta o gestiona el final del test con repetición o guardado.
    /// </summary>
    private void btnSiguiente_Click(object sender, RoutedEventArgs e)
    {
        if (_indicePregunta < _preguntas.Count - 1)
        {
            MostrarPregunta(_indicePregunta + 1);
        }
        else
        {
            _testTerminado = true;
            int maxPuntos = _preguntas.Count * 2;
            MessageBoxResult respuesta = MessageBox.Show(
                $"Has obtenido {_puntos} de {maxPuntos} puntos.\n¿Quieres repetir el test?",
                "Test finalizado",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (respuesta == MessageBoxResult.Yes)
            {
                ReiniciarTest();
            }
            else
            {
                txtResultado.Text = $"Test finalizado: {_puntos} de {maxPuntos} puntos.";
                btnGuardar.IsEnabled = true;
                btnSiguiente.IsEnabled = false;
                btnCorregir.IsEnabled = false;
                btnGuardar.Focus();
            }
        }
    }

    /// <summary>
    /// Reinicia todos los estados del test para volver a empezar desde la primera pregunta.
    /// </summary>
    private void ReiniciarTest()
    {
        _puntos = 0;
        _preguntasRespondidas = 0;
        _testTerminado = false;
        _resultadoGuardado = false;
        txtPuntos.Text = "Puntos: 0";
        btnGuardar.IsEnabled = false;
        MostrarPregunta(0);
    }

    /// <summary>
    /// Guarda el resultado de la sesión en el archivo autoevaluacion.txt en la carpeta Documentos.
    /// </summary>
    /// <returns>True si se guardó correctamente; false si ocurrió algún error de E/S o permisos.</returns>
    private bool GuardarResultado()
    {
        try
        {
            string carpetaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaFichero = Path.Combine(carpetaDocumentos, "autoevaluacion.txt");

            string estado = _testTerminado ? "terminado" : "a medias";
            int maxPuntos = _preguntas.Count * 2;
            string linea = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} · {_puntos} de {maxPuntos} puntos · {_preguntasRespondidas} de {_preguntas.Count} preguntas ({estado})";

            File.AppendAllLines(rutaFichero, [linea]);
            _resultadoGuardado = true;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                $"No se ha podido guardar el resultado en el archivo:\n{ex.Message}",
                "Error al guardar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }

    /// <summary>
    /// Manejador del botón Guardar resultado.
    /// </summary>
    private void btnGuardar_Click(object sender, RoutedEventArgs e)
    {
        if (GuardarResultado())
        {
            MessageBox.Show(
                "Resultado guardado correctamente en autoevaluacion.txt.",
                "Resultado guardado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            btnGuardar.IsEnabled = false;
        }
    }

    /// <summary>
    /// Manejador del cierre de ventana que comprueba si hay resultados pendientes de guardar.
    /// </summary>
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_preguntasRespondidas > 0 && !_resultadoGuardado)
        {
            MessageBoxResult respuesta = MessageBox.Show(
                "Hay preguntas respondidas y el resultado no se ha guardado.\n¿Quieres guardar el resultado antes de salir?",
                "Guardar resultado",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Cancel);

            switch (respuesta)
            {
                case MessageBoxResult.Yes:
                    bool guardado = GuardarResultado();
                    if (!guardado)
                    {
                        e.Cancel = true; // Si guardar falla, no cierra la ventana
                    }
                    break;
                case MessageBoxResult.No:
                    // Cierra sin guardar
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true; // Cancelar no cierra
                    break;
            }
        }
    }
}
