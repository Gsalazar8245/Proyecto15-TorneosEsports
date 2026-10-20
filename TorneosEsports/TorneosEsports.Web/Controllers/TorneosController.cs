using TorneosEsports.Web.Models.ViewModels;
using TorneosEsports.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TorneosEsports.Web.Controllers;

/// <summary>
/// Catálogo 2: lista pública de torneos y su mantenimiento para el administrador.
/// </summary>
/// <remarks>
/// El controlador no calcula ni valida reglas de negocio: recibe la
/// petición, revisa el ModelState, llama al servicio y decide qué vista o redirección
/// devolver. Las páginas de mantenimiento exigen el rol Administrador.
/// </remarks>
/// <param name="torneos">Servicio del catálogo de torneos.</param>
public class TorneosController(IServicioTorneos torneos) : Controller
{
    /// <summary>Nombre del rol de administrador (lo crea IdentitySeeder).</summary>
    public const string RolAdministrador = "Administrador";

    /// <summary>Lista todos los torneos. Ruta: /Torneos</summary>
    /// <returns>Vista con la lista.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Torneos";
        return View(await torneos.ListarAsync());
    }

    /// <summary>
    /// Lista los torneos de un evento. Ruta de atributo con restricción: el id del evento
    /// debe ser un número entero mayor que cero (por ejemplo /eventos/1/torneos).
    /// </summary>
    /// <param name="eventoId">Id del evento.</param>
    /// <returns>La misma vista Index filtrada por evento.</returns>
    [HttpGet("eventos/{eventoId:int:min(1)}/torneos")]
    [AllowAnonymous]
    public async Task<IActionResult> PorEvento(int eventoId)
    {
        var lista = await torneos.ListarAsync(eventoId);
        ViewData["Title"] = lista.Count > 0 ? $"Torneos de {lista[0].Evento?.Nombre}" : "Torneos del evento";
        return View(nameof(Index), lista);
    }

    /// <summary>Muestra el detalle de un torneo con sus equipos inscritos.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>Vista de detalle, o 404 si no existe.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Detalle(int id)
    {
        var torneo = await torneos.ObtenerAsync(id);
        return torneo is null ? NotFound() : View(torneo);
    }

    /// <summary>Formulario para crear un torneo.</summary>
    /// <param name="eventoId">Evento preseleccionado (opcional).</param>
    /// <returns>Vista del formulario.</returns>
    [HttpGet]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> Crear(int? eventoId)
    {
        var modelo = new TorneoFormularioViewModel
        {
            EventoId = eventoId,
            CierreInscripciones = DateTime.Today.AddDays(14).AddHours(23).AddMinutes(59),
        };
        await CargarEventosAsync(modelo);
        return View(modelo);
    }

    /// <summary>Guarda un torneo nuevo.</summary>
    /// <param name="modelo">Datos enviados por el formulario.</param>
    /// <returns>Redirección al detalle, o el formulario con errores.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> Crear(TorneoFormularioViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            await CargarEventosAsync(modelo);
            return View(modelo);
        }

        var torneo = modelo.ATorneo();
        var resultado = await torneos.CrearAsync(torneo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarEventosAsync(modelo);
            return View(modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Detalle), new { id = torneo.Id });
    }

    /// <summary>Formulario para editar un torneo.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>Vista del formulario, o 404.</returns>
    [HttpGet]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> Editar(int id)
    {
        var torneo = await torneos.ObtenerAsync(id);
        if (torneo is null)
            return NotFound();

        if (!torneo.PuedeEditarse)
        {
            TempData["Error"] = "Solo se puede editar un torneo con las inscripciones abiertas.";
            return RedirectToAction(nameof(Detalle), new { id });
        }

        var modelo = TorneoFormularioViewModel.DesdeTorneo(torneo);
        await CargarEventosAsync(modelo);
        return View(modelo);
    }

    /// <summary>Guarda los cambios de un torneo.</summary>
    /// <param name="id">Id del torneo (de la ruta).</param>
    /// <param name="modelo">Datos enviados por el formulario.</param>
    /// <returns>Redirección al detalle, o el formulario con errores.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> Editar(int id, TorneoFormularioViewModel modelo)
    {
        if (id != modelo.Id)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await CargarEventosAsync(modelo);
            return View(modelo);
        }

        var resultado = await torneos.ActualizarAsync(id, modelo.ATorneo());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            await CargarEventosAsync(modelo);
            return View(modelo);
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Detalle), new { id });
    }

    /// <summary>Pide confirmación antes de eliminar.</summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>Vista de confirmación, o 404.</returns>
    [HttpGet]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var torneo = await torneos.ObtenerAsync(id);
        return torneo is null ? NotFound() : View(torneo);
    }

    /// <summary>
    /// Elimina el torneo después de la confirmación. [ActionName] permite que GET y POST se
    /// llamen «Eliminar» aunque en C# los métodos tengan nombres distintos.
    /// </summary>
    /// <param name="id">Id del torneo.</param>
    /// <returns>Redirección a la lista o al detalle.</returns>
    [HttpPost]
    [ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RolAdministrador)]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var resultado = await torneos.EliminarAsync(id);
        if (!resultado.Exito)
        {
            TempData["Error"] = resultado.Mensaje;
            return RedirectToAction(nameof(Detalle), new { id });
        }

        TempData["Mensaje"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Llena el selector de eventos del formulario.</summary>
    /// <param name="modelo">ViewModel del formulario.</param>
    /// <returns>Tarea asíncrona.</returns>
    [NonAction]
    public async Task CargarEventosAsync(TorneoFormularioViewModel modelo)
    {
        var eventos = await torneos.ListarEventosAsync();
        modelo.Eventos = eventos.Select(e => new SelectListItem(
            $"{e.Nombre} ({e.Fecha:dd/MM/yyyy})", e.Id.ToString(), e.Id == modelo.EventoId));
    }
}
