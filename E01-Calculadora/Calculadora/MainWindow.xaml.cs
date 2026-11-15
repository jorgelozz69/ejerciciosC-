using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Calculadora;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-ES");
    private const string MensajeDivisionCero = "No se puede dividir entre cero";

    private decimal? _operando1;
    private string? _operacionPendiente;
    private bool _nuevoNumero = true;
    private bool _esResultado = false;
    private bool _esError = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnDigito_Click(object sender, RoutedEventArgs e)
    {
        string digito = (string)((Button)sender).Content;
        ProcesarDigito(digito);
    }

    private void ProcesarDigito(string digito)
    {
        txtPantalla.FontSize = 36;
        if (_esError)
        {
            ResetearEstado();
            txtPantalla.Text = digito;
            _nuevoNumero = false;
            return;
        }

        if (_esResultado)
        {
            txtOperacion.Text = string.Empty;
            txtPantalla.Text = digito;
            _esResultado = false;
            _nuevoNumero = false;
            return;
        }

        if (_nuevoNumero || txtPantalla.Text == "0")
        {
            txtPantalla.Text = digito;
            _nuevoNumero = false;
        }
        else
        {
            txtPantalla.Text += digito;
        }
    }

    private void btnComa_Click(object sender, RoutedEventArgs e)
    {
        ProcesarComa();
    }

    private void ProcesarComa()
    {
        txtPantalla.FontSize = 36;
        if (_esError || _esResultado)
        {
            if (_esError)
            {
                ResetearEstado();
            }
            else
            {
                txtOperacion.Text = string.Empty;
                _esResultado = false;
            }
            txtPantalla.Text = "0,";
            _nuevoNumero = false;
            return;
        }

        if (_nuevoNumero)
        {
            txtPantalla.Text = "0,";
            _nuevoNumero = false;
            return;
        }

        if (!txtPantalla.Text.Contains(','))
        {
            txtPantalla.Text += ",";
        }
    }

    private void btnOperacion_Click(object sender, RoutedEventArgs e)
    {
        string operacion = (string)((Button)sender).Content;
        ProcesarOperacion(operacion);
    }

    private void ProcesarOperacion(string operacion)
    {
        if (_esError)
            return;

        // Si ya había una operación pendiente y se introdujo un segundo operando, resolver antes
        if (_operacionPendiente != null && !_nuevoNumero)
        {
            if (!CalcularResultadoParcial())
                return;
        }

        if (decimal.TryParse(txtPantalla.Text, NumberStyles.Number, Cultura, out decimal valorPantalla))
        {
            _operando1 = valorPantalla;
            _operacionPendiente = operacion;
            txtOperacion.Text = $"{FormatearNumero(valorPantalla)} {operacion}";
            _nuevoNumero = true;
            _esResultado = false;
        }
    }

    private void btnIgual_Click(object sender, RoutedEventArgs e)
    {
        ProcesarIgual();
    }

    private void ProcesarIgual()
    {
        if (_esError || _operacionPendiente == null || _operando1 == null)
            return;

        if (!decimal.TryParse(txtPantalla.Text, NumberStyles.Number, Cultura, out decimal operando2))
            return;

        decimal op1 = _operando1.Value;
        string op = _operacionPendiente;

        if (op == "÷" && operando2 == 0)
        {
            MostrarErrorDivisionCero();
            return;
        }

        decimal resultado = RealizarCalculo(op1, operando2, op);

        txtOperacion.Text = $"{FormatearNumero(op1)} {op} {FormatearNumero(operando2)} =";
        txtPantalla.Text = FormatearNumero(resultado);

        _operando1 = null;
        _operacionPendiente = null;
        _esResultado = true;
        _nuevoNumero = true;
    }

    private bool CalcularResultadoParcial()
    {
        if (_operacionPendiente == null || _operando1 == null)
            return true;

        if (!decimal.TryParse(txtPantalla.Text, NumberStyles.Number, Cultura, out decimal operando2))
            return false;

        decimal op1 = _operando1.Value;
        string op = _operacionPendiente;

        if (op == "÷" && operando2 == 0)
        {
            MostrarErrorDivisionCero();
            return false;
        }

        decimal resultado = RealizarCalculo(op1, operando2, op);
        txtPantalla.Text = FormatearNumero(resultado);
        _operando1 = resultado;
        return true;
    }

    private static decimal RealizarCalculo(decimal n1, decimal n2, string operador)
    {
        return operador switch
        {
            "+" => n1 + n2,
            "−" or "-" => n1 - n2,
            "×" or "*" => n1 * n2,
            "÷" or "/" => n1 / n2,
            _ => n2
        };
    }

    private void btnBorrar_Click(object sender, RoutedEventArgs e)
    {
        ProcesarBorrar();
    }

    private void ProcesarBorrar()
    {
        ResetearEstado();
    }

    private void btnRetroceso_Click(object sender, RoutedEventArgs e)
    {
        ProcesarRetroceso();
    }

    private void ProcesarRetroceso()
    {
        // De un resultado, en error o tras un operador, retroceso no borra nada
        if (_esResultado || _esError || _nuevoNumero)
            return;

        if (txtPantalla.Text.Length > 1)
        {
            txtPantalla.Text = txtPantalla.Text[..^1];
            if (txtPantalla.Text == "-" || string.IsNullOrEmpty(txtPantalla.Text))
            {
                txtPantalla.Text = "0";
                _nuevoNumero = true;
            }
        }
        else
        {
            txtPantalla.Text = "0";
            _nuevoNumero = true;
        }
    }

    private void ResetearEstado()
    {
        _operando1 = null;
        _operacionPendiente = null;
        _nuevoNumero = true;
        _esResultado = false;
        _esError = false;
        txtPantalla.FontSize = 36;
        txtPantalla.Text = "0";
        txtOperacion.Text = string.Empty;
    }

    private void MostrarErrorDivisionCero()
    {
        _esError = true;
        _operando1 = null;
        _operacionPendiente = null;
        _nuevoNumero = true;
        _esResultado = false;
        txtPantalla.FontSize = 18;
        txtPantalla.Text = MensajeDivisionCero;
        txtOperacion.Text = string.Empty;
    }

    private static string FormatearNumero(decimal numero)
    {
        return numero.ToString("G29", Cultura);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Cifras teclado principal
        if (e.Key >= Key.D0 && e.Key <= Key.D9 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
        {
            string digito = ((int)e.Key - (int)Key.D0).ToString();
            btnDigito_Click(new Button { Content = digito }, e);
            e.Handled = true;
            return;
        }

        // Cifras teclado numérico
        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            string digito = ((int)e.Key - (int)Key.NumPad0).ToString();
            btnDigito_Click(new Button { Content = digito }, e);
            e.Handled = true;
            return;
        }

        // Coma y punto
        if (e.Key == Key.OemComma || e.Key == Key.OemPeriod || e.Key == Key.Decimal)
        {
            btnComa_Click(btnComa, e);
            e.Handled = true;
            return;
        }

        // Operaciones
        if (e.Key == Key.Add || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) == 0))
        {
            btnOperacion_Click(new Button { Content = "+" }, e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
        {
            btnOperacion_Click(new Button { Content = "−" }, e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Multiply)
        {
            btnOperacion_Click(new Button { Content = "×" }, e);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Divide)
        {
            btnOperacion_Click(new Button { Content = "÷" }, e);
            e.Handled = true;
            return;
        }

        // Igual / Intro
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            btnIgual_Click(btnIgual, e);
            e.Handled = true;
            return;
        }

        // Retroceso
        if (e.Key == Key.Back)
        {
            btnRetroceso_Click(btnRetroceso, e);
            e.Handled = true;
            return;
        }

        // Borrar todo (Escape o C)
        if (e.Key == Key.Escape || e.Key == Key.C)
        {
            btnBorrar_Click(btnBorrar, e);
            e.Handled = true;
            return;
        }
    }
}
