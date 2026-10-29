namespace TorneosEsports.Web.Models.Api;

/// <summary>
/// Respuesta de la pasarela (contrato de salida). Un rechazo también viaja aquí, con código 200:
/// la API funcionó; fue el banco el que dijo que no.
/// </summary>
public class RespuestaPagoDto
{
    /// <summary>Verdadero si el banco aprobó el cobro.</summary>
    public bool Aprobado { get; set; }

    /// <summary>Código de autorización con formato AUT-######; vacío si se rechazó.</summary>
    public string? CodigoAutorizacion { get; set; }

    /// <summary>Motivo de la decisión, por ejemplo «Aprobado» o «Fondos insuficientes».</summary>
    public string Motivo { get; set; } = string.Empty;

    /// <summary>Monto que se intentó cobrar, en colones.</summary>
    public decimal Monto { get; set; }

    /// <summary>Solo los últimos 4 dígitos de la tarjeta; el número completo nunca sale de la API.</summary>
    public string UltimosDigitos { get; set; } = string.Empty;

    /// <summary>Referencia que envió el comercio.</summary>
    public string? Referencia { get; set; }

    /// <summary>Fecha y hora (UTC) de la decisión.</summary>
    public DateTime Fecha { get; set; }
}
