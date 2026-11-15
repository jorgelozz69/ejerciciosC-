using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Inscripcion;

/// <summary>
/// Lógica de interacción para MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Brush BordeError = Brushes.Crimson;
    private static readonly Brush BordeNormal = (Brush)new BrushConverter().ConvertFromString("#D1D5DB")!;

    public MainWindow()
    {
        InitializeComponent();

        // Configuración requerida: no permitir fechas de nacimiento futuras
        dtpNacimiento.DisplayDateEnd = DateTime.Today;
    }

    /// <summary>
    /// Manejador de la casilla 'Todas las actividades'.
    /// Marca o desmarca en bloque las tres actividades individuales.
    /// </summary>
    private void chkTodas_Click(object sender, RoutedEventArgs e)
    {
        bool marcar = chkTodas.IsChecked == true;
        chkCoro.IsChecked = marcar;
        chkLenguaje.IsChecked = marcar;
        chkConciertos.IsChecked = marcar;
    }

    /// <summary>
    /// Manejador compartido por las tres actividades extra.
    /// Calcula el estado de tres estados de chkTodas (marcada, desmarcada o indeterminada).
    /// </summary>
    private void chkExtra_Click(object sender, RoutedEventArgs e)
    {
        ActualizarEstadoTodas();
    }

    private void ActualizarEstadoTodas()
    {
        bool c = chkCoro.IsChecked == true;
        bool l = chkLenguaje.IsChecked == true;
        bool co = chkConciertos.IsChecked == true;

        if (c && l && co)
        {
            chkTodas.IsChecked = true;
        }
        else if (!c && !l && !co)
        {
            chkTodas.IsChecked = false;
        }
        else
        {
            // Estado indeterminado: algunas marcadas y otras no
            chkTodas.IsChecked = null;
        }
    }

    /// <summary>
    /// Actualiza el contador de caracteres al escribir en el cuadro de texto de observaciones.
    /// </summary>
    private void txtObservaciones_TextChanged(object sender, TextChangedEventArgs e)
    {
        ActualizarContadorObservaciones();
    }

    /// <summary>
    /// Actualiza el contador mostrando también el número de caracteres seleccionados.
    /// </summary>
    private void txtObservaciones_SelectionChanged(object sender, RoutedEventArgs e)
    {
        ActualizarContadorObservaciones();
    }

    private void ActualizarContadorObservaciones()
    {
        if (txtObservaciones == null || txtContador == null)
            return;

        int total = txtObservaciones.Text.Length;
        int seleccionados = txtObservaciones.SelectedText.Length;

        if (seleccionados > 0)
        {
            txtContador.Text = $"{total} / 500 · {seleccionados} seleccionados";
        }
        else
        {
            txtContador.Text = $"{total} / 500";
        }
    }

    /// <summary>
    /// Valida exhaustivamente todos los campos del formulario y muestra el resumen o lista de errores.
    /// </summary>
    private void btnInscribir_Click(object sender, RoutedEventArgs e)
    {
        var errores = new List<string>();
        Control? primerControlError = null;

        // 1. Nombre y apellidos (obligatorio)
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            errores.Add("• El nombre y apellidos son obligatorios.");
            primerControlError ??= txtNombre;
            MarcarError(txtNombre, true);
        }
        else
        {
            MarcarError(txtNombre, false);
        }

        // 2. Fecha de nacimiento (válida y al menos 6 años)
        if (!dtpNacimiento.SelectedDate.HasValue)
        {
            errores.Add("• Debe seleccionar una fecha de nacimiento.");
            primerControlError ??= dtpNacimiento;
            MarcarError(dtpNacimiento, true);
        }
        else
        {
            var fecha = dtpNacimiento.SelectedDate.Value;
            if (fecha > DateTime.Today)
            {
                errores.Add("• La fecha de nacimiento no puede ser posterior a hoy.");
                primerControlError ??= dtpNacimiento;
                MarcarError(dtpNacimiento, true);
            }
            else if (fecha.AddYears(6) > DateTime.Today)
            {
                errores.Add("• El alumno debe tener al menos 6 años de edad.");
                primerControlError ??= dtpNacimiento;
                MarcarError(dtpNacimiento, true);
            }
            else
            {
                MarcarError(dtpNacimiento, false);
            }
        }

        // 3. Teléfono (exactamente 9 dígitos numéricos)
        string tel = txtTelefono.Text.Trim();
        if (string.IsNullOrWhiteSpace(tel) || tel.Length != 9 || !tel.All(char.IsDigit))
        {
            errores.Add("• El teléfono debe contener exactamente 9 cifras numéricas sin espacios.");
            primerControlError ??= txtTelefono;
            MarcarError(txtTelefono, true);
        }
        else
        {
            MarcarError(txtTelefono, false);
        }

        // 4. Instrumento seleccionado
        string? instrumentoTexto = null;
        if (cmbInstrumento.SelectedItem is ComboBoxItem itemInstrumento)
        {
            instrumentoTexto = itemInstrumento.Tag?.ToString() ?? "Instrumento seleccionado";
            MarcarError(cmbInstrumento, false);
        }
        else
        {
            errores.Add("• Debe seleccionar un instrumento.");
            primerControlError ??= cmbInstrumento;
            MarcarError(cmbInstrumento, true);
        }

        // 5. Nivel seleccionado
        string? nivelTexto = null;
        if (rbIniciacion.IsChecked == true) nivelTexto = "Iniciación";
        else if (rbMedio.IsChecked == true) nivelTexto = "Medio";
        else if (rbAvanzado.IsChecked == true) nivelTexto = "Avanzado";

        if (nivelTexto == null)
        {
            errores.Add("• Debe seleccionar un nivel (Iniciación, Medio o Avanzado).");
            primerControlError ??= rbIniciacion;
        }

        // 6. Horario seleccionado
        string? horarioTexto = null;
        if (rbManana.IsChecked == true) horarioTexto = "Mañana";
        else if (rbTarde.IsChecked == true) horarioTexto = "Tarde";

        if (horarioTexto == null)
        {
            errores.Add("• Debe seleccionar un horario (Mañana o Tarde).");
            primerControlError ??= rbManana;
        }

        // 7. Nombre de usuario
        if (string.IsNullOrWhiteSpace(txtUsuario.Text))
        {
            errores.Add("• El nombre de usuario es obligatorio.");
            primerControlError ??= txtUsuario;
            MarcarError(txtUsuario, true);
        }
        else
        {
            MarcarError(txtUsuario, false);
        }

        // 8. Contraseña y repetición
        // Regla: La repetición no se comprueba mientras la contraseña sea demasiado corta (< 8)
        string pass = pwdContrasena.Password;
        string repeticion = pwdRepetir.Password;

        if (pass.Length < 8 || pass.Length > 20)
        {
            errores.Add("• La contraseña debe tener entre 8 y 20 caracteres.");
            primerControlError ??= pwdContrasena;
            MarcarError(pwdContrasena, true);
        }
        else
        {
            MarcarError(pwdContrasena, false);

            if (pass != repeticion)
            {
                errores.Add("• Las contraseñas no coinciden.");
                primerControlError ??= pwdRepetir;
                MarcarError(pwdRepetir, true);
            }
            else
            {
                MarcarError(pwdRepetir, false);
            }
        }

        // Si se encontraron errores, mostrarlos en un único mensaje y enfocar el primero
        if (errores.Count > 0)
        {
            MessageBox.Show(
                "Se han detectado los siguientes errores en el formulario:\n\n" + string.Join("\n", errores),
                "Errores de inscripción",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            primerControlError?.Focus();
            return;
        }

        // Cálculo de cuota mensual: Base 40 € + 10 € por cada actividad extra
        var actividades = new List<string>();
        if (chkCoro.IsChecked == true) actividades.Add("Coro");
        if (chkLenguaje.IsChecked == true) actividades.Add("Lenguaje musical");
        if (chkConciertos.IsChecked == true) actividades.Add("Conciertos");

        int cuota = 40 + (actividades.Count * 10);
        string actividadesTexto = actividades.Count > 0 ? string.Join(", ", actividades) : "Ninguna";

        // Resumen completo (NUNCA muestra la contraseña)
        string resumen =
            "¡Inscripción realizada con éxito!\n\n" +
            $"• Alumno: {txtNombre.Text.Trim()}\n" +
            $"• Fecha de nacimiento: {dtpNacimiento.SelectedDate!.Value:dd/MM/yyyy}\n" +
            $"• Teléfono: {tel}\n" +
            $"• Instrumento: {instrumentoTexto}\n" +
            $"• Nivel: {nivelTexto}\n" +
            $"• Horario: {horarioTexto}\n" +
            $"• Actividades extra: {actividadesTexto}\n" +
            $"• Usuario: {txtUsuario.Text.Trim()}\n" +
            $"• Cuota mensual: {cuota} €";

        MessageBox.Show(
            resumen,
            "Resumen de inscripción",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    /// <summary>
    /// Restablece todos los campos del formulario a su estado vacío inicial y pone el foco en el nombre.
    /// </summary>
    private void btnLimpiar_Click(object sender, RoutedEventArgs e)
    {
        txtNombre.Text = string.Empty;
        dtpNacimiento.SelectedDate = null;
        txtTelefono.Text = string.Empty;
        cmbInstrumento.SelectedIndex = -1;

        rbIniciacion.IsChecked = false;
        rbMedio.IsChecked = false;
        rbAvanzado.IsChecked = false;

        rbManana.IsChecked = false;
        rbTarde.IsChecked = false;

        chkTodas.IsChecked = false;
        chkCoro.IsChecked = false;
        chkLenguaje.IsChecked = false;
        chkConciertos.IsChecked = false;

        txtObservaciones.Text = string.Empty;
        txtContador.Text = "0 / 500";

        txtUsuario.Text = string.Empty;
        pwdContrasena.Password = string.Empty;
        pwdRepetir.Password = string.Empty;

        LimpiarBordesError();

        txtNombre.Focus();
    }

    private void MarcarError(Control control, bool esError)
    {
        control.BorderBrush = esError ? BordeError : BordeNormal;
        control.BorderThickness = esError ? new Thickness(1.5) : new Thickness(1);
    }

    private void LimpiarBordesError()
    {
        MarcarError(txtNombre, false);
        MarcarError(dtpNacimiento, false);
        MarcarError(txtTelefono, false);
        MarcarError(cmbInstrumento, false);
        MarcarError(txtUsuario, false);
        MarcarError(pwdContrasena, false);
        MarcarError(pwdRepetir, false);
    }
}
