using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Entidades;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Administrador")]
public class EventosController : Controller
{
    private readonly ApplicationDbContext _context;

    public EventosController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Eventos
    public async Task<IActionResult> Index()
    {
        var eventos = await _context.Eventos
            .AsNoTracking()
            .OrderBy(e => e.Fecha)
            .ToListAsync();
        return View(eventos);
    }

    // GET: Eventos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        return View(evento);
    }

    // GET: Eventos/Create
    public IActionResult Create()
    {
        return View(new Evento { Fecha = DateTime.Today.AddDays(7).AddHours(9) });
    }

    // POST: Eventos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nombre,Sede,Fecha,CupoPuestos,PrecioPase,ImagenUrl,Descripcion")] Evento evento)
    {
        if (!ModelState.IsValid) return View(evento);

        evento.FechaCreacion = DateTime.Now;
        _context.Add(evento);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "Evento creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Eventos/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _context.Eventos.FindAsync(id);
        if (evento == null) return NotFound();

        return View(evento);
    }

    // POST: Eventos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id)
    {
        var evento = await _context.Eventos.FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        var actualizado = await TryUpdateModelAsync(evento, "",
            e => e.Nombre,
            e => e.Sede,
            e => e.Fecha,
            e => e.CupoPuestos,
            e => e.PrecioPase,
            e => e.ImagenUrl,
            e => e.Descripcion);

        if (!actualizado) return View(evento);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Eventos.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }

        TempData["Mensaje"] = "Evento actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Eventos/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _context.Eventos
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        return View(evento);
    }

    // POST: Eventos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var evento = await _context.Eventos.FindAsync(id);
        if (evento != null)
        {
            try
            {
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Evento eliminado.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar el evento porque tiene torneos asociados.";
            }
        }
        return RedirectToAction(nameof(Index));
    }
}