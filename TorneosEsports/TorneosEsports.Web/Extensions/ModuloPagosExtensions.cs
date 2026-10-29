using System.Reflection;
using Microsoft.AspNetCore.Identity.UI.Services;
using TorneosEsports.Web.Services.Correo;
using TorneosEsports.Web.Services.Pagos;

namespace TorneosEsports.Web.Extensions;

/// <summary>
/// Registro del módulo de pago, correo y Web API (Deilyn), siguiendo el mismo patrón que AddModuloTorneos.
/// </summary>
public static class ModuloPagosExtensions
{
    /// <summary>Registra la pasarela, el cliente HTTP, el correo y Swagger.</summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <param name="configuration">Configuración (secciones "Pasarela" y "Correo").</param>
    /// <returns>La misma colección, para encadenar llamadas.</returns>
    public static IServiceCollection AddModuloPagos(this IServiceCollection services, IConfiguration configuration)
    {
        // Banco simulado: singleton porque guarda en memoria los pagos aprobados.
        services.AddSingleton<BancoSimulado>();

        // Cliente de la pasarela: la dirección base sale de appsettings y el tiempo límite es de 5 s.
        var urlBase = configuration["Pasarela:UrlBase"] ?? "https://localhost:7168/";
        services.AddHttpClient<IPasarelaPago, PasarelaPagoApi>(cliente =>
        {
            cliente.BaseAddress = new Uri(urlBase);
            cliente.Timeout = TimeSpan.FromSeconds(5);
        });

        // Correo: la misma clase sirve para las compras y para Identity (confirmar cuenta).
        services.Configure<ConfiguracionCorreo>(configuration.GetSection("Correo"));
        services.AddTransient<ServicioCorreo>();
        services.AddTransient<IServicioCorreo>(sp => sp.GetRequiredService<ServicioCorreo>());
        services.AddTransient<IEmailSender>(sp => sp.GetRequiredService<ServicioCorreo>());

        // Documentación OpenAPI de la Web API (solo las rutas api/...).
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opciones =>
        {
            var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            if (File.Exists(xml)) opciones.IncludeXmlComments(xml);

            opciones.DocInclusionPredicate((_, api) =>
                api.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);
        });

        return services;
    }

    /// <summary>Publica Swagger en /swagger mientras se desarrolla.</summary>
    /// <param name="app">La aplicación.</param>
    /// <returns>La misma aplicación.</returns>
    public static WebApplication UseModuloPagos(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }
}
