using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;

[Authorize(Roles = "Administrador")]
public class TableroController : Controller
{
    private readonly ApplicationDbContext _context;

    public TableroController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Tablero
    public async Task<IActionResult> Index()
    {
        var torneos = await _context.Torneos
            .AsNoTracking()
            .OrderBy(t => t.EventoId)
            .ThenBy(t => t.Juego)
            .Select(t => new FilaTorneo
            {
                Juego = t.Juego,
                Evento = t.Evento != null ? t.Evento.Nombre : "Sin evento",
                Estado = t.Estado,
                Cupo = t.CupoEquipos,
                Inscritos = t.Equipos.Count(),
                Pagados = t.Equipos.Count(e => e.Pagado)
            })
            .ToListAsync();

        var eventos = await _context.Eventos
            .AsNoTracking()
            .OrderBy(e => e.Fecha)
            .Select(e => new FilaEvento
            {
                Nombre = e.Nombre,
                CantidadTorneos = e.Torneos.Count(),
                Inscritos = e.Torneos.SelectMany(t => t.Equipos).Count(),
                Pagados = e.Torneos.SelectMany(t => t.Equipos).Count(eq => eq.Pagado)
            })
            .ToListAsync();

        var modelo = new TableroViewModel
        {
            TotalEventos = eventos.Count,
            EventosProximos = await _context.Eventos.CountAsync(e => e.Fecha >= DateTime.Now),
            TotalTorneos = torneos.Count,
            EquiposInscritos = torneos.Sum(t => t.Inscritos),
            EquiposPagados = torneos.Sum(t => t.Pagados),
            CupoTotal = torneos.Sum(t => t.Cupo),
            Torneos = torneos,
            Eventos = eventos
        };

        return View(modelo);
    }
}

// Modelos de datos que usa la vista del tablero.
public class TableroViewModel
{
    public int TotalEventos { get; set; }
    public int EventosProximos { get; set; }
    public int TotalTorneos { get; set; }
    public int EquiposInscritos { get; set; }
    public int EquiposPagados { get; set; }
    public int CupoTotal { get; set; }
    public double OcupacionGlobal => CupoTotal == 0 ? 0 : EquiposPagados * 100.0 / CupoTotal;
    public List<FilaTorneo> Torneos { get; set; } = new();
    public List<FilaEvento> Eventos { get; set; } = new();
}

public class FilaTorneo
{
    public string Juego { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public EstadoTorneo Estado { get; set; }
    public int Cupo { get; set; }
    public int Inscritos { get; set; }
    public int Pagados { get; set; }

    public string Etiqueta => $"{Juego} ({Evento})";
    public double Ocupacion => Cupo == 0 ? 0 : Pagados * 100.0 / Cupo;
    public string EstadoTexto => Estado switch
    {
        EstadoTorneo.Abierto => "Abierto",
        EstadoTorneo.Cerrado => "Cerrado",
        EstadoTorneo.EnJuego => "En juego",
        EstadoTorneo.Finalizado => "Finalizado",
        _ => Estado.ToString()
    };
}

public class FilaEvento
{
    public string Nombre { get; set; } = string.Empty;
    public int CantidadTorneos { get; set; }
    public int Inscritos { get; set; }
    public int Pagados { get; set; }
}