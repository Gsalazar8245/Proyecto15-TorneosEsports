namespace TorneosEsports.Web.Services;

/// <summary>
/// Respuesta de un servicio: si la operación se pudo hacer y el mensaje para el usuario.
/// </summary>
/// <remarks>
/// Los servicios devuelven esto en lugar de lanzar excepciones por
/// reglas de negocio (por ejemplo «el torneo ya tiene equipos»), así el controlador solo
/// decide qué mostrar.
/// </remarks>
/// <param name="Exito">Verdadero si la operación se completó.</param>
/// <param name="Mensaje">Texto que se le muestra al usuario.</param>
public sealed record ResultadoOperacion(bool Exito, string Mensaje)
{
    /// <summary>Crea un resultado exitoso.</summary>
    /// <param name="mensaje">Mensaje de confirmación.</param>
    /// <returns>Resultado con Exito = true.</returns>
    public static ResultadoOperacion Ok(string mensaje) => new(true, mensaje);

    /// <summary>Crea un resultado fallido.</summary>
    /// <param name="mensaje">Motivo por el que no se pudo.</param>
    /// <returns>Resultado con Exito = false.</returns>
    public static ResultadoOperacion Fallo(string mensaje) => new(false, mensaje);
}
