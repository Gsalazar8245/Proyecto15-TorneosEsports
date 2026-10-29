using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TorneosEsports.Web.Models.Entidades;

/// <summary>Estado de un pedido.</summary>
public enum EstadoPedido
{
    /// <summary>Creado, esperando el pago.</summary>
    Pendiente = 0,

    /// <summary>Pago aprobado.</summary>
    Pagado = 1,

    /// <summary>Pago rechazado.</summary>
    Rechazado = 2,
}

/// <summary>
/// Pedido que se genera al confirmar el carrito.
/// </summary>
/// <remarks>Estructura base para la migración inicial; se puede ampliar desde el módulo de pagos.</remarks>
public class Pedido : EntidadBase
{
    /// <summary>Id del usuario que compra.</summary>
    [Required]
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Fecha de la compra.</summary>
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Total del pedido, en colones.</summary>
    public decimal Total { get; set; }

    /// <summary>Estado del pedido.</summary>
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pendiente;

    /// <summary>Productos comprados.</summary>
    public ICollection<LineaPedido> Lineas { get; set; } = new List<LineaPedido>();

    /// <summary>Pago asociado, si ya se intentó pagar.</summary>
    public Pago? Pago { get; set; }

    /// <summary>
    /// Crea un pedido pendiente a partir de los ítems del carrito y calcula su total en el modelo.
    /// </summary>
    /// <param name="usuarioId">Usuario que compra.</param>
    /// <param name="items">Ítems del carrito.</param>
    /// <returns>Pedido en estado Pendiente, con una línea por ítem.</returns>
    /// <exception cref="InvalidOperationException">Si el carrito está vacío.</exception>
    public static Pedido DesdeCarrito(string usuarioId, IEnumerable<ItemCarrito> items)
    {
        var pedido = new Pedido { UsuarioId = usuarioId };
        foreach (var item in items)
        {
            pedido.Lineas.Add(new LineaPedido
            {
                Tipo = item.Tipo,
                ReferenciaId = item.ReferenciaId,
                Descripcion = item.Descripcion,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            });
        }

        if (pedido.Lineas.Count == 0)
            throw new InvalidOperationException("No se puede crear un pedido con el carrito vacío.");

        pedido.Total = pedido.Lineas.Sum(l => l.Subtotal);
        return pedido;
    }

    /// <summary>Guarda el resultado de la pasarela y cambia el estado del pedido.</summary>
    /// <param name="aprobado">Decisión del banco.</param>
    /// <param name="codigoAutorizacion">Código AUT-###### si se aprobó.</param>
    /// <param name="mensaje">Motivo que devolvió la pasarela.</param>
    /// <exception cref="InvalidOperationException">Si el pedido ya tenía un pago registrado.</exception>
    public void RegistrarPago(bool aprobado, string? codigoAutorizacion, string? mensaje)
    {
        if (Pago != null)
            throw new InvalidOperationException("Este pedido ya tiene un pago registrado.");

        Pago = new Pago
        {
            Aprobado = aprobado,
            Referencia = codigoAutorizacion ?? string.Empty,
            Mensaje = mensaje,
            Fecha = DateTime.UtcNow
        };
        Estado = aprobado ? EstadoPedido.Pagado : EstadoPedido.Rechazado;
    }
}

/// <summary>
/// Producto comprado dentro de un pedido.
/// </summary>
/// <remarks>Estructura base; se puede ampliar desde el módulo de pagos.</remarks>
public class LineaPedido : EntidadBase
{
    /// <summary>Llave foránea del pedido.</summary>
    public int PedidoId { get; set; }

    /// <summary>Pedido al que pertenece.</summary>
    public Pedido? Pedido { get; set; }

    /// <summary>Tipo de producto.</summary>
    public TipoItem Tipo { get; set; }

    /// <summary>Id del evento (pase) o del equipo (inscripción).</summary>
    public int ReferenciaId { get; set; }

    /// <summary>Texto del producto.</summary>
    [Required, StringLength(150)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Cantidad comprada.</summary>
    public int Cantidad { get; set; }

    /// <summary>Precio por unidad, en colones.</summary>
    public decimal PrecioUnitario { get; set; }

    /// <summary>Cantidad × precio; se calcula, no se guarda en la base de datos.</summary>
    [NotMapped]
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

/// <summary>
/// Resultado del cobro de un pedido.
/// </summary>
/// <remarks>Estructura base; se puede ampliar desde el módulo de pagos.</remarks>
public class Pago : EntidadBase
{
    /// <summary>Llave foránea del pedido (uno a uno).</summary>
    public int PedidoId { get; set; }

    /// <summary>Pedido pagado.</summary>
    public Pedido? Pedido { get; set; }

    /// <summary>Verdadero si la pasarela aprobó el cobro.</summary>
    public bool Aprobado { get; set; }

    /// <summary>Referencia que devuelve la pasarela.</summary>
    [StringLength(80)]
    public string Referencia { get; set; } = string.Empty;

    /// <summary>Fecha del intento de pago.</summary>
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    /// <summary>Mensaje de la pasarela, por ejemplo el motivo del rechazo.</summary>
    [StringLength(200)]
    public string? Mensaje { get; set; }
}
