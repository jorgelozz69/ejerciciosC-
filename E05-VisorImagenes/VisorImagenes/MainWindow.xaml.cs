using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VisorImagenes.Servicios;
using VisorImagenes.Ventanas;

namespace VisorImagenes;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<string> _rutasImagenes = [];
    private int _indiceActual = -1;
    private BitmapImage? _imagenActual = null;
    private bool _imagenCargadaCorrectamente = false;

    private bool _ajustadaAVentana = true;
    private double _factorZoom = 1.0;

    private const double ZoomMinimo = 0.10;
    private const double ZoomMaximo = 8.00;
    private const double FactorMultiplicador = 1.25;

    private string _ultimaCarpeta = string.Empty;
    private GridLength _anchoListaGuardado = new(220);

    public MainWindow()
    {
        InitializeComponent();
        CargarConfiguracionInicial();
        ActualizarEstadoControles();
    }

    /// <summary>
    /// Restaura la configuración persistida del usuario (posición, tamaño, tema, visor, etc.).
    /// </summary>
    private void CargarConfiguracionInicial()
    {
        ConfiguracionVisor config = ServicioConfiguracion.Cargar();

        if (config.Width >= MinWidth)
        {
            Width = config.Width;
        }

        if (config.Height >= MinHeight)
        {
            Height = config.Height;
        }

        if (config.Left >= 0 && config.Top >= 0)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = config.Left;
            Top = config.Top;
        }

        if (config.Maximizada)
        {
            WindowState = WindowState.Maximized;
        }

        if (config.AnchoLista >= 100)
        {
            _anchoListaGuardado = new GridLength(config.AnchoLista);
            colLista.Width = _anchoListaGuardado;
        }

        AplicarVisibilidadLista(config.MostrarLista);
        AplicarTema(config.Tema);

        _ultimaCarpeta = config.UltimaCarpeta;

        // Desafío opcional: cargar última imagen si sigue existiendo
        if (!string.IsNullOrEmpty(config.UltimaImagen) && File.Exists(config.UltimaImagen))
        {
            string? dir = Path.GetDirectoryName(config.UltimaImagen);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                // Cargar todas las imágenes de la misma carpeta manteniendo la posición
                var archivos = Directory.GetFiles(dir, "*.*");
                var permitidos = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".png", ".jpg", ".jpeg", ".bmp", ".gif"
                };

                var lista = new List<string>();
                foreach (var f in archivos)
                {
                    if (permitidos.Contains(Path.GetExtension(f)))
                    {
                        lista.Add(f);
                    }
                }

                if (lista.Count > 0)
                {
                    CargarImagenes(lista);
                    int pos = lista.IndexOf(config.UltimaImagen);
                    if (pos >= 0)
                    {
                        lstImagenes.SelectedIndex = pos;
                    }

                    if (config.UltimoZoomAjustado == false && config.UltimoZoom.HasValue)
                    {
                        _ajustadaAVentana = false;
                        _factorZoom = Math.Clamp(config.UltimoZoom.Value, ZoomMinimo, ZoomMaximo);
                        AplicarZoom();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Comando predefinido ApplicationCommands.Open (Ctrl+O). Abre el diálogo de selección de archivos.
    /// </summary>
    private void Abrir_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Imágenes (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Todos los archivos (*.*)|*.*",
            Multiselect = true,
            Title = "Seleccionar imágenes"
        };

        if (!string.IsNullOrEmpty(_ultimaCarpeta) && Directory.Exists(_ultimaCarpeta))
        {
            dlg.InitialDirectory = _ultimaCarpeta;
        }
        else
        {
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        if (dlg.ShowDialog(this) == true && dlg.FileNames.Length > 0)
        {
            _ultimaCarpeta = Path.GetDirectoryName(dlg.FileNames[0]) ?? string.Empty;
            CargarImagenes(dlg.FileNames);
        }
    }

    /// <summary>
    /// Reemplaza la lista actual de imágenes por las seleccionadas y muestra la primera.
    /// </summary>
    private void CargarImagenes(IEnumerable<string> rutas)
    {
        _rutasImagenes.Clear();
        _rutasImagenes.AddRange(rutas);

        lstImagenes.Items.Clear();
        foreach (var ruta in _rutasImagenes)
        {
            lstImagenes.Items.Add(Path.GetFileName(ruta));
        }

        if (_rutasImagenes.Count > 0)
        {
            lstImagenes.SelectedIndex = 0;
        }
        else
        {
            CerrarTodas();
        }
    }

    /// <summary>
    /// Manejador de cambio de selección en la lista de imágenes.
    /// </summary>
    private void lstImagenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lstImagenes.SelectedIndex >= 0 && lstImagenes.SelectedIndex < _rutasImagenes.Count)
        {
            MostrarImagen(lstImagenes.SelectedIndex);
        }
    }

    /// <summary>
    /// Carga el BitmapImage con CacheOption.OnLoad y gestiona excepciones si la imagen está corrupta.
    /// </summary>
    private void MostrarImagen(int indice)
    {
        _indiceActual = indice;
        string ruta = _rutasImagenes[indice];
        string nombre = Path.GetFileName(ruta);

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(ruta, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            _imagenActual = bitmap;
            _imagenCargadaCorrectamente = true;

            imgVista.Source = bitmap;
            imgVista.Visibility = Visibility.Visible;
            txtMensaje.Visibility = Visibility.Collapsed;

            Title = $"{nombre} · Visor de imágenes";
            txtEstadoNombre.Text = $"{nombre} ({indice + 1} de {_rutasImagenes.Count})";
            txtEstadoTamano.Text = $"{bitmap.PixelWidth} × {bitmap.PixelHeight} px";

            _ajustadaAVentana = true;
            _factorZoom = 1.0;
            AplicarZoom();
        }
        catch (Exception ex) when (ex is NotSupportedException or FileFormatException or IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            _imagenActual = null;
            _imagenCargadaCorrectamente = false;

            imgVista.Source = null;
            imgVista.Visibility = Visibility.Collapsed;
            txtMensaje.Text = $"No se puede abrir \"{nombre}\":\n{ex.Message}";
            txtMensaje.Visibility = Visibility.Visible;

            Title = $"{nombre} · Visor de imágenes";
            txtEstadoNombre.Text = $"{nombre} ({indice + 1} de {_rutasImagenes.Count})";
            txtEstadoTamano.Text = "Formato no válido";
            txtEstadoZoom.Text = string.Empty;
        }
        finally
        {
            ActualizarEstadoControles();
        }
    }

    /// <summary>
    /// Limpia todas las imágenes cargadas y devuelve la interfaz a su estado inicial neutro.
    /// </summary>
    private void CerrarTodas()
    {
        _rutasImagenes.Clear();
        lstImagenes.Items.Clear();
        _indiceActual = -1;
        _imagenActual = null;
        _imagenCargadaCorrectamente = false;

        imgVista.Source = null;
        imgVista.Visibility = Visibility.Collapsed;
        txtMensaje.Text = "No hay ninguna imagen abierta.\nUsa el menú Archivo > Abrir imágenes... o pulsa Ctrl+O.";
        txtMensaje.Visibility = Visibility.Visible;

        Title = "Visor de imágenes";
        txtEstadoNombre.Text = "Sin imágenes";
        txtEstadoTamano.Text = string.Empty;
        txtEstadoZoom.Text = string.Empty;

        ActualizarEstadoControles();
    }

    private void CerrarTodas_Click(object sender, RoutedEventArgs e)
    {
        CerrarTodas();
    }

    private void Salir_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Anterior_Click(object sender, RoutedEventArgs e)
    {
        if (_indiceActual > 0)
        {
            lstImagenes.SelectedIndex = _indiceActual - 1;
        }
    }

    private void Siguiente_Click(object sender, RoutedEventArgs e)
    {
        if (_indiceActual < _rutasImagenes.Count - 1)
        {
            lstImagenes.SelectedIndex = _indiceActual + 1;
        }
    }

    private void Acercar_Click(object sender, RoutedEventArgs e)
    {
        if (!_imagenCargadaCorrectamente || _imagenActual == null) return;

        if (_ajustadaAVentana)
        {
            _factorZoom = ObtenerFactorActualAjustado();
            _ajustadaAVentana = false;
        }

        _factorZoom = Math.Min(ZoomMaximo, _factorZoom * FactorMultiplicador);
        AplicarZoom();
    }

    private void Alejar_Click(object sender, RoutedEventArgs e)
    {
        if (!_imagenCargadaCorrectamente || _imagenActual == null) return;

        if (_ajustadaAVentana)
        {
            _factorZoom = ObtenerFactorActualAjustado();
            _ajustadaAVentana = false;
        }

        _factorZoom = Math.Max(ZoomMinimo, _factorZoom / FactorMultiplicador);
        AplicarZoom();
    }

    private void TamanoReal_Click(object sender, RoutedEventArgs e)
    {
        if (!_imagenCargadaCorrectamente || _imagenActual == null) return;

        _ajustadaAVentana = false;
        _factorZoom = 1.0;
        AplicarZoom();
    }

    private void Ajustar_Click(object sender, RoutedEventArgs e)
    {
        if (!_imagenCargadaCorrectamente || _imagenActual == null) return;

        _ajustadaAVentana = true;
        AplicarZoom();
    }

    /// <summary>
    /// Calcula el factor de escala proporcional actual de la imagen cuando está ajustada a la ventana.
    /// </summary>
    private double ObtenerFactorActualAjustado()
    {
        if (_imagenActual == null || scvImagen.ActualWidth <= 0 || scvImagen.ActualHeight <= 0)
        {
            return 1.0;
        }

        double escalaX = scvImagen.ActualWidth / _imagenActual.PixelWidth;
        double escalaY = scvImagen.ActualHeight / _imagenActual.PixelHeight;
        return Math.Clamp(Math.Min(escalaX, escalaY), ZoomMinimo, ZoomMaximo);
    }

    /// <summary>
    /// Aplica las dimensiones y reglas de desplazamiento según el modo de zoom activo.
    /// </summary>
    private void AplicarZoom()
    {
        if (!_imagenCargadaCorrectamente || _imagenActual == null)
        {
            txtEstadoZoom.Text = string.Empty;
            return;
        }

        if (_ajustadaAVentana)
        {
            scvImagen.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scvImagen.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            imgVista.Stretch = Stretch.Uniform;
            imgVista.Width = double.NaN;
            imgVista.Height = double.NaN;
            txtEstadoZoom.Text = "Ajustada a la ventana";
        }
        else
        {
            scvImagen.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scvImagen.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            imgVista.Stretch = Stretch.Uniform;
            imgVista.Width = _imagenActual.PixelWidth * _factorZoom;
            imgVista.Height = _imagenActual.PixelHeight * _factorZoom;
            txtEstadoZoom.Text = $"{Math.Round(_factorZoom * 100):0} %";
        }
    }

    /// <summary>
    /// Zoom con Ctrl + rueda de ratón o desplazamiento normal con la rueda sola.
    /// </summary>
    private void scvImagen_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (e.Delta > 0)
            {
                Acercar_Click(sender, e);
            }
            else if (e.Delta < 0)
            {
                Alejar_Click(sender, e);
            }
            e.Handled = true;
        }
    }

    /// <summary>
    /// Captura atajos de teclado a nivel de ventana para zoom y navegación.
    /// </summary>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // No interceptar si el usuario está navegando por un menú abierto
        if (MenuEstaAbierto())
        {
            return;
        }

        bool ctrl = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (ctrl)
        {
            switch (e.Key)
            {
                case Key.OemPlus or Key.Add:
                    if (btnAcercar.IsEnabled)
                    {
                        Acercar_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Key.OemMinus or Key.Subtract:
                    if (btnAlejar.IsEnabled)
                    {
                        Alejar_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Key.D0 or Key.NumPad0:
                    if (btnTamanoReal.IsEnabled)
                    {
                        TamanoReal_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Key.D1 or Key.NumPad1:
                    if (btnAjustar.IsEnabled)
                    {
                        Ajustar_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
            }
        }
        else if (Keyboard.Modifiers == ModifierKeys.None)
        {
            switch (e.Key)
            {
                case Key.Left:
                    if (btnAnterior.IsEnabled)
                    {
                        Anterior_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
                case Key.Right:
                    if (btnSiguiente.IsEnabled)
                    {
                        Siguiente_Click(sender, e);
                        e.Handled = true;
                    }
                    break;
            }
        }
    }

    private bool MenuEstaAbierto()
    {
        return mnuArchivo.IsSubmenuOpen || mnuVer.IsSubmenuOpen || mnuAyuda.IsSubmenuOpen;
    }

    private void MostrarLista_Click(object sender, RoutedEventArgs e)
    {
        AplicarVisibilidadLista(mnuMostrarLista.IsChecked);
    }

    private void AplicarVisibilidadLista(bool visible)
    {
        mnuMostrarLista.IsChecked = visible;
        if (visible)
        {
            colLista.Width = _anchoListaGuardado.Value >= 100 ? _anchoListaGuardado : new GridLength(220);
            colSeparador.Width = GridLength.Auto;
            lstImagenes.Visibility = Visibility.Visible;
            splLista.Visibility = Visibility.Visible;
        }
        else
        {
            if (colLista.ActualWidth >= 100)
            {
                _anchoListaGuardado = new GridLength(colLista.ActualWidth);
            }
            colLista.Width = new GridLength(0);
            colSeparador.Width = new GridLength(0);
            lstImagenes.Visibility = Visibility.Collapsed;
            splLista.Visibility = Visibility.Collapsed;
        }
    }

    private void mnuTema_Click(object sender, RoutedEventArgs e)
    {
        if (sender == mnuTemaClaro)
        {
            AplicarTema("Claro");
        }
        else if (sender == mnuTemaOscuro)
        {
            AplicarTema("Oscuro");
        }
        else if (sender == mnuTemaSistema)
        {
            AplicarTema("Del sistema");
        }
    }

    private void AplicarTema(string nombreTema)
    {
        mnuTemaClaro.IsChecked = (nombreTema == "Claro");
        mnuTemaOscuro.IsChecked = (nombreTema == "Oscuro");
        mnuTemaSistema.IsChecked = (nombreTema == "Del sistema");

        ThemeMode modo = nombreTema switch
        {
            "Claro" => ThemeMode.Light,
            "Oscuro" => ThemeMode.Dark,
            _ => ThemeMode.System
        };

        Application.Current.ThemeMode = modo;

        // Actualizar el recurso dinámico ColorMensajeCentral para que siempre contraste con el fondo
        if (modo == ThemeMode.Dark)
        {
            Application.Current.Resources["ColorMensajeCentral"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        }
        else if (modo == ThemeMode.Light)
        {
            Application.Current.Resources["ColorMensajeCentral"] = new SolidColorBrush(Color.FromRgb(70, 70, 70));
        }
        else
        {
            Application.Current.Resources["ColorMensajeCentral"] = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }
    }

    private void AcercaDe_Click(object sender, RoutedEventArgs e)
    {
        var acercaDe = new VentanaAcercaDe
        {
            Owner = this
        };
        acercaDe.ShowDialog();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        var config = new ConfiguracionVisor
        {
            Maximizada = WindowState == WindowState.Maximized,
            Tema = mnuTemaOscuro.IsChecked ? "Oscuro" : (mnuTemaClaro.IsChecked ? "Claro" : "Del sistema"),
            MostrarLista = mnuMostrarLista.IsChecked,
            UltimaCarpeta = _ultimaCarpeta,
            AnchoLista = colLista.ActualWidth >= 100 ? colLista.ActualWidth : _anchoListaGuardado.Value,
            UltimaImagen = (_indiceActual >= 0 && _indiceActual < _rutasImagenes.Count) ? _rutasImagenes[_indiceActual] : null,
            UltimoZoom = _factorZoom,
            UltimoZoomAjustado = _ajustadaAVentana
        };

        if (WindowState == WindowState.Maximized)
        {
            config.Left = RestoreBounds.Left;
            config.Top = RestoreBounds.Top;
            config.Width = RestoreBounds.Width;
            config.Height = RestoreBounds.Height;
        }
        else
        {
            config.Left = Left;
            config.Top = Top;
            config.Width = Width;
            config.Height = Height;
        }

        ServicioConfiguracion.Guardar(config);
    }

    /// <summary>
    /// Deshabilita o habilita menús y botones según el estado actual de las imágenes y la selección.
    /// </summary>
    private void ActualizarEstadoControles()
    {
        bool hayImagenes = _rutasImagenes.Count > 0;
        bool puedeAnterior = hayImagenes && _indiceActual > 0;
        bool puedeSiguiente = hayImagenes && _indiceActual >= 0 && _indiceActual < _rutasImagenes.Count - 1;
        bool puedeZoom = _imagenCargadaCorrectamente && _imagenActual != null;

        mnuCerrarTodas.IsEnabled = hayImagenes;

        mnuAnterior.IsEnabled = btnAnterior.IsEnabled = puedeAnterior;
        mnuSiguiente.IsEnabled = btnSiguiente.IsEnabled = puedeSiguiente;

        mnuAcercar.IsEnabled = btnAcercar.IsEnabled = puedeZoom;
        mnuAlejar.IsEnabled = btnAlejar.IsEnabled = puedeZoom;
        mnuTamanoReal.IsEnabled = btnTamanoReal.IsEnabled = puedeZoom;
        mnuAjustar.IsEnabled = btnAjustar.IsEnabled = puedeZoom;
    }
}
