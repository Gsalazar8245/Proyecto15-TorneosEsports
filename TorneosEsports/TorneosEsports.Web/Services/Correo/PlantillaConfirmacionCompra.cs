using System.Net;
using System.Text;
using QRCoder;
using TorneosEsports.Web.Models.Entidades;

namespace TorneosEsports.Web.Services.Correo;

/// <summary>Equipo pagado que lleva su QR en el correo, con la dirección de validación que codifica el QR.</summary>
/// <param name="Equipo">Equipo que pagó su inscripción.</param>
/// <param name="Juego">Juego del torneo.</param>
/// <param name="UrlValidacion">Dirección de Comprobante/Validar con el código del comprobante.</param>
public record EquipoConQr(Equipo Equipo, string Juego, string UrlValidacion);

/// <summary>
/// Arma el HTML del correo de confirmación de compra y los QR de cada equipo.
/// </summary>
/// <remarks>
/// El QR codifica la misma dirección que usa el comprobante de Stefanny (Comprobante/Validar?codigo=...),
/// así el administrador lo valida en la entrada igual que desde la pantalla.
/// </remarks>
public static class PlantillaConfirmacionCompra
{
    /// <summary>Genera el asunto, el cuerpo HTML y las imágenes QR del correo.</summary>
    /// <param name="pedido">Pedido pagado, con sus líneas.</param>
    /// <param name="codigoAutorizacion">Código que devolvió la pasarela.</param>
    /// <param name="equipos">Equipos inscritos en este pedido.</param>
    /// <returns>Asunto, HTML e imágenes listos para <see cref="IServicioCorreo.EnviarAsync"/>.</returns>
    public static (string Asunto, string Html, List<ImagenIncrustada> Imagenes) Crear(
        Pedido pedido, string codigoAutorizacion, IReadOnlyList<EquipoConQr> equipos)
    {
        var html = new StringBuilder();
        var imagenes = new List<ImagenIncrustada>();

        html.Append("<div style=\"font-family:Arial,sans-serif;max-width:600px\">");
        html.Append("<h2>¡Gracias por tu compra!</h2>");
        html.Append($"<p>Pedido <strong>#{pedido.Id}</strong> · Autorización <strong>{Html(codigoAutorizacion)}</strong> · {pedido.Fecha.ToLocalTime():dd/MM/yyyy HH:mm}</p>");

        html.Append("<table cellpadding=\"6\" style=\"border-collapse:collapse;width:100%\">");
        html.Append("<tr style=\"background:#f0f0f0\"><th align=\"left\">Producto</th><th>Cant.</th><th align=\"right\">Subtotal</th></tr>");
        foreach (var linea in pedido.Lineas)
        {
            html.Append($"<tr><td>{Html(linea.Descripcion)}</td><td align=\"center\">{linea.Cantidad}</td>" +
                        $"<td align=\"right\">₡{linea.Subtotal:N0}</td></tr>");
        }
        html.Append($"<tr><td colspan=\"2\" align=\"right\"><strong>Total</strong></td><td align=\"right\"><strong>₡{pedido.Total:N0}</strong></td></tr>");
        html.Append("</table>");

        foreach (var e in equipos)
        {
            var id = $"qr-{e.Equipo.Id}";
            imagenes.Add(new ImagenIncrustada(id, GenerarQr(e.UrlValidacion)));

            html.Append("<hr/>");
            html.Append($"<h3>Comprobante: {Html(e.Equipo.Nombre)}</h3>");
            html.Append($"<p>Torneo: {Html(e.Juego)}<br/>Presentá este código en la entrada.</p>");
            html.Append($"<img src=\"cid:{id}\" alt=\"Código QR de {Html(e.Equipo.Nombre)}\" width=\"220\" height=\"220\"/>");
        }

        html.Append("<p style=\"color:#777;font-size:12px\">Este correo se generó automáticamente; no lo respondas.</p></div>");

        return ($"Confirmación de compra #{pedido.Id}", html.ToString(), imagenes);
    }

    private static byte[] GenerarQr(string contenido)
    {
        using var generador = new QRCodeGenerator();
        using var datos = generador.CreateQrCode(contenido, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(datos).GetGraphic(10);
    }

    private static string Html(string? texto) => WebUtility.HtmlEncode(texto ?? string.Empty);
}
