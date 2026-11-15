using System.Windows;
using System.Windows.Navigation;

namespace Tarjeta;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnContactar_Click(object sender, RoutedEventArgs e) { }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) { }
}
