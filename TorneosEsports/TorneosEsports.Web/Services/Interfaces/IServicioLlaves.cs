using TorneosEsports.Web.Models.ViewModels;

namespace TorneosEsports.Web.Services.Interfaces;

/// <summary>
/// Regla distintiva, segunda parte: cerrar inscripciones, generar las llaves y registrar resultados.
/// </summary>
public interface IServicioLlaves
{
    /// <summary>
    /// Cierra las inscripciones del torneo y genera automáticamente las llaves con los
    /// equipos que tienen la inscripción pagada.
    /// </summary>
    /// <param name="torneoId">Id del torneo.</param>
    /// <returns>Resultado con la cantidad de partidos generados.</returns>
    Task<ResultadoOperacion> CerrarInscripcionesYGenerarAsync(int torneoId);

    /// <summary>Arma la vista del bracket: rondas, partidos, equipos y campeón.</summary>
    /// <param name="torneoId">Id del torneo.</param>
    /// <returns>El bracket, o null si el torneo no existe.</returns>
    Task<LlavesViewModel?> ObtenerBracketAsync(int torneoId);

    /// <summary>Registra el ganador de un partido y lo pasa a la ronda siguiente.</summary>
    /// <param name="partidoId">Id del partido.</param>
    /// <param name="ganadorId">Id del equipo ganador.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<ResultadoOperacion> RegistrarResultadoAsync(int partidoId, int ganadorId);
}
