using System;
using System.IO;
using System.Text.Json;

namespace VisorImagenes.Servicios;

/// <summary>
/// Servicio responsable de cargar y guardar la configuración del usuario en disco.
/// </summary>
public static class ServicioConfiguracion
{
    private static readonly string RutaConfiguracion = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "VisorImagenes",
        "configuracion.json");

    /// <summary>
    /// Carga la configuración del usuario desde %AppData%\VisorImagenes\configuracion.json.
    /// Si el fichero no existe o está corrupto, devuelve una configuración con valores por defecto.
    /// </summary>
    public static ConfiguracionVisor Cargar()
    {
        try
        {
            if (File.Exists(RutaConfiguracion))
            {
                string json = File.ReadAllText(RutaConfiguracion);
                var config = JsonSerializer.Deserialize<ConfiguracionVisor>(json);
                if (config != null)
                {
                    return config;
                }
            }
        }
        catch (Exception)
        {
            // Un archivo dañado o inaccesible no interrumpe el arranque
        }

        return new ConfiguracionVisor();
    }

    /// <summary>
    /// Guarda la configuración actual en %AppData%\VisorImagenes\configuracion.json.
    /// Los errores de escritura no bloquean el cierre de la aplicación.
    /// </summary>
    public static void Guardar(ConfiguracionVisor config)
    {
        try
        {
            string? carpeta = Path.GetDirectoryName(RutaConfiguracion);
            if (!string.IsNullOrEmpty(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var opciones = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(config, opciones);
            File.WriteAllText(RutaConfiguracion, json);
        }
        catch (Exception)
        {
            // Errores al guardar no impiden cerrar la aplicación
        }
    }
}
