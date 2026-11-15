using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculadora;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnDigito_Click(object sender, RoutedEventArgs e) { }
    private void btnOperacion_Click(object sender, RoutedEventArgs e) { }
    private void btnComa_Click(object sender, RoutedEventArgs e) { }
    private void btnIgual_Click(object sender, RoutedEventArgs e) { }
    private void btnBorrar_Click(object sender, RoutedEventArgs e) { }
    private void btnRetroceso_Click(object sender, RoutedEventArgs e) { }
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { }
}
