using TorneosEsports.Web.Models.Api;

namespace TorneosEsports.Web.Services.Pagos;

/// <summary>
/// Abstracción de la pasarela de pago. El checkout solo conoce esta interfaz (inversión de dependencias):
/// detrás puede estar la Web API simulada, un proveedor real o una clase de pruebas,
/// y el controlador no cambia.
/// </summary>
public interface IPasarelaPago
{
    /// <summary>Envía un cobro a la pasarela.</summary>
    /// <param name="solicitud">Datos del cobro.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Siempre devuelve una respuesta; si la red falla, devuelve un rechazo ordenado en vez de lanzar una excepción.</returns>
    Task<RespuestaPagoDto> CobrarAsync(SolicitudPagoDto solicitud, CancellationToken ct = default);
}
