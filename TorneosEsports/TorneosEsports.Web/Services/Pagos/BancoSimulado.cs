using System.Collections.Concurrent;
using TorneosEsports.Web.Models.Api;

namespace TorneosEsports.Web.Services.Pagos;

/// <summary>
/// Banco de prueba con reglas fijas y predecibles. Aquí vive la decisión de negocio del cobro,
/// no en el controlador (el controlador no calcula).
/// </summary>
/// <remarks>
/// Se registra como singleton. Los pagos aprobados se guardan en memoria en un
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> porque la API atiende varias peticiones a la vez;
/// al reiniciar la aplicación ese registro se pierde (es una pasarela de desarrollo, no de producción).
/// </remarks>
public class BancoSimulado
{
    /// <summary>Monto máximo que aprueba el banco simulado, en colones.</summary>
    public const decimal LimiteMonto = 250_000m;

    private readonly ConcurrentDictionary<string, RespuestaPagoDto> _aprobados = new();

    /// <summary>
    /// Aplica las tres reglas en orden; la primera que se cumple decide:
    /// 1) tarjeta que termina en 0000 → rechazo; 2) monto mayor a 250 000 → rechazo; 3) si no, se aprueba.
    /// </summary>
    /// <param name="solicitud">Solicitud ya validada por el contrato.</param>
    /// <returns>La decisión del banco, con solo los últimos 4 dígitos de la tarjeta.</returns>
    public RespuestaPagoDto Procesar(SolicitudPagoDto solicitud)
    {
        var respuesta = new RespuestaPagoDto
        {
            Monto = solicitud.Monto,
            UltimosDigitos = solicitud.NumeroTarjeta[^4..],
            Referencia = solicitud.Referencia,
            Fecha = DateTime.UtcNow
        };

        if (respuesta.UltimosDigitos == "0000")
        {
            respuesta.Motivo = "Fondos insuficientes";
            return respuesta;
        }

        if (solicitud.Monto > LimiteMonto)
        {
            respuesta.Motivo = $"El monto supera el límite de ₡{LimiteMonto:N0}";
            return respuesta;
        }

        respuesta.Aprobado = true;
        respuesta.Motivo = "Aprobado";

        // Se genera un código hasta que no choque con uno existente.
        do
        {
            respuesta.CodigoAutorizacion = $"AUT-{Random.Shared.Next(0, 1_000_000):D6}";
        }
        while (!_aprobados.TryAdd(respuesta.CodigoAutorizacion, respuesta));

        return respuesta;
    }

    /// <summary>Busca un pago aprobado por su código de autorización.</summary>
    /// <param name="codigo">Código con formato AUT-######.</param>
    /// <returns>El pago, o null si no existe.</returns>
    public RespuestaPagoDto? Buscar(string codigo) =>
        _aprobados.TryGetValue(codigo.Trim().ToUpperInvariant(), out var pago) ? pago : null;
}
