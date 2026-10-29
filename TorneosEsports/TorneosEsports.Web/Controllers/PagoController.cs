using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Models.Api;
using TorneosEsports.Web.Models.Entidades;
using TorneosEsports.Web.Models.ViewModels;
using TorneosEsports.Web.Services.Correo;
using TorneosEsports.Web.Services.Pagos;

namespace TorneosEsports.Web.Controllers;

/// <summary>
/// Checkout: convierte el carrito en un pedido, lo cobra con la pasarela y envía la confirmación con los QR.
/// </summary>
/// <remarks>
/// El controlador solo coordina: el total lo calcula <see cref="Pedido.DesdeCarrito"/>, la decisión la toma la
/// pasarela detrás de <see cref="IPasarelaPago"/> y el correo lo arma <see cref="PlantillaConfirmacionCompra"/>.
/// </remarks>
[Authorize]
public class PagoController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IPasarelaPago _pasarela;
    private readonly IServicioCorreo _correo;
    private readonly ILogger<PagoController> _logger;

    /// <summary>Recibe sus dependencias por inyección (nunca new).</summary>
    public PagoController(ApplicationDbContext db, IPasarelaPago pasarela, IServicioCorreo correo, ILogger<PagoController> logger)
    {
        _db = db;
        _pasarela = pasarela;
        _correo = correo;
        _logger = logger;
    }

    private string UsuarioActual => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>Muestra el resumen del carrito y el formulario de la tarjeta: /Pago/Checkout</summary>
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var items = await ItemsDelCarrito();
        if (items.Count == 0)
        {
            TempData["Error"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }
        return View(new CheckoutViewModel { Items = items });
    }

    /// <summary>
    /// Crea el pedido, lo cobra y guarda el resultado. Si se aprueba: marca los equipos como pagados,
    /// vacía el carrito, registra la bitácora y envía el correo. Si se rechaza, el carrito queda igual.
    /// </summary>
    /// <param name="vm">Titular y número de tarjeta.</param>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel vm)
    {
        var items = await ItemsDelCarrito();
        vm.Items = items;

        if (items.Count == 0)
        {
            TempData["Error"] = "Tu carrito está vacío.";
            return RedirectToAction("Index", "Carrito");
        }
        if (!ModelState.IsValid) return View(vm);

        // 1. Pedido pendiente (el total lo calcula el modelo).
        var pedido = Pedido.DesdeCarrito(UsuarioActual, items);
        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();

        // 2. Cobro por la Web API. La pasarela nunca lanza excepción: si falla, responde un rechazo.
        var respuesta = await _pasarela.CobrarAsync(new SolicitudPagoDto
        {
            Monto = pedido.Total,
            NumeroTarjeta = vm.NumeroTarjeta,
            Titular = vm.Titular,
            Referencia = $"PED-{pedido.Id}"
        });

        pedido.RegistrarPago(respuesta.Aprobado, respuesta.CodigoAutorizacion, respuesta.Motivo);

        // 3. Si se aprobó: los equipos quedan pagados (y reciben su código de comprobante) y se vacía el carrito.
        var equipos = new List<Equipo>();
        if (respuesta.Aprobado)
        {
            var idsEquipos = pedido.Lineas
                .Where(l => l.Tipo == TipoItem.Inscripcion)
                .Select(l => l.ReferenciaId)
                .ToList();

            equipos = await _db.Equipos
                .Include(e => e.Torneo)
                .Where(e => idsEquipos.Contains(e.Id))
                .ToListAsync();

            foreach (var equipo in equipos)
                equipo.MarcarComoPagado();

            _db.ItemsCarrito.RemoveRange(items);
        }

        // 4. Bitácora de la compra (solo los últimos 4 dígitos, nunca la tarjeta completa).
        var correoUsuario = (await _db.Users.FindAsync(UsuarioActual))?.Email ?? string.Empty;
        _db.BitacoraAcciones.Add(new BitacoraAccion
        {
            UsuarioId = UsuarioActual,
            CorreoUsuario = correoUsuario,
            Accion = respuesta.Aprobado ? "Pago aprobado" : "Pago rechazado",
            Detalle = $"Pedido #{pedido.Id} por ₡{pedido.Total:N0}, tarjeta ****{respuesta.UltimosDigitos}. " +
                      (respuesta.Aprobado ? $"Autorización {respuesta.CodigoAutorizacion}." : $"Motivo: {respuesta.Motivo}."),
            FechaHora = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // 5. Correo de confirmación con los QR. Si el correo falla, la compra sigue siendo válida.
        if (respuesta.Aprobado)
            await EnviarConfirmacion(pedido, respuesta.CodigoAutorizacion!, equipos, correoUsuario);

        return RedirectToAction(nameof(Resultado), new { id = pedido.Id });
    }

    /// <summary>Muestra si el pago se aprobó o se rechazó: /Pago/Resultado/5</summary>
    /// <param name="id">Id del pedido.</param>
    [HttpGet]
    public async Task<IActionResult> Resultado(int id)
    {
        var pedido = await _db.Pedidos
            .AsNoTracking()
            .Include(p => p.Lineas)
            .Include(p => p.Pago)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == UsuarioActual);
        if (pedido == null) return NotFound();

        var idsEquipos = pedido.Lineas
            .Where(l => l.Tipo == TipoItem.Inscripcion)
            .Select(l => l.ReferenciaId)
            .ToList();

        var equipos = pedido.Estado == EstadoPedido.Pagado
            ? await _db.Equipos.AsNoTracking().Where(e => idsEquipos.Contains(e.Id) && e.Pagado).ToListAsync()
            : new List<Equipo>();

        return View(new ResultadoPagoViewModel { Pedido = pedido, EquiposPagados = equipos });
    }

    private Task<List<ItemCarrito>> ItemsDelCarrito() =>
        _db.ItemsCarrito.Where(i => i.UsuarioId == UsuarioActual).ToListAsync();

    private async Task EnviarConfirmacion(Pedido pedido, string codigoAutorizacion, List<Equipo> equipos, string correoUsuario)
    {
        if (string.IsNullOrWhiteSpace(correoUsuario)) return;

        var equiposConQr = equipos
            .Select(e => new EquipoConQr(
                e,
                e.Torneo?.Juego ?? string.Empty,
                Url.Action("Validar", "Comprobante", new { codigo = e.CodigoComprobante }, Request.Scheme)!))
            .ToList();

        var (asunto, html, imagenes) = PlantillaConfirmacionCompra.Crear(pedido, codigoAutorizacion, equiposConQr);

        try
        {
            await _correo.EnviarAsync(correoUsuario, asunto, html, imagenes);
            _db.BitacoraAcciones.Add(new BitacoraAccion
            {
                UsuarioId = UsuarioActual,
                CorreoUsuario = correoUsuario,
                Accion = "Correo de confirmación",
                Detalle = $"Pedido #{pedido.Id}: correo enviado con {equiposConQr.Count} QR.",
                FechaHora = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el correo del pedido {PedidoId}.", pedido.Id);
            TempData["Error"] = "El pago se aprobó, pero no se pudo enviar el correo. Tu comprobante está disponible abajo.";
            _db.BitacoraAcciones.Add(new BitacoraAccion
            {
                UsuarioId = UsuarioActual,
                CorreoUsuario = correoUsuario,
                Accion = "Correo no enviado",
                Detalle = $"Pedido #{pedido.Id}: {ex.Message}",
                FechaHora = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();
    }
}
