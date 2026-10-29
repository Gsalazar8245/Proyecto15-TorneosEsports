using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Models.ViewModels;

/// <summary>Pantalla de pago: resumen del carrito y datos de la tarjeta.</summary>
/// <remarks>Las reglas son las mismas del contrato de la API, para avisar al usuario antes de enviar.</remarks>
public class CheckoutViewModel
{
    /// <summary>Ítems del carrito (solo para mostrar; no se envían en el formulario).</summary>
    [ValidateNever]
    public List<ItemCarrito> Items { get; set; } = new();

    /// <summary>Total a pagar, calculado en el modelo.</summary>
    public decimal Total => Items.Sum(i => i.Subtotal);

    /// <summary>Nombre como aparece en la tarjeta.</summary>
    [Required(ErrorMessage = "El titular es obligatorio.")]
    [StringLength(60, MinimumLength = 3, ErrorMessage = "El titular debe tener entre 3 y 60 caracteres.")]
    [Display(Name = "Titular de la tarjeta")]
    public string Titular { get; set; } = string.Empty;

    /// <summary>Número de tarjeta de 16 dígitos (no se guarda).</summary>
    [Required(ErrorMessage = "El número de tarjeta es obligatorio.")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Escribí los 16 dígitos, sin espacios.")]
    [Display(Name = "Número de tarjeta")]
    public string NumeroTarjeta { get; set; } = string.Empty;
}

/// <summary>Pantalla de resultado del pago (aprobado o rechazado).</summary>
public class ResultadoPagoViewModel
{
    /// <summary>Pedido con sus líneas y su pago.</summary>
    public Pedido Pedido { get; set; } = null!;

    /// <summary>Equipos que quedaron pagados en este pedido, para enlazar a su comprobante.</summary>
    public List<Equipo> EquiposPagados { get; set; } = new();
}
