using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace TorneosEsports.Web.Services.Correo;

/// <summary>
/// Correo base de la aplicación. Implementa <see cref="IServicioCorreo"/> (compras) y también
/// <see cref="IEmailSender"/>, así los correos de Identity (confirmar cuenta, recuperar contraseña) dejan de perderse.
/// </summary>
/// <remarks>
/// Sin servidor SMTP configurado, cada correo se guarda como .eml en la carpeta CorreosEnviados del proyecto.
/// </remarks>
public class ServicioCorreo : IServicioCorreo, IEmailSender
{
    private readonly ConfiguracionCorreo _config;
    private readonly IWebHostEnvironment _entorno;
    private readonly ILogger<ServicioCorreo> _logger;

    /// <summary>Recibe la configuración de la sección "Correo", el entorno y el registro de eventos.</summary>
    public ServicioCorreo(IOptions<ConfiguracionCorreo> config, IWebHostEnvironment entorno, ILogger<ServicioCorreo> logger)
    {
        _config = config.Value;
        _entorno = entorno;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task EnviarAsync(string para, string asunto, string html, IEnumerable<ImagenIncrustada>? imagenes = null)
    {
        using var mensaje = new MailMessage
        {
            From = new MailAddress(_config.Remitente, _config.NombreRemitente),
            Subject = asunto,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        mensaje.To.Add(para);

        // Vista HTML con las imágenes incrustadas (los QR viajan dentro del correo, no como enlace).
        var vista = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, MediaTypeNames.Text.Html);
        foreach (var imagen in imagenes ?? Enumerable.Empty<ImagenIncrustada>())
        {
            vista.LinkedResources.Add(new LinkedResource(new MemoryStream(imagen.Contenido), imagen.TipoMime)
            {
                ContentId = imagen.IdContenido
            });
        }
        mensaje.AlternateViews.Add(vista);

        using var smtp = CrearCliente();
        await smtp.SendMailAsync(mensaje);
        _logger.LogInformation("Correo «{Asunto}» enviado a {Para}.", asunto, para);
    }

    /// <summary>Lo usa Identity para confirmar cuentas y recuperar contraseñas.</summary>
    Task IEmailSender.SendEmailAsync(string email, string subject, string htmlMessage) =>
        EnviarAsync(email, subject, htmlMessage);

    private SmtpClient CrearCliente()
    {
        if (string.IsNullOrWhiteSpace(_config.Host))
        {
            var carpeta = Path.Combine(_entorno.ContentRootPath, _config.CarpetaDesarrollo);
            Directory.CreateDirectory(carpeta);
            return new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = carpeta
            };
        }

        return new SmtpClient(_config.Host, _config.Puerto)
        {
            EnableSsl = _config.UsarSsl,
            Credentials = new NetworkCredential(_config.Usuario, _config.Clave)
        };
    }
}
