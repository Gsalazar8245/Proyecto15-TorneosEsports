using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Services.Interfaces;

/// <summary>
/// Operaciones del catálogo 2: consultar y mantener los torneos de cada evento.
/// </summary>
/// <remarks>
/// El controlador depende de esta interfaz y no de la clase concreta
/// (principio de inversión de dependencias): se puede cambiar la implementación sin tocar
/// el controlador.
/// </remarks>
public interface IServicioTorneos
{
    /// <summary>Lista los torneos, opcionalmente solo los de un evento.</summary>
    /// <param name="eventoId">Id del evento, o null para todos.</param>
    /// <returns>Torneos con su evento y sus equipos cargados.</returns>
    Task<IReadOnlyList<Torneo>> ListarAsync(int? eventoId = null);

    /// <summary>Obtiene un torneo con su evento y sus equipos.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>El torneo, o null si no existe.</returns>
    Task<Torneo?> ObtenerAsync(int id);

    /// <summary>Lista los eventos para el selector del formulario.</summary>
    /// <returns>Eventos ordenados por fecha.</returns>
    Task<IReadOnlyList<Evento>> ListarEventosAsync();

    /// <summary>Crea un torneo nuevo con inscripciones abiertas.</summary>
    /// <param name="torneo">Datos del torneo.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<ResultadoOperacion> CrearAsync(Torneo torneo);

    /// <summary>Actualiza los datos de un torneo que todavía está abierto.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <param name="datos">Valores nuevos.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<ResultadoOperacion> ActualizarAsync(int id, Torneo datos);

    /// <summary>Elimina un torneo que no tiene equipos inscritos.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<ResultadoOperacion> EliminarAsync(int id);
}
