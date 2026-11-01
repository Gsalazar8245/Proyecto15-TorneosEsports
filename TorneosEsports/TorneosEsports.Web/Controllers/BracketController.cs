using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Hubs;
using TorneosEsports.Web.Models.ViewModels;

namespace TorneosEsports.Web.Controllers;

public class BracketController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IBracketNotificador _notificador;

    public BracketController(ApplicationDbContext db, IBracketNotificador notificador)
    {
        _db = db;
        _notificador = notificador;
    }

    // Página completa: /Bracket/Ver/1
    [HttpGet]
    public async Task<IActionResult> Ver(int id)
    {
        var vm = await Cargar(id);
        return vm == null ? NotFound() : View(vm);
    }

    // Solo el bracket (lo pide el navegador cuando llega un aviso en vivo)
    [HttpGet]
    public async Task<IActionResult> Llaves(int id)
    {
        var vm = await Cargar(id);
        return vm == null ? NotFound() : PartialView("_Bracket", vm);
    }

    // Prueba para la demo: avisa a todos que el bracket cambió
    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Notificar(int id)
    {
        await _notificador.NotificarAsync(id);
        return RedirectToAction(nameof(Ver), new { id });
    }

    private async Task<BracketViewModel?> Cargar(int torneoId)
    {
        var torneo = await _db.Torneos.FindAsync(torneoId);
        if (torneo == null) return null;

        var partidos = await _db.Partidos
            .Include(p => p.EquipoA)
            .Include(p => p.EquipoB)
            .Include(p => p.Ganador)
            .Where(p => p.TorneoId == torneoId)
            .OrderBy(p => p.Ronda).ThenBy(p => p.Posicion)
            .ToListAsync();

        return new BracketViewModel { TorneoId = torneoId, Juego = torneo.Juego, Partidos = partidos };
    }
}