using System.Windows;

namespace VisorImagenes.Ventanas;

/// <summary>
/// Ventana modal de información de la aplicación.
/// Se cierra automáticamente con el botón Aceptar, Intro o Esc sin necesidad de código adicional
/// gracias a las propiedades IsDefault="True" e IsCancel="True" en su botón.
/// </summary>
public partial class VentanaAcercaDe : Window
{
    public VentanaAcercaDe()
    {
        InitializeComponent();
    }
}
