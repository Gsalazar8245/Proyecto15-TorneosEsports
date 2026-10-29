using Microsoft.AspNetCore.Mvc;
using TorneosEsports.Web.Models.Api;
using TorneosEsports.Web.Services.Pagos;

namespace TorneosEsports.Web.Controllers.Api;

/// <summary>
/// Web API de la pasarela de pago simulada. Es para programas, no para personas:
/// responde JSON y se documenta con OpenAPI (Swagger en /swagger).
/// </summary>
/// <remarks>
/// Contrato: POST api/pagos cobra; GET api/pagos/{codigo} consulta un pago aprobado.
/// «Un rechazo no es un error»: si el banco dice que no, la respuesta es 200 con Aprobado = false.
/// El 400 queda solo para solicitudes que no cumplen el contrato.
/// </remarks>
[ApiController]
[Route("api/pagos")]
[Produces("application/json")]
public class PagosApiController : ControllerBase
{
    private readonly BancoSimulado _banco;

    /// <summary>Recibe el banco simulado por inyección de dependencias.</summary>
    /// <param name="banco">Banco con las reglas de aprobación.</param>
    public PagosApiController(BancoSimulado banco) => _banco = banco;

    /// <summary>Procesa un cobro con tarjeta.</summary>
    /// <param name="solicitud">Monto, tarjeta de 16 dígitos, titular y referencia opcional.</param>
    /// <returns>La decisión del banco (aprobado o rechazado).</returns>
    /// <response code="200">El banco decidió: revise Aprobado y Motivo.</response>
    /// <response code="400">La solicitud no cumple el contrato (por ejemplo, tarjeta sin 16 dígitos).</response>
    [HttpPost]
    [ProducesResponseType(typeof(RespuestaPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<RespuestaPagoDto> Cobrar([FromBody] SolicitudPagoDto solicitud) =>
        Ok(_banco.Procesar(solicitud));

    /// <summary>Consulta un pago aprobado por su código de autorización.</summary>
    /// <param name="codigo">Código con formato AUT-######.</param>
    /// <returns>Los datos del pago.</returns>
    /// <response code="200">El pago existe.</response>
    /// <response code="404">No hay ningún pago aprobado con ese código.</response>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(RespuestaPagoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<RespuestaPagoDto> Consultar(string codigo)
    {
        var pago = _banco.Buscar(codigo);
        return pago is null ? NotFound() : Ok(pago);
    }
}
