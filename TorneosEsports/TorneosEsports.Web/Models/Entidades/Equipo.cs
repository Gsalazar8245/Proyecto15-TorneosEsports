using System.ComponentModel.DataAnnotations;
using TorneosEsports.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace TorneosEsports.Web.Models.Entidades;

/// <summary>
/// Equipo inscrito en un torneo. Lo crea el capitán, que paga por todo el equipo.
/// </summary>
/// <remarks>
/// Las llaves se arman solo con los equipos que tienen <see cref="Pagado"/> en verdadero.
/// Nota para Deilyn: al aprobarse el pago de la inscripción hay que llamar a <see cref="MarcarComoPagado"/>,
/// que pone Pagado = true y genera el código del comprobante.
/// </remarks>
[Index(nameof(CodigoComprobante), IsUnique = true)]
public class Equipo : EntidadBase
{
    /// <summary>Cantidad de integrantes que exige la regla del proyecto.</summary>
    public const int IntegrantesRequeridos = 5;

    /// <summary>Llave foránea del torneo.</summary>
    public int TorneoId { get; set; }

    /// <summary>Torneo en el que está inscrito.</summary>
    public Torneo? Torneo { get; set; }

    /// <summary>Nombre del equipo; único dentro del torneo.</summary>
    [Required(ErrorMessage = "El nombre del equipo es obligatorio.")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Id del usuario capitán (Identity).</summary>
    [Required]
    public string CapitanId { get; set; } = string.Empty;

    /// <summary>Usuario capitán (navegación).</summary>
    public ApplicationUser? Capitan { get; set; }

    /// <summary>Verdadero cuando el pago de la inscripción fue aprobado.</summary>
    public bool Pagado { get; set; }

    /// <summary>Los cinco integrantes registrados por el capitán.</summary>
    public ICollection<IntegranteEquipo> Integrantes { get; set; } = new List<IntegranteEquipo>();

    /// <summary>Código único del comprobante; se genera cuando el equipo paga.</summary>
    public string? CodigoComprobante { get; set; }

    /// <summary>Verdadero cuando el equipo ya ingresó al evento (el QR se usó).</summary>
    public bool Validado { get; set; }

    public DateTime? FechaValidacion { get; set; }    /// <summary>Fecha y hora en que se registró el ingreso con el QR.</summary>

    /// <summary>Marca el equipo como pagado y le genera su código de comprobante.</summary>
    public void MarcarComoPagado()
    {
        Pagado = true;
        CodigoComprobante ??= Guid.NewGuid().ToString("N");
    }

    /// <summary>Registra el ingreso del equipo con su QR.</summary>
    /// <exception cref="InvalidOperationException">Si no está pagado o el QR ya se usó.</exception>
    public void RegistrarIngreso()
    {
        if (!Pagado)
            throw new InvalidOperationException("El equipo no ha pagado su inscripción.");
        if (Validado)
            throw new InvalidOperationException("Este comprobante ya fue utilizado.");
        Validado = true;
        FechaValidacion = DateTime.Now;
    }
}
