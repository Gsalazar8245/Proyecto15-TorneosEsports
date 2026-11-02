using TorneosEsports.Web.Models.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TorneosEsports.Web.Data.Semilla;

/// <summary>
/// Equipos de prueba para demostrar el cierre de inscripciones y las llaves sin tener que
/// inscribir equipos a mano.
/// </summary>
/// <remarks>
/// Corre solo en Desarrollo y solo si el torneo de Valorant (Id 1)
/// no tiene equipos. Crea 6 equipos pagados: como 6 no es potencia de 2, la demo muestra
/// también los byes.
/// </remarks>
public static class DatosDePrueba
{
    /// <summary>Correo del usuario capitán de los equipos de prueba.</summary>
    public const string CorreoCapitan = "capitan.demo@arenalan.test";

    /// <summary>Contraseña de los usuarios de prueba (solo para desarrollo; cumple la política de 12 caracteres).</summary>
    public const string ClaveCapitan = "ArenaDemo2026!";

    /// <summary>Correo del administrador temporal de pruebas.</summary>
    public const string CorreoAdminTemporal = "admin.demo@arenalan.test";

    private const int TorneoDemoId = 1;

    private static readonly string[] NombresEquipos =
        ["Volcanes FC", "Quetzales Gaming", "Lapas Rojas", "Toros de Zarcero", "Jaguares Esports", "Pura Vida Squad"];

    /// <summary>Aplica las migraciones pendientes y crea los equipos de prueba si faltan.</summary>
    /// <param name="servicios">Proveedor de servicios con un scope ya creado.</param>
    /// <returns>Tarea asíncrona.</returns>
    public static async Task SembrarAsync(IServiceProvider servicios)
    {
        var db = servicios.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var usuarios = servicios.GetRequiredService<UserManager<ApplicationUser>>();
        await CrearAdministradorTemporalAsync(servicios, usuarios);

        if (await db.Equipos.AnyAsync(e => e.TorneoId == TorneoDemoId))
            return;

        var capitan = await ObtenerOCrearUsuarioAsync(usuarios, CorreoCapitan);

        var inicio = DateTime.UtcNow.AddDays(-6);
        for (int i = 0; i < NombresEquipos.Length; i++)
        {
            var equipo = new Equipo
            {
                TorneoId = TorneoDemoId,
                Nombre = NombresEquipos[i],
                CapitanId = capitan.Id,
                Pagado = true,
                FechaCreacion = inicio.AddHours(i), // el orden de inscripción define la siembra
            };
            for (int j = 1; j <= Equipo.IntegrantesRequeridos; j++)
            {
                string nick = $"{NombresEquipos[i].Split(' ')[0].ToLowerInvariant()}{j}";
                equipo.Integrantes.Add(new IntegranteEquipo
                {
                    Nombre = $"Jugador {j} de {NombresEquipos[i]}",
                    Nick = nick,
                    Correo = $"{nick}@arenalan.test",
                });
            }
            db.Equipos.Add(equipo);
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Crea un usuario administrador de prueba para la demo del mantenimiento de torneos y de
    /// las llaves. Los roles los crea IdentitySeeder; si todavía no existen, se crean aquí.
    /// </summary>
    /// <param name="servicios">Proveedor de servicios.</param>
    /// <param name="usuarios">Administrador de usuarios de Identity.</param>
    /// <returns>Tarea asíncrona.</returns>
    private static async Task CrearAdministradorTemporalAsync(IServiceProvider servicios, UserManager<ApplicationUser> usuarios)
    {
        var roles = servicios.GetService<RoleManager<IdentityRole>>();
        if (roles is null)
            return; // Identity se registró sin AddRoles<IdentityRole>().

        const string rol = Controllers.TorneosController.RolAdministrador;
        if (!await roles.RoleExistsAsync(rol))
            await roles.CreateAsync(new IdentityRole(rol));

        var admin = await ObtenerOCrearUsuarioAsync(usuarios, CorreoAdminTemporal);
        if (!await usuarios.IsInRoleAsync(admin, rol))
            await usuarios.AddToRoleAsync(admin, rol);
    }

    /// <summary>Busca un usuario por correo y, si no existe, lo crea con la clave de prueba.</summary>
    /// <param name="usuarios">Administrador de usuarios de Identity.</param>
    /// <param name="correo">Correo (también es el nombre de usuario).</param>
    /// <returns>El usuario.</returns>
    private static async Task<ApplicationUser> ObtenerOCrearUsuarioAsync(UserManager<ApplicationUser> usuarios, string correo)
    {
        var usuario = await usuarios.FindByEmailAsync(correo);
        if (usuario is not null)
            return usuario;

        usuario = new ApplicationUser { UserName = correo, Email = correo, EmailConfirmed = true };
        var creado = await usuarios.CreateAsync(usuario, ClaveCapitan);
        if (!creado.Succeeded)
            throw new InvalidOperationException(
                $"No se pudo crear {correo}: " + string.Join("; ", creado.Errors.Select(e => e.Description)));
        return usuario;
    }
}
