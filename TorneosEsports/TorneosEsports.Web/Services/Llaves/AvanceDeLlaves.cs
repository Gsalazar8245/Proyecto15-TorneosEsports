using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Services.Llaves;

/// <summary>
/// Regla de avance: dónde se ubica el ganador de un partido en la ronda siguiente.
/// </summary>
/// <remarks>
/// La usan el generador (para los byes) y el servicio de llaves
/// (para los resultados), así la regla existe en un solo lugar.
/// </remarks>
public static class AvanceDeLlaves
{
    /// <summary>
    /// Coloca al ganador en el partido de la ronda siguiente: posición / 2, lado A si la
    /// posición es par y lado B si es impar.
    /// </summary>
    /// <param name="partido">Partido que ya tiene ganador.</param>
    /// <param name="ganadorId">Id del equipo ganador.</param>
    /// <param name="partidosDelTorneo">Todos los partidos del torneo.</param>
    /// <returns>El partido siguiente, o null si el partido era la final.</returns>
    public static Partido? AvanzarGanador(Partido partido, int ganadorId, IEnumerable<Partido> partidosDelTorneo)
    {
        var siguiente = partidosDelTorneo.FirstOrDefault(p =>
            p.Ronda == partido.Ronda + 1 && p.Posicion == partido.PosicionSiguiente);

        siguiente?.ColocarEquipo(ganadorId, partido.GanadorVaComoEquipoA);
        return siguiente;
    }

    /// <summary>Nombre de la ronda según cuántas faltan para la final.</summary>
    /// <param name="ronda">Número de ronda (1 es la primera).</param>
    /// <param name="totalRondas">Cantidad total de rondas.</param>
    /// <returns>«Final», «Semifinal», «Cuartos de final», «Octavos de final» o «Ronda n».</returns>
    public static string NombreDeRonda(int ronda, int totalRondas) => (totalRondas - ronda) switch
    {
        0 => "Final",
        1 => "Semifinal",
        2 => "Cuartos de final",
        3 => "Octavos de final",
        _ => $"Ronda {ronda}",
    };
}
