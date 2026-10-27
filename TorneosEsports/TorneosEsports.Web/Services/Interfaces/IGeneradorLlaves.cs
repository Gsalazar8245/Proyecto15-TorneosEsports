using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Services.Interfaces;

/// <summary>
/// Genera los partidos de las llaves a partir de los equipos inscritos.
/// </summary>
/// <remarks>
/// No toca la base de datos: recibe ids y devuelve partidos en
/// memoria. Por eso se puede probar solo, sin levantar la aplicación.
/// </remarks>
public interface IGeneradorLlaves
{
    /// <summary>Nombre del formato que genera, por ejemplo «Eliminación directa».</summary>
    string Formato { get; }

    /// <summary>Genera todos los partidos de todas las rondas.</summary>
    /// <param name="torneoId">Torneo al que pertenecen los partidos.</param>
    /// <param name="equiposEnOrden">Ids de los equipos en orden de siembra (el primero es la semilla 1).</param>
    /// <returns>Partidos de la primera ronda con sus equipos y las rondas siguientes vacías.</returns>
    IReadOnlyList<Partido> Generar(int torneoId, IReadOnlyList<int> equiposEnOrden);
}
