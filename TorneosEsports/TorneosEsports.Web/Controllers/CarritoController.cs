using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Models.ViewModels;

namespace TorneosEsports.Web.Controllers;

[Authorize]
public class CarritoController : Controller
{
    private readonly ApplicationDbContext _db;
    public CarritoController(ApplicationDbContext db) => _db = db;

    private string UsuarioActual => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var items = await _db.ItemsCarrito
            .Where(i => i.UsuarioId == UsuarioActual)
            .ToListAsync();
        return View(new CarritoViewModel { Items = items });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Quitar(int id)
    {
        var item = await _db.ItemsCarrito
            .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == UsuarioActual);
        if (item == null) return NotFound();

        // Si era una inscripción sin pagar, se libera el cupo borrando el equipo
        if (item.Tipo == TipoItem.Inscripcion)
        {
            var equipo = await _db.Equipos
                .FirstOrDefaultAsync(e => e.Id == item.ReferenciaId && !e.Pagado);
            if (equipo != null) _db.Equipos.Remove(equipo);
        }

        _db.ItemsCarrito.Remove(item);
        await _db.SaveChangesAsync();
        TempData["Exito"] = "Ítem quitado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    // Pase LAN: el evento se elige desde el catálogo de Erlan
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarPase(int eventoId, int cantidad = 1)
    {
        var evento = await _db.Eventos.FindAsync(eventoId);
        if (evento == null) return NotFound();
        if (cantidad < 1 || cantidad > 10)
        {
            TempData["Error"] = "La cantidad debe estar entre 1 y 10.";
            return RedirectToAction(nameof(Index));
        }

        var existente = await _db.ItemsCarrito.FirstOrDefaultAsync(i =>
            i.UsuarioId == UsuarioActual && i.Tipo == TipoItem.PaseLan && i.ReferenciaId == eventoId);

        if (existente != null)
            existente.Cantidad += cantidad;
        else
            _db.ItemsCarrito.Add(new ItemCarrito
            {
                UsuarioId = UsuarioActual,
                Tipo = TipoItem.PaseLan,
                ReferenciaId = eventoId,
                Descripcion = $"Pase LAN: {evento.Nombre}",
                Cantidad = cantidad,
                PrecioUnitario = evento.PrecioPase
            });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}