using System.ComponentModel.DataAnnotations;
using TorneosEsports.Web.Models.Entidades; // ajustá al namespace real

namespace TorneosEsports.Web.Models.ViewModels;

public class IntegranteViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "El nick es obligatorio")]
    [StringLength(40)]
    public string Nick { get; set; } = "";

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Correo no válido")]
    public string Correo { get; set; } = "";
}

public class InscripcionEquipoViewModel
{
    public int TorneoId { get; set; }
    public string? TorneoNombre { get; set; }
    public decimal PrecioInscripcion { get; set; }

    [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
    [StringLength(60)]
    public string NombreEquipo { get; set; } = "";

    // Exactamente 5 filas, tantas como pide Equipo.IntegrantesRequeridos
    public List<IntegranteViewModel> Integrantes { get; set; } =
        Enumerable.Range(0, Equipo.IntegrantesRequeridos)
                  .Select(_ => new IntegranteViewModel()).ToList();
}