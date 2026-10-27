using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Models.ViewModels;

/// <summary>
/// Lo que necesita la vista del bracket: datos del torneo, rondas, partidos y campeón.
/// </summary>
/// <remarks>La vista solo dibuja; todo lo calcula el servicio de llaves.</remarks>
public class LlavesViewModel
{
    /// <summary>Id del torneo.</summary>
    public int TorneoId { get; set; }

    /// <summary>Juego del torneo.</summary>
    public string Juego { get; set; } = string.Empty;

    /// <summary>Formato del torneo.</summary>
    public string Formato { get; set; } = string.Empty;

    /// <summary>Nombre del evento.</summary>
    public string Evento { get; set; } = string.Empty;

    /// <summary>Estado actual del torneo.</summary>
    public EstadoTorneo Estado { get; set; }

    /// <summary>Equipos con la inscripción pagada (los que entran a las llaves).</summary>
    public int EquiposPagados { get; set; }

    /// <summary>Nombre del campeón, cuando el torneo terminó.</summary>
    public string? Campeon { get; set; }

    /// <summary>Rondas, de la primera a la final.</summary>
    public List<RondaLlavesViewModel> Rondas { get; set; } = [];

    /// <summary>Las llaves ya fueron generadas.</summary>
    public bool TieneLlaves => Rondas.Count > 0;
}

/// <summary>Una ronda del bracket.</summary>
public class RondaLlavesViewModel
{
    /// <summary>Número de ronda (1 es la primera).</summary>
    public int Numero { get; set; }

    /// <summary>Nombre visible, por ejemplo «Semifinal».</summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Partidos de la ronda, ordenados por posición.</summary>
    public List<PartidoLlavesViewModel> Partidos { get; set; } = [];
}

/// <summary>Un partido tal como se dibuja en el bracket.</summary>
public class PartidoLlavesViewModel
{
    /// <summary>Id del partido.</summary>
    public int Id { get; set; }

    /// <summary>Id del equipo A, si ya se conoce.</summary>
    public int? EquipoAId { get; set; }

    /// <summary>Nombre del equipo A, o «Por definir».</summary>
    public string EquipoA { get; set; } = string.Empty;

    /// <summary>Id del equipo B, si ya se conoce.</summary>
    public int? EquipoBId { get; set; }

    /// <summary>Nombre del equipo B, o «Por definir».</summary>
    public string EquipoB { get; set; } = string.Empty;

    /// <summary>Id del ganador, si ya hay resultado.</summary>
    public int? GanadorId { get; set; }

    /// <summary>El equipo pasó sin rival.</summary>
    public bool EsBye { get; set; }

    /// <summary>Tiene los dos equipos y aún no tiene ganador.</summary>
    public bool ListoParaJugar { get; set; }
}
