namespace TorneosEsports.Web.Services.Correo;

/// <summary>
/// Sección "Correo" de appsettings.json.
/// </summary>
/// <remarks>
/// Si <see cref="Host"/> queda vacío, los correos no salen a Internet: se guardan como archivos .eml
/// en <see cref="CarpetaDesarrollo"/> (se abren con Outlook o el lector de correo de Windows).
/// Para enviar de verdad se llena Host, Puerto, Usuario y Clave; la clave va en User Secrets, nunca en el repositorio.
/// </remarks>
public class ConfiguracionCorreo
{
    /// <summary>Servidor SMTP, por ejemplo smtp.gmail.com. Vacío = modo desarrollo (archivos .eml).</summary>
    public string? Host { get; set; }

    /// <summary>Puerto SMTP (587 con TLS).</summary>
    public int Puerto { get; set; } = 587;

    /// <summary>Usar conexión cifrada.</summary>
    public bool UsarSsl { get; set; } = true;

    /// <summary>Usuario de la cuenta SMTP.</summary>
    public string? Usuario { get; set; }

    /// <summary>Clave o contraseña de aplicación de la cuenta SMTP (User Secrets).</summary>
    public string? Clave { get; set; }

    /// <summary>Dirección que aparece como remitente.</summary>
    public string Remitente { get; set; } = "no-responder@torneosesports.local";

    /// <summary>Nombre que aparece como remitente.</summary>
    public string NombreRemitente { get; set; } = "Torneos Esports y LAN Party";

    /// <summary>Carpeta (relativa al proyecto) donde se guardan los correos en modo desarrollo.</summary>
    public string CarpetaDesarrollo { get; set; } = "CorreosEnviados";
}
