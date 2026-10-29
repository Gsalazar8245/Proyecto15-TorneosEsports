namespace TorneosEsports.Web.Services.Correo;

/// <summary>Imagen que va dentro del cuerpo del correo y se referencia con src="cid:IdContenido".</summary>
/// <param name="IdContenido">Identificador que usa el HTML, por ejemplo "qr-3".</param>
/// <param name="Contenido">Bytes de la imagen.</param>
/// <param name="TipoMime">Tipo de la imagen, por ejemplo "image/png".</param>
public record ImagenIncrustada(string IdContenido, byte[] Contenido, string TipoMime = "image/png");

/// <summary>
/// Envío de correos de la aplicación. Quien envía un correo solo conoce esta interfaz,
/// no sabe si sale por SMTP o se guarda en un archivo.
/// </summary>
public interface IServicioCorreo
{
    /// <summary>Envía un correo en HTML.</summary>
    /// <param name="para">Dirección del destinatario.</param>
    /// <param name="asunto">Asunto.</param>
    /// <param name="html">Cuerpo en HTML.</param>
    /// <param name="imagenes">Imágenes incrustadas (por ejemplo, los QR), opcional.</param>
    Task EnviarAsync(string para, string asunto, string html, IEnumerable<ImagenIncrustada>? imagenes = null);
}
