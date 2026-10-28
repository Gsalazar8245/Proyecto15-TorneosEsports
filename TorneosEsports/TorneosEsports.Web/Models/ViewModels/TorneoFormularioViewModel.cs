using System.ComponentModel.DataAnnotations;
using TorneosEsports.Web.Models.Entidades;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TorneosEsports.Web.Models.ViewModels;

/// <summary>
/// Datos del formulario para crear o editar un torneo, con sus validaciones.
/// </summary>
/// <remarks>
/// Se usa un ViewModel en lugar de la entidad para que el formulario
/// solo pueda enviar estos campos (por ejemplo, nadie puede cambiar el Estado desde el navegador).
/// La vista dibuja las validaciones a partir de estas DataAnnotations.
/// </remarks>
public class TorneoFormularioViewModel
{
    /// <summary>Id del torneo (0 al crear).</summary>
    public int Id { get; set; }

    /// <summary>Evento al que pertenece el torneo.</summary>
    [Display(Name = "Evento")]
    [Required(ErrorMessage = "Seleccione un evento.")]
    public int? EventoId { get; set; }

    /// <summary>Juego del torneo.</summary>
    [Required(ErrorMessage = "Escriba el juego.")]
    [StringLength(80, ErrorMessage = "Máximo {1} caracteres.")]
    public string Juego { get; set; } = string.Empty;

    /// <summary>Formato del torneo.</summary>
    [Required(ErrorMessage = "Escriba el formato.")]
    [StringLength(80, ErrorMessage = "Máximo {1} caracteres.")]
    public string Formato { get; set; } = "Eliminación directa 5v5";

    /// <summary>Máximo de equipos.</summary>
    [Display(Name = "Cupo de equipos")]
    [Required(ErrorMessage = "Indique el cupo de equipos.")]
    [Range(2, 64, ErrorMessage = "El cupo debe estar entre {1} y {2} equipos.")]
    public int CupoEquipos { get; set; } = 8;

    /// <summary>Precio de inscripción por equipo, en colones.</summary>
    [Display(Name = "Precio de inscripción por equipo (₡)")]
    [Required(ErrorMessage = "Indique el precio de inscripción.")]
    [Range(typeof(decimal), "0", "10000000", ErrorMessage = "El precio debe estar entre {1} y {2}.")]
    public decimal PrecioInscripcion { get; set; }

    /// <summary>Fecha y hora de cierre de inscripciones.</summary>
    [Display(Name = "Cierre de inscripciones")]
    [Required(ErrorMessage = "Indique cuándo cierran las inscripciones.")]
    [DataType(DataType.DateTime)]
    public DateTime? CierreInscripciones { get; set; }

    /// <summary>Opciones del selector de eventos; no viene del formulario.</summary>
    [ValidateNever]
    public IEnumerable<SelectListItem> Eventos { get; set; } = [];

    /// <summary>Crea el ViewModel con los datos de un torneo existente.</summary>
    /// <param name="torneo">Torneo a editar.</param>
    /// <returns>ViewModel listo para la vista.</returns>
    public static TorneoFormularioViewModel DesdeTorneo(Torneo torneo) => new()
    {
        Id = torneo.Id,
        EventoId = torneo.EventoId,
        Juego = torneo.Juego,
        Formato = torneo.Formato,
        CupoEquipos = torneo.CupoEquipos,
        PrecioInscripcion = torneo.PrecioInscripcion,
        CierreInscripciones = torneo.CierreInscripciones,
    };

    /// <summary>Convierte los datos del formulario en una entidad Torneo.</summary>
    /// <returns>Torneo nuevo con los valores del formulario.</returns>
    public Torneo ATorneo() => new()
    {
        EventoId = EventoId ?? 0,
        Juego = Juego.Trim(),
        Formato = Formato.Trim(),
        CupoEquipos = CupoEquipos,
        PrecioInscripcion = PrecioInscripcion,
        CierreInscripciones = CierreInscripciones ?? default,
    };
}
