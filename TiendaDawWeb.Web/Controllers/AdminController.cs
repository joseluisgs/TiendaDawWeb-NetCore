using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para el panel de administración
///     Solo accesible para usuarios con rol ADMIN
/// </summary>
[Authorize(Roles = "ADMIN")]
[Route("admin")]
public class AdminController(
    ApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<IdentityRole<long>> roleManager,
    IPurchaseService purchaseService,
    IProductService productService,
    ILogger<AdminController> logger
) : Controller {
    private readonly IPurchaseService _purchaseService = purchaseService;

    /// <summary>
    ///     GET /admin - Dashboard principal con estadísticas
    ///     GET /admin/dashboard - Alias for Index (matches Spring Boot original)
    /// </summary>
    [HttpGet]
    [HttpGet("dashboard")]
    public async Task<IActionResult> Index() {
        var viewModel = new AdminDashboardViewModel();

        // Obtener estadísticas generales
        viewModel.TotalUsuarios = await context.Users.CountAsync(u => !u.Deleted);
        viewModel.TotalProductos = await context.Products.CountAsync(p => !p.Deleted);
        viewModel.TotalCompras = await context.Purchases.CountAsync();
        viewModel.TotalVentas = await context.Purchases.SumAsync(p => p.Total);

        // Usuarios activos (que no están eliminados)
        viewModel.UsuariosActivos = viewModel.TotalUsuarios;

        // Productos disponibles (no vendidos)
        viewModel.ProductosDisponibles = await context.Products
            .CountAsync(p => !p.Deleted && p.CompraId == null);

        // Estadísticas de tiempo
        var now = DateTime.UtcNow;
        var hoy = now.Date;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        var inicioMes = new DateTime(now.Year, now.Month, 1);

        // Compras por período
        viewModel.ComprasHoy = await context.Purchases
            .CountAsync(p => p.FechaCompra >= hoy);
        viewModel.ComprasSemana = await context.Purchases
            .CountAsync(p => p.FechaCompra >= inicioSemana);
        viewModel.ComprasMes = await context.Purchases
            .CountAsync(p => p.FechaCompra >= inicioMes);

        // Ventas por período
        viewModel.VentasHoy = await context.Purchases
            .Where(p => p.FechaCompra >= hoy)
            .SumAsync(p => (decimal?)p.Total) ?? 0;
        viewModel.VentasSemana = await context.Purchases
            .Where(p => p.FechaCompra >= inicioSemana)
            .SumAsync(p => (decimal?)p.Total) ?? 0;
        viewModel.VentasMes = await context.Purchases
            .Where(p => p.FechaCompra >= inicioMes)
            .SumAsync(p => (decimal?)p.Total) ?? 0;

        return View(viewModel);
    }

    /// <summary>
    ///     GET /admin/usuarios - Lista de usuarios
    /// </summary>
    [HttpGet("usuarios")]
    public async Task<IActionResult> Usuarios(int page = 1, int pageSize = 20) {
        var skip = (page - 1) * pageSize;

        var usuarios = await context.Users
            .Include(u => u.Products.Where(p => !p.Deleted))
            .Include(u => u.Purchases)
            .Where(u => !u.Deleted)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalUsuarios = await context.Users.CountAsync(u => !u.Deleted);

        return View(usuarios);
    }

    /// <summary>
    ///     GET /admin/usuarios/{id} - Detalle de usuario
    /// </summary>
    [HttpGet("usuarios/{id}")]
    public async Task<IActionResult> UsuarioDetails(long id) {
        var usuario = await context.Users
            .Include(u => u.Products)
            .Include(u => u.Purchases)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToAction(nameof(Usuarios));
        }

        // Obtener roles del usuario
        var roles = await userManager.GetRolesAsync(usuario);
        ViewBag.Roles = roles.ToList();

        return View(usuario);
    }

    /// <summary>
    ///     POST /admin/usuarios/{id}/cambiar-rol - Cambiar rol de usuario
    /// </summary>
    [HttpPost("usuarios/{id}/cambiar-rol")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarRol(long id, string nuevoRol) {
        var usuario = await userManager.FindByIdAsync(id.ToString());
        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToAction(nameof(Usuarios));
        }

        // Verificar que el rol existe
        if (!await roleManager.RoleExistsAsync(nuevoRol)) {
            TempData["Error"] = "Rol no válido";
            return RedirectToAction(nameof(UsuarioDetails), new { id });
        }

        // Remover roles actuales
        var rolesActuales = await userManager.GetRolesAsync(usuario);
        await userManager.RemoveFromRolesAsync(usuario, rolesActuales);

        // Añadir nuevo rol
        var result = await userManager.AddToRoleAsync(usuario, nuevoRol);

        if (result.Succeeded) {
            logger.LogInformation("Rol de usuario {UserId} cambiado a {Role}", id, nuevoRol);
            TempData["Success"] = $"Rol cambiado a {nuevoRol}";
        }
        else {
            TempData["Error"] = "Error al cambiar el rol";
        }

        return RedirectToAction(nameof(UsuarioDetails), new { id });
    }

    /// <summary>
    ///     POST /admin/usuarios/{id}/eliminar - Soft delete de usuario
    /// </summary>
    [HttpPost("usuarios/{id}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarUsuario(long id) {
        var usuario = await context.Users.FindAsync(id);
        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToAction(nameof(Usuarios));
        }

        // CRÍTICO: Impedir eliminar usuarios con productos activos (no vendidos)
        var hasProductosActivos = await context.Products
            .IgnoreQueryFilters() // Ignorar filtro de soft delete para comprobar todos los productos
            .Where(p => p.PropietarioId == id && !p.Deleted && p.CompraId == null)
            .AnyAsync();

        if (hasProductosActivos) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con productos activos a la venta", id);
            TempData["Error"] = "No se puede eliminar un usuario con productos a la venta";
            return RedirectToAction(nameof(Usuarios));
        }

        // CRÍTICO: Impedir eliminar usuarios con productos vendidos
        var hasProductosVendidos = await context.Products
            .IgnoreQueryFilters()
            .Where(p => p.PropietarioId == id && p.CompraId != null)
            .AnyAsync();

        if (hasProductosVendidos) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con productos vendidos", id);
            TempData["Error"] = "No se puede eliminar un usuario que ha vendido productos";
            return RedirectToAction(nameof(Usuarios));
        }

        // Impedir eliminar usuarios con compras realizadas
        var hasCompras = await context.Purchases
            .AnyAsync(p => p.CompradorId == id);

        if (hasCompras) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con compras realizadas", id);
            TempData["Error"] = "No se puede eliminar un usuario que ha realizado compras";
            return RedirectToAction(nameof(Usuarios));
        }

        // Soft delete
        usuario.Deleted = true;
        usuario.DeletedAt = DateTime.UtcNow;

        var adminUser = await userManager.GetUserAsync(User);
        usuario.DeletedBy = adminUser?.Id.ToString();

        await context.SaveChangesAsync();

        logger.LogInformation("Usuario {UserId} eliminado (soft delete) por admin {AdminId}",
            id, adminUser?.Id);

        TempData["Success"] = "Usuario eliminado correctamente";
        return RedirectToAction(nameof(Usuarios));
    }

    /// <summary>
    ///     GET /admin/productos - Lista de todos los productos
    /// </summary>
    [HttpGet("productos")]
    public async Task<IActionResult> Productos(int page = 1, int pageSize = 20, string? categoria = null) {
        var skip = (page - 1) * pageSize;

        var query = context.Products
            .Include(p => p.Propietario)
            .Where(p => !p.Deleted);

        if (!string.IsNullOrEmpty(categoria))
            if (Enum.TryParse<ProductCategory>(categoria, out var cat))
                query = query.Where(p => p.Categoria == cat);

        var productos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalProductos = await query.CountAsync();
        ViewBag.CategoriaSeleccionada = categoria;

        return View(productos);
    }

    /// <summary>
    ///     POST /admin/productos/{id}/eliminar - Soft delete de producto
    /// </summary>
    [HttpPost("productos/{id}/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProducto(long id) {
        var adminUser = await userManager.GetUserAsync(User);
        if (adminUser == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToAction(nameof(Productos));
        }

        var result = await productService.DeleteAsync(id, adminUser.Id, true);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto eliminado correctamente";

        return RedirectToAction(nameof(Productos));
    }

    /// <summary>
    ///     GET /admin/compras - Lista de todas las compras
    /// </summary>
    [HttpGet("compras")]
    public async Task<IActionResult> Compras(int page = 1, int pageSize = 20, DateTime? desde = null,
        DateTime? hasta = null) {
        var skip = (page - 1) * pageSize;

        var query = context.Purchases
            .Include(p => p.Comprador)
            .Include(p => p.Products)
            .AsQueryable();

        // Filtrar por fecha si se proporciona
        if (desde.HasValue) query = query.Where(p => p.FechaCompra >= desde.Value);
        if (hasta.HasValue) {
            var hastaFinal = hasta.Value.AddDays(1).AddSeconds(-1);
            query = query.Where(p => p.FechaCompra <= hastaFinal);
        }

        var compras = await query
            .OrderByDescending(p => p.FechaCompra)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCompras = await query.CountAsync();
        ViewBag.Desde = desde;
        ViewBag.Hasta = hasta;

        return View(compras);
    }

    /// <summary>
    ///     GET /admin/ventas - Alias for Compras (matches Spring Boot original)
    /// </summary>
    [HttpGet("ventas")]
    public async Task<IActionResult> Ventas(int page = 1, int pageSize = 20, DateTime? desde = null,
        DateTime? hasta = null) {
        return await Compras(page, pageSize, desde, hasta);
    }

    /// <summary>
    ///     GET /admin/estadisticas - Estadísticas avanzadas
    /// </summary>
    [HttpGet("estadisticas")]
    public async Task<IActionResult> Estadisticas() {
        // Productos más vendidos (top 10)
        var productosMasVendidos = await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => p.Categoria)
            .Select(g => new {
                Categoria = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();

        // Compradores más activos (top 10)
        var compradoresActivos = await context.Purchases
            .GroupBy(p => p.CompradorId)
            .Select(g => new {
                CompradorId = g.Key,
                TotalCompras = g.Count(),
                TotalGastado = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.TotalCompras)
            .Take(10)
            .ToListAsync();

        // Vendedores más activos
        var vendedoresActivos = await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => p.PropietarioId)
            .Select(g => new {
                PropietarioId = g.Key,
                ProductosVendidos = g.Count()
            })
            .OrderByDescending(x => x.ProductosVendidos)
            .Take(10)
            .ToListAsync();

        // Ventas por mes (últimos 12 meses)
        var hace12Meses = DateTime.UtcNow.AddMonths(-12);
        var ventasPorMes = await context.Purchases
            .Where(p => p.FechaCompra >= hace12Meses)
            .GroupBy(p => new { p.FechaCompra.Year, p.FechaCompra.Month })
            .Select(g => new {
                Año = g.Key.Year,
                Mes = g.Key.Month,
                TotalVentas = g.Sum(p => p.Total),
                NumeroCompras = g.Count()
            })
            .OrderBy(x => x.Año)
            .ThenBy(x => x.Mes)
            .ToListAsync();

        ViewBag.ProductosMasVendidos = productosMasVendidos;
        ViewBag.CompradoresActivos = compradoresActivos;
        ViewBag.VendedoresActivos = vendedoresActivos;
        ViewBag.VentasPorMes = ventasPorMes;

        return View();
    }
}