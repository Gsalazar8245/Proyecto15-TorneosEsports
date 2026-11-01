using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorneosEsports.Web.Models.Entidades;

/// <summary>Qué se está comprando en una línea del carrito o del pedido.</summary>
public enum TipoItem
{
    /// <summary>Pase de entrada a la LAN de un evento.</summary>
    PaseLan = 0,

    /// <summary>Inscripción de un equipo a un torneo.</summary>
    Inscripcion = 1,
}

/// <summary>
/// Línea del carrito de compras de un usuario.
/// </summary>
/// <remarks>Estructura base para la migración inicial; se puede ampliar desde el módulo de inscripción y carrito.</remarks>
public class ItemCarrito : EntidadBase
{
    /// <summary>Id del usuario dueño del carrito.</summary>
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Tipo de producto.</summary>
    public TipoItem Tipo { get; set; }

    /// <summary>Id del evento (pase) o del equipo (inscripción).</summary>
    public int ReferenciaId { get; set; }

    /// <summary>Texto que se muestra en el carrito.</summary>
    [Required, StringLength(150)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Cantidad; una inscripción siempre es 1.</summary>
    [Range(1, 10)]
    public int Cantidad { get; set; } = 1;

    /// <summary>Precio por unidad, en colones.</summary>
    public decimal PrecioUnitario { get; set; }
    [NotMapped]
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

