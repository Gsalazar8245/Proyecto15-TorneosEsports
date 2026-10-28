using TorneosEsports.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TorneosEsports.Web.Controllers;

/// <summary>
/// Llaves (bracket) de un torneo: verlas y registrar resultados.
/// </summary>
/// <remarks>
/// Usa rutas de atributo con restricciones de tipo. La acción
/// <see cref="Parcial"/> devuelve solo el bracket para que el tiempo real (SignalR)
/// lo recargue cuando haya un cambio, sin recargar toda la página.
/// </remarks>
/// <param name="llaves">Servicio de llaves.</param>
public class LlavesController(IServicioLlaves llaves) : Controller
{
    /// <summary>Página del bracket. Ruta: /torneos/{torneoId}/llaves</summary>
    /// <param name="torneoId">Id del torneo.</param>
    /// <returns>Vista del bracket, o 404.</returns>
    [HttpGet("torneos/{torneoId:int}/llaves")]
    [AllowAnonymous]
    public async Task<IActionResult> Ver(int torneoId)
    {
        var bracket = await llaves.ObtenerBracketAsync(torneoId);
        return bracket is null ? NotFound() : View(bracket);
    }

    /// <summary>Solo el bracket (vista parcial), para refrescarlo sin recargar la página.</summary>
    /// <param name="torneoId">Id del torneo.</param>
    /// <returns>Vista parcial _Bracket, o 404.</returns>
    [HttpGet("torneos/{torneoId:int}/llaves/parcial")]
    [AllowAnonymous]
    public async Task<IActionResult> Parcial(int torneoId)
    {
        var bracket = await llaves.ObtenerBracketAsync(torneoId);
        return bracket is null ? NotFound() : PartialView("_Bracket", bracket);
    }

    /// <summary>Registra el ganador de un partido.</summary>
    /// <param name="partidoId">Id del partido (de la ruta).</param>
    /// <param name="ganadorId">Id del equipo ganador (del formulario).</param>
    /// <param name="torneoId">Id del torneo, para volver al bracket.</param>
    /// <returns>Redirección al bracket con un mensaje.</returns>
    [HttpPost("partidos/{partidoId:int}/resultado")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = TorneosController.RolAdministrador)]
    public async Task<IActionResult> RegistrarResultado(int partidoId, int ganadorId, int torneoId)
    {
        var resultado = await llaves.RegistrarResultadoAsync(partidoId, ganadorId);
        TempData[resultado.Exito ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Ver), new { torneoId });
    }
}
