using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tarjeta;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string CorreoContacto = "marta.rios@example.org";
    private const string UrlWeb = "https://example.org/martarios";

    public MainWindow()
    {
        InitializeComponent();

        // Construcción dinámica de la interfaz desde C# (temas 3 y 5)
        CrearEtiquetas();
        pnlBotones.Children.Add(CrearBotonCompartir());
    }

    /// <summary>
    /// Genera dinámicamente etiquetas de habilidades en pnlHabilidades.
    /// Utiliza FindResource para obtener el recurso ColorTarjeta definido en Window.Resources.
    /// </summary>
    private void CrearEtiquetas()
    {
        var brushFondo = (Brush)FindResource("ColorTarjeta");
        string[] habilidades = ["XAML", "C#", "Figma", "Accesibilidad", "MVVM"];

        foreach (var habilidad in habilidades)
        {
            var borde = new Border
            {
                Background = brushFondo,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 4, 10, 4),
                Margin = new Thickness(0, 0, 8, 8)
            };

            var texto = new TextBlock
            {
                Text = habilidad,
                Foreground = Brushes.White,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold
            };

            borde.Child = texto;
            pnlHabilidades.Children.Add(borde);
        }
    }

    /// <summary>
    /// Construye en C# un botón con contenido compuesto idéntico a btnContactar:
    /// estilo BotonTarjeta, StackPanel horizontal con icono 🔗, texto 'Compartir' en negrita
    /// y texto descriptivo en gris. Asocia con += el manejador btnCompartir_Click.
    /// </summary>
    /// <returns>El control Button creado y configurado.</returns>
    private Button CrearBotonCompartir()
    {
        var boton = new Button
        {
            Style = (Style)FindResource("BotonTarjeta")
        };

        var panelContenido = new StackPanel
        {
            Orientation = Orientation.Horizontal
        };

        var icono = new TextBlock
        {
            Text = "🔗",
            Foreground = (Brush)FindResource("ColorTarjeta"),
            Margin = new Thickness(0, 0, 6, 0)
        };

        var textoTitulo = new TextBlock
        {
            Text = "Compartir",
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 6, 0)
        };

        var textoSubtitulo = new TextBlock
        {
            Text = "· copia dirección web",
            Foreground = Brushes.Gray
        };

        panelContenido.Children.Add(icono);
        panelContenido.Children.Add(textoTitulo);
        panelContenido.Children.Add(textoSubtitulo);

        boton.Content = panelContenido;

        // Asociación del manejador mediante el operador += en C#
        boton.Click += btnCompartir_Click;

        return boton;
    }

    /// <summary>
    /// Manejador del botón Contactar definido en XAML.
    /// Muestra un MessageBox con el correo de contacto.
    /// </summary>
    private void btnContactar_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            $"Correo de contacto:\n{CorreoContacto}",
            "Contactar con Marta Ríos",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Manejador del botón Compartir creado en C#.
    /// Copia la dirección web al portapapeles y notifica con un MessageBox.
    /// </summary>
    private void btnCompartir_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Clipboard.SetText(UrlWeb);
            MessageBox.Show(
                $"Dirección web copiada al portapapeles con éxito:\n{UrlWeb}",
                "Enlace copiado",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo copiar al portapapeles: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// Abre el navegador predeterminado al pulsar el hipervínculo en el TextBlock.
    /// </summary>
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo abrir el navegador: {ex.Message}",
                "Error al abrir enlace",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        e.Handled = true;
    }

    /// <summary>
    /// Desafío opcional: Construcción completa del avatar en C# con LinearGradientBrush.
    /// Permite comparar la sintaxis imperativa de C# con la declarativa de XAML.
    /// </summary>
    public static Grid CrearAvatarEnCodigo()
    {
        var grid = new Grid
        {
            Width = 96,
            Height = 96,
            Margin = new Thickness(0, 0, 16, 12),
            VerticalAlignment = VerticalAlignment.Top
        };

        var gradiente = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };
        gradiente.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#42A5F5"), 0.0));
        gradiente.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1565C0"), 1.0));

        var elipse = new Ellipse
        {
            Fill = gradiente
        };

        var texto = new TextBlock
        {
            Text = "MR",
            Foreground = Brushes.White,
            FontSize = 34,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        grid.Children.Add(elipse);
        grid.Children.Add(texto);

        return grid;
    }
}
