using System.Net;
using System.Net.Http.Json;
using TorneosEsports.Web.Models.Api;

namespace TorneosEsports.Web.Services.Pagos;

/// <summary>
/// Implementación de <see cref="IPasarelaPago"/> que consume la Web API de pagos por HTTP (JSON).
/// </summary>
/// <remarks>
/// El HttpClient lo fabrica AddHttpClient (ver ModuloPagosExtensions): la dirección base sale de
/// appsettings.json (Pasarela:UrlBase) y el tiempo límite es de 5 segundos. Si la pasarela no contesta,
/// se devuelve un rechazo estructurado: la aplicación no se cae porque un servicio externo falló.
/// </remarks>
public class PasarelaPagoApi : IPasarelaPago
{
    private readonly HttpClient _http;
    private readonly ILogger<PasarelaPagoApi> _logger;

    /// <summary>Recibe el cliente HTTP configurado y el registro de eventos.</summary>
    public PasarelaPagoApi(HttpClient http, ILogger<PasarelaPagoApi> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<RespuestaPagoDto> CobrarAsync(SolicitudPagoDto solicitud, CancellationToken ct = default)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/pagos", solicitud, ct);

            if (respuesta.StatusCode == HttpStatusCode.BadRequest)
                return Rechazo(solicitud, "Los datos de la tarjeta no son válidos.");

            respuesta.EnsureSuccessStatusCode();

            return await respuesta.Content.ReadFromJsonAsync<RespuestaPagoDto>(ct)
                   ?? Rechazo(solicitud, "La pasarela no envió una respuesta.");
        }
        catch (HttpRequestException ex)
        {
            // Nunca se escribe el número de tarjeta en la bitácora del servidor.
            _logger.LogError(ex, "No se pudo contactar la pasarela de pago (referencia {Referencia}).", solicitud.Referencia);
            return Rechazo(solicitud, "No se pudo contactar la pasarela de pago. Intentá de nuevo.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("La pasarela de pago no respondió a tiempo (referencia {Referencia}).", solicitud.Referencia);
            return Rechazo(solicitud, "La pasarela de pago tardó demasiado en responder.");
        }
    }

    private static RespuestaPagoDto Rechazo(SolicitudPagoDto solicitud, string motivo) => new()
    {
        Aprobado = false,
        Motivo = motivo,
        Monto = solicitud.Monto,
        UltimosDigitos = solicitud.NumeroTarjeta.Length >= 4 ? solicitud.NumeroTarjeta[^4..] : string.Empty,
        Referencia = solicitud.Referencia,
        Fecha = DateTime.UtcNow
    };
}
