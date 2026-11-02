using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Models.ViewModels;
using TorneosEsports.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TorneosEsports.Web.Services.Llaves;

/// <summary>
/// Regla distintiva (segunda parte): al cerrar inscripciones se generan las llaves
/// automáticamente, y cada resultado hace avanzar al ganador.
/// </summary>
/// <remarks>
/// Este servicio coordina; las reglas puntuales están en el modelo
/// (<see cref="Torneo"/>, <see cref="Partido"/>) y en el generador. Recibe sus
/// dependencias por constructor (inyección de dependencias).
/// </remarks>
/// <param name="db">Contexto de la base de datos.</param>
/// <param name="generador">Generador del formato de llaves.</param>
/// <param name="notificador">Aviso de cambios (SignalR cuando exista).</param>
/// <param name="reloj">Fuente de la hora actual; se inyecta para poder probar.</param>
/// <param name="logger">Bitácora (ILogger).</param>
public class ServicioLlaves(
    ApplicationDbContext db,
    IGeneradorLlaves generador,
    INotificadorLlaves notificador,
    TimeProvider reloj,
    ILogger<ServicioLlaves> logger) : IServicioLlaves
{
    /// <inheritdoc />
    public async Task<ResultadoOperacion> CerrarInscripcionesYGenerarAsync(int torneoId)
    {
        var torneo = await db.Torneos
            .Include(t => t.Equipos)
            .FirstOrDefaultAsync(t => t.Id == torneoId);

        if (torneo is null)
            return ResultadoOperacion.Fallo("El torneo no existe.");

        if (torneo.Estado != EstadoTorneo.Abierto)
            return ResultadoOperacion.Fallo("Las inscripciones de este torneo ya están cerradas.");

        // Solo entran a las llaves los equipos que pagaron, en orden de inscripción (semilla 1 = primero).
        var equiposEnOrden = torneo.Equipos
            .Where(e => e.Pagado)
            .OrderBy(e => e.FechaCreacion)
            .ThenBy(e => e.Id)
            .Select(e => e.Id)
            .ToList();

        if (equiposEnOrden.Count < Torneo.MinimoEquiposParaLlaves)
            return ResultadoOperacion.Fallo(
                $"Se necesitan al menos {Torneo.MinimoEquiposParaLlaves} equipos con la inscripción pagada; hay {equiposEnOrden.Count}.");

        torneo.CerrarInscripciones();
        var partidos = generador.Generar(torneo.Id, equiposEnOrden);
        db.Partidos.AddRange(partidos);
        torneo.IniciarJuego();

        await db.SaveChangesAsync();
        logger.LogInformation(
            "Llaves generadas para el torneo {TorneoId}: {Equipos} equipos, {Partidos} partidos",
            torneoId, equiposEnOrden.Count, partidos.Count);

        await notificador.LlavesActualizadasAsync(torneoId);
        return ResultadoOperacion.Ok(
            $"Inscripciones cerradas. Se generaron {partidos.Count} partidos para {equiposEnOrden.Count} equipos.");
    }

    /// <inheritdoc />
    public async Task<ResultadoOperacion> RegistrarResultadoAsync(int partidoId, int ganadorId)
    {
        var torneo = await db.Torneos
            .Include(t => t.Partidos)
            .FirstOrDefaultAsync(t => t.Partidos.Any(p => p.Id == partidoId));

        if (torneo is null)
            return ResultadoOperacion.Fallo("El partido no existe.");

        if (torneo.Estado != EstadoTorneo.EnJuego)
            return ResultadoOperacion.Fallo("El torneo no está en juego.");

        var partido = torneo.Partidos.First(p => p.Id == partidoId);
        try
        {
            partido.RegistrarGanador(ganadorId, reloj.GetUtcNow().UtcDateTime);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            return ResultadoOperacion.Fallo(ex.Message);
        }

        // Si no hay partido siguiente, era la final: el torneo termina.
        var siguiente = AvanceDeLlaves.AvanzarGanador(partido, ganadorId, torneo.Partidos);
        if (siguiente is null)
            torneo.Finalizar();

        await db.SaveChangesAsync();
        logger.LogInformation(
            "Resultado registrado en el partido {PartidoId} del torneo {TorneoId}: gana el equipo {GanadorId}",
            partidoId, torneo.Id, ganadorId);

        await notificador.LlavesActualizadasAsync(torneo.Id);
        return ResultadoOperacion.Ok(siguiente is null ? "¡Torneo finalizado! Ya hay campeón." : "Resultado registrado.");
    }

    /// <inheritdoc />
    public async Task<LlavesViewModel?> ObtenerBracketAsync(int torneoId)
    {
        var torneo = await db.Torneos
            .AsNoTracking()
            .Include(t => t.Evento)
            .Include(t => t.Equipos)
            .Include(t => t.Partidos)
            .FirstOrDefaultAsync(t => t.Id == torneoId);

        if (torneo is null)
            return null;

        var nombres = torneo.Equipos.ToDictionary(e => e.Id, e => e.Nombre);
        string Nombre(int? id, bool esBye) =>
            id is int valor && nombres.TryGetValue(valor, out var nombre) ? nombre
            : esBye ? "— (pasa directo)" : "Por definir";

        int totalRondas = torneo.Partidos.Count == 0 ? 0 : torneo.Partidos.Max(p => p.Ronda);
        var final = torneo.Partidos.FirstOrDefault(p => p.Ronda == totalRondas);

        return new LlavesViewModel
        {
            TorneoId = torneo.Id,
            Juego = torneo.Juego,
            Formato = torneo.Formato,
            Evento = torneo.Evento?.Nombre ?? string.Empty,
            Estado = torneo.Estado,
            EquiposPagados = torneo.Equipos.Count(e => e.Pagado),
            Campeon = final?.GanadorId is int campeonId ? nombres.GetValueOrDefault(campeonId) : null,
            Rondas = torneo.Partidos
                .GroupBy(p => p.Ronda)
                .OrderBy(g => g.Key)
                .Select(g => new RondaLlavesViewModel
                {
                    Numero = g.Key,
                    Nombre = AvanceDeLlaves.NombreDeRonda(g.Key, totalRondas),
                    Partidos = g.OrderBy(p => p.Posicion).Select(p => new PartidoLlavesViewModel
                    {
                        Id = p.Id,
                        EquipoAId = p.EquipoAId,
                        EquipoA = Nombre(p.EquipoAId, p.EsBye),
                        EquipoBId = p.EquipoBId,
                        EquipoB = Nombre(p.EquipoBId, p.EsBye),
                        GanadorId = p.GanadorId,
                        EsBye = p.EsBye,
                        ListoParaJugar = p.ListoParaJugar,
                    }).ToList(),
                })
                .ToList(),
        };
    }
}
