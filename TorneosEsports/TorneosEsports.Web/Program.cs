using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TorneosEsports.Web.Data;
using TorneosEsports.Web.Extensions;
using TorneosEsports.Web.Security;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configuración de la base de datos.
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSignalR();
builder.Services.AddScoped<TorneosEsports.Web.Hubs.IBracketNotificador, TorneosEsports.Web.Hubs.BracketNotificador>();
builder.Services.AddScoped<TorneosEsports.Web.Services.Interfaces.INotificadorLlaves, TorneosEsports.Web.Hubs.NotificadorLlavesSignalR>();

// Configuración de ASP.NET Core Identity con roles.
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Confirmación de correo obligatoria.
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Requisitos de contraseña.
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Bloqueo temporal tras varios intentos fallidos.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configuración de MVC.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
// Servicios necesarios para las páginas Razor de Identity.
builder.Services.AddRazorPages();

// Módulos de cada integrante.
builder.Services.AddModuloTorneos(); // Torneos, base de datos y llaves

var app = builder.Build();

// Configuración del manejo de errores.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configuración de las solicitudes HTTP.
app.UseHttpsRedirection();
app.UseRouting();

// Identificar al usuario y comprobar sus permisos.
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Archivos estáticos.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.MapHub<TorneosEsports.Web.Hubs.BracketHub>("/hubs/bracket");

// Páginas de ASP.NET Core Identity.
app.MapRazorPages()
   .WithStaticAssets();
// En Desarrollo: aplica las migraciones y crea los datos de prueba (equipos y administrador de prueba).
   await app.SembrarDatosDePruebaAsync();
// Crear los roles iniciales del sistema.
await IdentitySeeder.InitializeAsync(app.Services, app.Configuration);
app.Run();