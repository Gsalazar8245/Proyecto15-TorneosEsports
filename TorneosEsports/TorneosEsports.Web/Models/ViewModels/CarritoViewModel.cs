using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Models.ViewModels;

public class CarritoViewModel
{
    public List<ItemCarrito> Items { get; set; } = new();

    /// <summary>Total del carrito, calculado en el modelo.</summary>
    public decimal Total => Items.Sum(i => i.Subtotal);

    public bool EstaVacio => Items.Count == 0;
}