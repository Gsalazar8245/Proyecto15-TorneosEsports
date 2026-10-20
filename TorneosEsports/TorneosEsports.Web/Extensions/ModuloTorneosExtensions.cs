using TorneosEsports.Web.Services.Interfaces;
using TorneosEsports.Web.Services.Torneos;

namespace TorneosEsports.Web.Extensions;

/// <summary>
/// Registro del módulo de torneos, base de datos y llaves en el contenedor de dependencias.
/// </summary>
/// <remarks>
/// En Program.cs basta con <c>builder.Services.AddModuloTorneos();</c>. Cada integrante
/// tiene su propio archivo como este, así Program.cs casi no se toca y no hay conflictos.
/// </remarks>
public static class ModuloTorneosExtensions
{
    /// <summary>Registra los servicios del módulo de torneos.</summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <returns>La misma colección, para encadenar llamadas.</returns>
    public static IServiceCollection AddModuloTorneos(this IServiceCollection services)
    {
        services.AddScoped<IServicioTorneos, ServicioTorneos>();
        return services;
    }
}
