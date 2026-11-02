using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Models.ViewModels;

public class BracketViewModel
{
    public int TorneoId { get; set; }
    public string Juego { get; set; } = "";
    public List<Partido> Partidos { get; set; } = new();
}