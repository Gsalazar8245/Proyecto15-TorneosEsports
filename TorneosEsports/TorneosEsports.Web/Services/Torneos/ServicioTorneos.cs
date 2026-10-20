using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TorneosEsports.Web.Services.Torneos;

/// <summary>
/// Mantenimiento del catálogo 2 (torneos) con Entity Framework.
/// </summary>
/// <remarks>
/// Aquí viven las validaciones que dependen de la base de datos
/// (que el evento exista, que no haya equipos inscritos antes de borrar). Las
/// validaciones de formato las hacen las DataAnnotations del ViewModel.
/// </remarks>
/// <param name="db">Contexto de la base de datos.</param>
/// <param name="logger">Bitácora (ILogger).</param>
public class ServicioTorneos(ApplicationDbContext db, ILogger<ServicioTorneos> logger) : IServicioTorneos
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Torneo>> ListarAsync(int? eventoId = null)
    {
        var consulta = db.Torneos
            .AsNoTracking()
            .Include(t => t.Evento)
            .Include(t => t.Equipos)
            .AsQueryable();

        if (eventoId.HasValue)
            consulta = consulta.Where(t => t.EventoId == eventoId.Value);

        return await consulta
            .OrderBy(t => t.Evento!.Fecha)
            .ThenBy(t => t.Juego)
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<Torneo?> ObtenerAsync(int id) =>
        db.Torneos
            .AsNoTracking()
            .Include(t => t.Evento)
            .Include(t => t.Equipos).ThenInclude(e => e.Integrantes)
            .FirstOrDefaultAsync(t => t.Id == id);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Evento>> ListarEventosAsync() =>
        await db.Eventos.AsNoTracking().OrderBy(e => e.Fecha).ToListAsync();

    /// <inheritdoc />
    public async Task<ResultadoOperacion> CrearAsync(Torneo torneo)
    {
        var evento = await db.Eventos.FindAsync(torneo.EventoId);
        if (evento is null)
            return ResultadoOperacion.Fallo("El evento seleccionado no existe.");

        if (torneo.CierreInscripciones >= evento.Fecha)
            return ResultadoOperacion.Fallo("Las inscripciones deben cerrar antes de la fecha del evento.");

        torneo.Estado = EstadoTorneo.Abierto;
        db.Torneos.Add(torneo);
        await db.SaveChangesAsync();

        logger.LogInformation("Torneo creado: {TorneoId} {Juego} en el evento {EventoId}", torneo.Id, torneo.Juego, torneo.EventoId);
        return ResultadoOperacion.Ok($"Se creó el torneo de {torneo.Juego}.");
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> ActualizarAsync(int id, Torneo datos)
    {
        var torneo = await db.Torneos.Include(t => t.Equipos).FirstOrDefaultAsync(t => t.Id == id);
        if (torneo is null)
            return ResultadoOperacion.Fallo("El torneo no existe.");

        if (!torneo.PuedeEditarse)
            return ResultadoOperacion.Fallo("Solo se puede editar un torneo con las inscripciones abiertas.");

        var evento = await db.Eventos.FindAsync(datos.EventoId);
        if (evento is null)
            return ResultadoOperacion.Fallo("El evento seleccionado no existe.");

        if (datos.CierreInscripciones >= evento.Fecha)
            return ResultadoOperacion.Fallo("Las inscripciones deben cerrar antes de la fecha del evento.");

        if (datos.CupoEquipos < torneo.Equipos.Count)
            return ResultadoOperacion.Fallo($"El cupo no puede ser menor que los {torneo.Equipos.Count} equipos ya inscritos.");

        if (torneo.Equipos.Count > 0 && datos.EventoId != torneo.EventoId)
            return ResultadoOperacion.Fallo("No se puede cambiar de evento un torneo que ya tiene equipos inscritos.");

        torneo.EventoId = datos.EventoId;
        torneo.Juego = datos.Juego;
        torneo.Formato = datos.Formato;
        torneo.CupoEquipos = datos.CupoEquipos;
        torneo.PrecioInscripcion = datos.PrecioInscripcion;
        torneo.CierreInscripciones = datos.CierreInscripciones;
        await db.SaveChangesAsync();

        logger.LogInformation("Torneo actualizado: {TorneoId}", id);
        return ResultadoOperacion.Ok("Los cambios se guardaron.");
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> EliminarAsync(int id)
    {
        var torneo = await db.Torneos.Include(t => t.Equipos).FirstOrDefaultAsync(t => t.Id == id);
        if (torneo is null)
            return ResultadoOperacion.Fallo("El torneo no existe.");

        if (torneo.Equipos.Count > 0)
            return ResultadoOperacion.Fallo("No se puede eliminar: el torneo ya tiene equipos inscritos.");

        db.Torneos.Remove(torneo);
        await db.SaveChangesAsync();

        logger.LogInformation("Torneo eliminado: {TorneoId}", id);
        return ResultadoOperacion.Ok($"Se eliminó el torneo de {torneo.Juego}.");
    }
}
