using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;

[AllowAnonymous]
public class CatalogoController : Controller
{
    private readonly ApplicationDbContext _context;

    public CatalogoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Catalogo
    public async Task<IActionResult> Index(string? q)
    {
        var consulta = _context.Eventos
            .AsNoTracking()
            .Include(e => e.Torneos)
                .ThenInclude(t => t.Equipos)
            .Where(e => e.Fecha >= DateTime.Now);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim();
            consulta = consulta.Where(e => e.Nombre.Contains(texto) || e.Sede.Contains(texto));
        }

        ViewData["Busqueda"] = q;
        var eventos = await consulta.OrderBy(e => e.Fecha).ToListAsync();
        return View(eventos);
    }

    // GET: /Catalogo/Detalle/5
    public async Task<IActionResult> Detalle(int? id)
    {
        if (id == null) return NotFound();

        var evento = await _context.Eventos
            .AsNoTracking()
            .Include(e => e.Torneos)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (evento == null) return NotFound();

        return View(evento);
    }
}