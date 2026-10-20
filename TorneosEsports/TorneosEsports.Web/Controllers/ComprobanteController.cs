using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.ViewModels;

namespace TorneosEsports.Web.Controllers;

[Authorize]
public class ComprobanteController : Controller
{
    private readonly ApplicationDbContext _db;
    public ComprobanteController(ApplicationDbContext db) => _db = db;

    private string UsuarioActual => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Comprobante del capitán: /Comprobante/Ver/1
    [HttpGet]
    public async Task<IActionResult> Ver(int id)
    {
        var equipo = await _db.Equipos
            .Include(e => e.Integrantes)
            .FirstOrDefaultAsync(e => e.Id == id && e.CapitanId == UsuarioActual);
        if (equipo == null) return NotFound();

        if (!equipo.Pagado || equipo.CodigoComprobante == null)
        {
            TempData["Error"] = "El comprobante está disponible cuando el equipo ha pagado.";
            return RedirectToAction("Index", "Carrito");
        }

        var torneo = await _db.Torneos.FindAsync(equipo.TorneoId);

        // El QR contiene la dirección de la página de validación con el código
        var url = Url.Action("Validar", "Comprobante",
            new { codigo = equipo.CodigoComprobante }, Request.Scheme)!;

        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(datos).GetGraphic(10);

        return View(new ComprobanteViewModel
        {
            Equipo = equipo,
            TorneoJuego = torneo?.Juego ?? "",
            QrBase64 = Convert.ToBase64String(png)
        });
    }

    // Validación en la entrada (solo administrador): /Comprobante/Validar?codigo=...
    [HttpGet]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Validar(string? codigo)
    {
        var vm = new ValidacionViewModel { Codigo = codigo };
        if (string.IsNullOrWhiteSpace(codigo)) return View(vm);

        await CargarEquipo(vm);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Validar(string codigo, bool confirmar)
    {
        var vm = new ValidacionViewModel { Codigo = codigo };
        await CargarEquipo(vm);

        if (vm.Equipo != null && confirmar)
        {
            try
            {
                vm.Equipo.RegistrarIngreso();
                await _db.SaveChangesAsync();
                vm.Mensaje = "Ingreso registrado correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                vm.Mensaje = ex.Message;
                vm.Valido = false;
            }
        }
        return View(vm);
    }

    private async Task CargarEquipo(ValidacionViewModel vm)
    {
        var codigo = vm.Codigo!.Trim();
        vm.Equipo = await _db.Equipos
            .Include(e => e.Integrantes)
            .FirstOrDefaultAsync(e => e.CodigoComprobante == codigo);

        if (vm.Equipo == null)
        {
            vm.Mensaje = "Código no válido.";
            return;
        }

        vm.TorneoJuego = (await _db.Torneos.FindAsync(vm.Equipo.TorneoId))?.Juego;
        vm.Valido = vm.Equipo.Pagado && !vm.Equipo.Validado;
        vm.Mensaje = vm.Equipo.Validado
            ? $"Este comprobante ya fue utilizado ({vm.Equipo.FechaValidacion:dd/MM/yyyy HH:mm})."
            : vm.Equipo.Pagado ? "Comprobante válido." : "El equipo no ha pagado.";
    }
}