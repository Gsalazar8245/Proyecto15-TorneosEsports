using TorneosEsports.Web.Data.Semilla;
using TorneosEsports.Web.Services.Interfaces;
using TorneosEsports.Web.Services.Llaves;
using TorneosEsports.Web.Services.Torneos;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// <summary>Registra los servicios de torneos y llaves.</summary>
    /// <param name="services">Colección de servicios de la aplicación.</param>
    /// <returns>La misma colección, para encadenar llamadas.</returns>
    public static IServiceCollection AddModuloTorneos(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IServicioTorneos, ServicioTorneos>();
        services.AddScoped<IServicioLlaves, ServicioLlaves>();
        services.AddSingleton<IGeneradorLlaves, EliminacionDirecta>();

        // TryAdd: si se registra otro notificador (por ejemplo con SignalR), se usa ese.
        services.TryAddScoped<INotificadorLlaves, NotificadorLlavesNulo>();
        return services;
    }

    /// <summary>
    /// En Desarrollo, aplica las migraciones y crea los equipos de prueba.
    /// </summary>
    /// <param name="app">La aplicación ya construida.</param>
    /// <returns>Tarea asíncrona.</returns>
    public static async Task SembrarDatosDePruebaAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        using var scope = app.Services.CreateScope();
        await DatosDePrueba.SembrarAsync(scope.ServiceProvider);
    }
}
