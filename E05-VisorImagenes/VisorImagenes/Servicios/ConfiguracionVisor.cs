namespace VisorImagenes.Servicios;

/// <summary>
/// Modelo de datos que almacena las preferencias y el estado de la ventana del visor de imágenes.
/// </summary>
public class ConfiguracionVisor
{
    public double Left { get; set; } = 100;
    public double Top { get; set; } = 100;
    public double Width { get; set; } = 800;
    public double Height { get; set; } = 550;
    public double AnchoLista { get; set; } = 220;
    public bool Maximizada { get; set; } = false;
    public string Tema { get; set; } = "Del sistema";
    public bool MostrarLista { get; set; } = true;
    public string UltimaCarpeta { get; set; } = string.Empty;

    // Desafío opcional: persistir última imagen y zoom
    public string? UltimaImagen { get; set; } = null;
    public double? UltimoZoom { get; set; } = 1.0;
    public bool? UltimoZoomAjustado { get; set; } = true;
}
