using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Models.ViewModels;

public class ComprobanteViewModel
{
    public Equipo Equipo { get; set; } = null!;
    public string TorneoJuego { get; set; } = "";
    public string QrBase64 { get; set; } = "";
}

public class ValidacionViewModel
{
    public string? Codigo { get; set; }
    public Equipo? Equipo { get; set; }
    public string? TorneoJuego { get; set; }
    public string? Mensaje { get; set; }
    public bool Valido { get; set; }
}