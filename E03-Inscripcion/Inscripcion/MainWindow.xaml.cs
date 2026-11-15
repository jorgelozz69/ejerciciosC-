using System;
using System.Windows;
using System.Windows.Controls;

namespace Inscripcion;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        dtpNacimiento.DisplayDateEnd = DateTime.Today;
    }

    private void chkTodas_Click(object sender, RoutedEventArgs e) { }
    private void chkExtra_Click(object sender, RoutedEventArgs e) { }
    private void txtObservaciones_TextChanged(object sender, TextChangedEventArgs e) { }
    private void txtObservaciones_SelectionChanged(object sender, RoutedEventArgs e) { }
    private void btnInscribir_Click(object sender, RoutedEventArgs e) { }
    private void btnLimpiar_Click(object sender, RoutedEventArgs e) { }
}
