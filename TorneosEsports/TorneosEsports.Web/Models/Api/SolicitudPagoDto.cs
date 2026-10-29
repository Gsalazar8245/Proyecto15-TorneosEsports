using System.ComponentModel.DataAnnotations;

namespace TorneosEsports.Web.Models.Api;

/// <summary>
/// Datos que el comercio envía a la pasarela para cobrar (contrato de entrada del POST api/pagos).
/// </summary>
/// <remarks>
/// Las anotaciones forman parte del contrato: si algo no se cumple, [ApiController]
/// responde 400 antes de entrar al método y la lógica de negocio nunca se ejecuta.
/// </remarks>
public class SolicitudPagoDto
{
    /// <summary>Monto a cobrar, en colones.</summary>
    [Range(1, 1_000_000, ErrorMessage = "El monto debe estar entre 1 y 1 000 000.")]
    public decimal Monto { get; set; }

    /// <summary>Número de la tarjeta: exactamente 16 dígitos. Nunca se guarda ni se devuelve completo.</summary>
    [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "La tarjeta debe tener 16 dígitos.")]
    public string NumeroTarjeta { get; set; } = string.Empty;

    /// <summary>Nombre del titular de la tarjeta.</summary>
    [Required(ErrorMessage = "El titular es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El titular debe tener entre 3 y 60 caracteres.")]
    public string Titular { get; set; } = string.Empty;

    /// <summary>Referencia opcional del comercio, por ejemplo el número de pedido.</summary>
    [StringLength(40)]
    public string? Referencia { get; set; }
}
