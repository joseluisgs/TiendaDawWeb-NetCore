using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.Services.Implementations;
using TiendaDawWeb.Services.Implementations.BackgroundServices;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using TiendaDawWeb.Binders;

Console.OutputEncoding = Encoding.UTF8;

// Configure Serilog before building the application
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

// Configurar cultura española por defecto
var defaultCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Entity Framework Core con InMemory
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("WalaDawDb"));

// ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole<long>>(options =>
{
    // Password settings - configuración flexible para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    
    // Signin settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Servicios de aplicación (Scoped para mantener contexto por request)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IStorageService, StorageService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();

// Background Services
builder.Services.AddHostedService<CarritoCleanupService>();
builder.Services.AddHostedService<ReservaCleanupService>();

// Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// MVC + Razor Pages (removed Blazor Server)
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
})
.AddViewLocalization() // Añadir localización para vistas
.AddDataAnnotationsLocalization(); // Añadir localización para anotaciones
builder.Services.AddRazorPages();

// Add antiforgery for AJAX requests
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// Session para carrito de compras
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS (si es necesario para desarrollo)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Logging (handled by Serilog)
// Removed default logging configuration

var app = builder.Build();

// Seed Data (inicializar datos de ejemplo)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var scopeLogger = services.GetRequiredService<ILogger<Program>>();
        scopeLogger.LogError(ex, "Error al inicializar la base de datos");
    }
}

// Configurar path base para uploads de forma segura
var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadPath = Path.Combine(webRootPath, "uploads");

// Asegurar que wwwroot existe
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

// Limpiar directorio de uploads al iniciar (tanto en DEV como PROD)
try 
{
    if (Directory.Exists(uploadPath))
    {
        Log.Information("🗑️ Limpiando directorio uploads en: {Path}", uploadPath);
        Directory.Delete(uploadPath, true);
        Log.Information("✅ Directorio uploads limpiado");
    }
}
catch (Exception ex)
{
    Log.Warning(ex, "⚠️ No se pudo limpiar completamente el directorio uploads, se intentará usar el existente.");
}

// Recrear directorio
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}
Log.Information("📁 Directorio uploads listo en: {Path}", uploadPath);

// Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar archivos estáticos para directorio uploads explícitamente
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

app.UseRouting();

// Configurar localización con soporte para parámetro ?lang=
var supportedCultures = new[] 
{ 
    new CultureInfo("es-ES"),    // 🔴 Español primero (default)
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR"),
    new CultureInfo("de-DE"),
    new CultureInfo("pt-PT")
};
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"), // 🔴 Español por defecto
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(), // Permite ?lang=en o ?culture=en
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

Log.Information("🌍 Soporte de localización configurado, idioma por defecto: 🇪🇸 es-ES");

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Enrutamiento de controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
// Removed MapBlazorHub - no longer using Blazor Server

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Startup banner - matching Spring Boot style
var appUrls = builder.Configuration["ASPNETCORE_URLS"] ?? "http://localhost:5000";
var port = appUrls.Split(';').FirstOrDefault()?.Split(':').LastOrDefault() ?? "5000";
Log.Information("🌐 Acceso: http://localhost:{Port}/Public", port);
Log.Information("🔑 Login admin: admin@waladaw.com / admin");
Log.Information("🔑 Login user: prueba@prueba.com / prueba");

try
{
    Log.Information("🚀 Aplicación iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
