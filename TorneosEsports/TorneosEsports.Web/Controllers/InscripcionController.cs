using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Models.ViewModels;

namespace TorneosEsports.Web.Controllers;

[Authorize]
public class InscripcionController : Controller
{
    private readonly ApplicationDbContext _db;
    public InscripcionController(ApplicationDbContext db) => _db = db;

    // GET: /Inscripcion/Crear?torneoId=3
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var torneos = await _db.Torneos
            .Include(t => t.Equipos)
            .Where(t => t.Estado == EstadoTorneo.Abierto)
            .OrderBy(t => t.CierreInscripciones)
            .ToListAsync();

        return View(torneos);
    }
    public async Task<IActionResult> Crear(int torneoId)
    {
        var torneo = await _db.Torneos.FindAsync(torneoId);
        if (torneo == null) return NotFound();

        var error = await ValidarTorneo(torneo);
        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction("Index", "Carrito"); // o el catálogo de Erlan
        }

        return View(new InscripcionEquipoViewModel
        {
            TorneoId = torneo.Id,
            TorneoNombre = torneo.Juego,
            PrecioInscripcion = torneo.PrecioInscripcion
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(InscripcionEquipoViewModel vm)
    {
        var torneo = await _db.Torneos.FindAsync(vm.TorneoId);
        if (torneo == null) return NotFound();

        vm.TorneoNombre = torneo.Juego;
        vm.PrecioInscripcion = torneo.PrecioInscripcion;

        // Regla: exactamente 5 integrantes
        if (vm.Integrantes.Count != Equipo.IntegrantesRequeridos)
            ModelState.AddModelError("", $"El equipo debe tener exactamente {Equipo.IntegrantesRequeridos} integrantes.");

        // Regla: nicks y correos no repetidos dentro del equipo
        if (vm.Integrantes.Select(i => i.Nick.Trim().ToLower()).Distinct().Count() != vm.Integrantes.Count)
            ModelState.AddModelError("", "Hay nicks repetidos en el equipo.");
        if (vm.Integrantes.Select(i => i.Correo.Trim().ToLower()).Distinct().Count() != vm.Integrantes.Count)
            ModelState.AddModelError("", "Hay correos repetidos en el equipo.");

        // Reglas del torneo: abierto, fecha de cierre y cupo
        var error = await ValidarTorneo(torneo);
        if (error != null) ModelState.AddModelError("", error);

        // Regla: nombre único dentro del torneo
        var nombre = vm.NombreEquipo.Trim();
        if (await _db.Equipos.AnyAsync(e => e.TorneoId == torneo.Id && e.Nombre == nombre))
            ModelState.AddModelError(nameof(vm.NombreEquipo), "Ya existe un equipo con ese nombre en este torneo.");

        if (!ModelState.IsValid) return View(vm);

        var capitanId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var equipo = new Equipo
            {
                TorneoId = torneo.Id,
                Nombre = nombre,
                CapitanId = capitanId,
                Pagado = false,
                Integrantes = vm.Integrantes.Select(i => new IntegranteEquipo
                {
                    Nombre = i.Nombre.Trim(),
                    Nick = i.Nick.Trim(),
                    Correo = i.Correo.Trim()
                }).ToList()
            };
            _db.Equipos.Add(equipo);
            await _db.SaveChangesAsync(); // aquí se genera equipo.Id

            _db.ItemsCarrito.Add(new ItemCarrito
            {
                UsuarioId = capitanId,
                Tipo = TipoItem.Inscripcion,
                ReferenciaId = equipo.Id,
                Descripcion = $"Inscripción: {equipo.Nombre} - {torneo.Juego}",
                Cantidad = 1,
                PrecioUnitario = torneo.PrecioInscripcion
            });
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", "No se pudo guardar la inscripción. Intentá de nuevo.");
            return View(vm);
        }

        TempData["Exito"] = "Equipo inscrito. Está en tu carrito pendiente de pago.";
        return RedirectToAction("Index", "Home"); // se crea en ST2
    }

    private async Task<string?> ValidarTorneo(Torneo torneo)
    {
        if (!torneo.AceptaInscripciones(DateTime.Now))
            return "Este torneo no acepta inscripciones.";

        var equiposInscritos = await _db.Equipos.CountAsync(e => e.TorneoId == torneo.Id);
        if (torneo.CuposDisponibles(equiposInscritos) <= 0)
            return "El torneo ya no tiene cupos disponibles.";

        return null;
    }
}