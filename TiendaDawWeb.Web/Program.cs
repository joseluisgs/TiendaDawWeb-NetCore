using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Carrito;
using TiendaDawWeb.Services.Email;
using TiendaDawWeb.Services.Favorite;
using TiendaDawWeb.Services.Pdf;
using TiendaDawWeb.Services.Product;
using TiendaDawWeb.Services.Purchase;
using TiendaDawWeb.Services.Rating;
using TiendaDawWeb.Services.Storage;
using TiendaDawWeb.Services.BackgroundServices;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using TiendaDawWeb.Binders;
using TiendaDawWeb.Web.Middlewares;
using TiendaDawWeb.Web.Hubs;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.OutputCaching;

// Configura la codificación de la consola a UTF8 para evitar problemas con tildes y eñes en los logs
Console.OutputEncoding = Encoding.UTF8;

// Configuración de Serilog: Reemplaza el logger por defecto de .NET por uno más potente y visual
Log.Logger = new LoggerConfiguration()
    // Define el nivel mínimo de log global. 'Information' es ideal para ver qué pasa sin saturar.
    .MinimumLevel.Information()
    
    // 💡 FILTRO ANTI-RUIDO:
    // 'Override' permite cambiar el nivel de log para namespaces específicos.
    
    // Silenciamos los logs internos de Microsoft (ASP.NET Core) a 'Warning'. 
    // Solo veremos si algo falla, no cada petición HTTP interna.
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    
    // Excepto los mensajes sobre el ciclo de vida de la app (ej. "Application started").
    // Queremos ver que la app ha arrancado correctamente.
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    
    // Silenciamos las consultas SQL generadas por Entity Framework.
    // Evita que la consola se llene de comandos SELECT/INSERT cada vez que la app accede a datos.
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    
    // Configura la salida hacia la consola
    .WriteTo.Console(
        // Define el formato visual: [Fecha Hora NIVEL] Mensaje + Excepción si la hubiera
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        // Aplica un tema de colores elegante para que los logs sean fáciles de leer de un vistazo
        theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = "wwwroot"
});

// OBJETIVO: Cargar los recursos estáticos (JS/CSS) de librerías de componentes (como Blazor).
// RAZÓN: Sin esto, los archivos virtuales de Blazor (_framework/blazor.server.js) no se encontrarían 
// durante el desarrollo si se sirven desde paquetes NuGet o proyectos referenciados.
builder.WebHost.UseStaticWebAssets();

// AJUSTE DINÁMICO DE RUTAS:
// Si ejecutamos desde la raíz de la solución, el 'ContentRoot' por defecto podría ser erróneo.
// Este bloque asegura que el servidor encuentre siempre la carpeta 'wwwroot' de la Web.
if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")) && 
    Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "TiendaDawWeb.Web", "wwwroot")))
{
    var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "TiendaDawWeb.Web");
    builder.Environment.ContentRootPath = projectPath;
    builder.Environment.WebRootPath = Path.Combine(projectPath, "wwwroot");
}

// Use Serilog for logging
builder.Host.UseSerilog();

// Configurar cultura española por defecto
var defaultCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// CONFIGURACIÓN DE PERSISTENCIA (SQLite In-Memory Persistente):
// 1. Creamos una conexión manual que mantendremos abierta durante todo el ciclo de vida de la app.
//    DataSource=:memory: indica que la DB vive solo en la RAM.
var keepAliveConnection = new SqliteConnection("DataSource=:memory:");
keepAliveConnection.Open();

// 2. Registramos el DbContext usando esa conexión persistente.
//    Aunque el DbContext es Scoped, todos compartirán la misma conexión Singleton (la misma RAM).
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(keepAliveConnection));

// 3. Opcional: Registramos la conexión para que se cierre limpiamente al apagar el servidor.
builder.Services.AddSingleton(keepAliveConnection);

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
builder.Services.AddScoped<RatingStateContainer>();

// Background Services
builder.Services.AddHostedService<CarritoCleanupService>();
builder.Services.AddHostedService<ReservaCleanupService>();

// Registro de servicios de localización para soportar .resx
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

// CONFIGURACIÓN MVC Y BLAZOR:
builder.Services.AddControllersWithViews(options =>
{
    // Registra nuestro binder personalizado para tratar comas decimales correctamente en toda la app
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
})
.AddViewLocalization() // Habilita la traducción en las vistas (.cshtml)
.AddDataAnnotationsLocalization(); // Habilita la traducción en los mensajes de validación de los Modelos

builder.Services.AddRazorPages();

// Registra los servicios necesarios para Blazor Server
// DetailedErrors = true es fundamental en desarrollo para ver por qué falla un componente
builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; }); 

// 🚀 MEJORA DE RENDIMIENTO: Registro de OutputCache (.NET 10)
// Permite cachear la salida HTML en el servidor para reducir carga de CPU y DB.
builder.Services.AddOutputCache();

// 🧠 CACHÉ DE APLICACIÓN: Registro de IMemoryCache para optimizar servicios
builder.Services.AddMemoryCache();

// 🔔 INTERACTIVIDAD: Registro de SignalR
// Habilita la comunicación bidireccional en tiempo real, para las notificaciones push
// No tiene nada que ver con Blazor, que usa SignalR internamente.,
// esto es solo para nuestro Hub personalizado.
builder.Services.AddSignalR();

// CONFIGURACIÓN DE SEGURIDAD AJAX:
// Obliga a que las peticiones POST de JS/Blazor incluyan este nombre de cabecera con el token CSRF
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "RequestVerificationToken";
});

// GESTIÓN DE ESTADO Y CACHÉ:
builder.Services.AddDistributedMemoryCache(); // Almacén en memoria para la sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true; // Impide que JavaScript acceda a la cookie de sesión (Seguridad)
    options.Cookie.IsEssential = true; // La sesión se cargará aunque el usuario no haya aceptado cookies de rastreo
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

// SEED DATA: Inicialización de la base de datos con datos de prueba
// Usamos un Scope para asegurar que el DbContext se libere correctamente tras la carga
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 🚨 PASO CRÍTICO (SQLite In-Memory):
        // A diferencia del proveedor 'InMemory', SQLite es un motor real que requiere 
        // que las tablas existan físicamente en la memoria antes de insertar datos.
        // EnsureCreatedAsync() analiza nuestros Modelos y crea el esquema (tablas y relaciones) 
        // automáticamente en cada arranque del servidor.
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        // Una vez las tablas existen, procedemos a llenarlas con datos de prueba
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var scopeLogger = services.GetRequiredService<ILogger<Program>>();
        scopeLogger.LogError(ex, "Error al inicializar la base de datos");
    }
}

// GESTIÓN DEL SISTEMA DE ARCHIVOS (UPLOADS):
var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadPath = Path.Combine(webRootPath, "uploads");

// Asegura que la carpeta física exista para evitar errores de IO
if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}

// Lógica de limpieza: Borramos los uploads antiguos al reiniciar el servidor
// Esto mantiene la base de datos InMemory sincronizada con los archivos físicos
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

// Recrea el directorio vacío si no existe
if (!Directory.Exists(uploadPath))
{
    Directory.CreateDirectory(uploadPath);
}
Log.Information("📁 Directorio uploads listo en: {Path}", uploadPath);

// Middleware Pipeline - El orden aquí es CRÍTICO.

// 🚨 RED DE SEGURIDAD GLOBAL: Captura excepciones no controladas
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // En desarrollo, podemos usar el handler personalizado o la página detallada
    app.UseExceptionHandler("/Error"); 
    // app.UseDeveloperExceptionPage(); // Comentamos para probar nuestra página de error
}

// 🌐 CAPTURA DE CÓDIGOS DE ESTADO (404, 403, etc.)
// Redirige a ErrorController pasando el código
app.UseStatusCodePagesWithReExecute("/Error/{0}"); 

// Redirige automáticamente peticiones HTTP a HTTPS
app.UseHttpsRedirection();

// Permite servir archivos desde wwwroot (css, js, imágenes)
app.UseStaticFiles();

// Configura archivos estáticos para el directorio virtual de uploads
// Esto permite que /uploads/foto.jpg sea accesible aunque esté fuera de wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

// Analiza la URL y decide qué ruta corresponde a la petición (antes de ejecutarla)
app.UseRouting();

// 🚀 MEJORA DE RENDIMIENTO: Middleware de OutputCache.
// Debe ir después de Routing pero antes de Authentication si queremos servir caché a anónimos.
// app.UseOutputCache();

// Configurar las culturas soportadas por la aplicación
var supportedCultures = new[] 
{ 
    new CultureInfo("es-ES"),
    new CultureInfo("en-US"),
    new CultureInfo("fr-FR"),
    new CultureInfo("de-DE"),
    new CultureInfo("pt-PT")
};

// Middleware de Localización: Detecta el idioma del usuario (Cookie, QueryString o Header)
// y lo aplica al hilo actual para traducir la UI
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    ApplyCurrentCultureToResponseHeaders = true,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(), 
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    }
});

Log.Information("🌍 Soporte de localización configurado, idioma por defecto: 🇪🇸 es-ES");

// Identifica quién es el usuario (lee la cookie de autenticación)
app.UseAuthentication();
// Determina si el usuario identificado tiene permiso para acceder al recurso solicitado
app.UseAuthorization();
// Habilita el uso de variables de sesión (necesario para el carrito de compras)
app.UseSession();

// Enrutamiento de controladores MVC (Controller/Action/Id)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Habilita el enrutamiento para Razor Pages si existieran
app.MapRazorPages();

// Punto de conexión para Blazor Server. Crea el túnel SignalR para la interactividad real-time
app.MapBlazorHub(); 

// Punto de conexión para nuestro Hub de Notificaciones personalizado
app.MapHub<NotificationHub>("/notificationHub");

// Endpoint de salud del sistema: útil para monitorización y Docker

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