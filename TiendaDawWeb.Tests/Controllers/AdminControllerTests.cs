using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Product;
using TiendaDawWeb.Services.Purchase;
using TiendaDawWeb.ViewModels;
using FluentAssertions;
using CSharpFunctionalExtensions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de administración.
/// LO QUE BUSCA: Asegurar que las operaciones de admin funcionan correctamente.
/// </summary>
[TestFixture]
public class AdminControllerTests
{
    private Mock<ApplicationDbContext> _contextMock = null!;
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<RoleManager<IdentityRole<long>>> _roleManagerMock = null!;
    private Mock<IPurchaseService> _purchaseServiceMock = null!;
    private Mock<IProductService> _productServiceMock = null!;
    private Mock<ILogger<AdminController>> _loggerMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private AdminController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        var roleStoreMock = new Mock<IRoleStore<IdentityRole<long>>>();
        _roleManagerMock = new Mock<RoleManager<IdentityRole<long>>>(
            roleStoreMock.Object,
            null!, null!, null!, null!);

        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _purchaseServiceMock = new Mock<IPurchaseService>();
        _productServiceMock = new Mock<IProductService>();
        _loggerMock = new Mock<ILogger<AdminController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new AdminController(
            _contextMock.Object,
            _userManagerMock.Object,
            _roleManagerMock.Object,
            _purchaseServiceMock.Object,
            _productServiceMock.Object,
            _loggerMock.Object)
        {
            TempData = _tempDataMock.Object
        };

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: CambiarRol redirige si usuario no existe.
    /// </summary>
    [Test]
    public async Task CambiarRol_ShouldRedirect_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.FindByIdAsync("999"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.CambiarRol(999, "USER");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Usuarios");
        _tempDataMock.VerifySet(t => t["Error"] = "Usuario no encontrado", Times.Once);
    }

    /// <summary>
    /// PRUEBA: CambiarRol redirige si rol no válido.
    /// </summary>
    [Test]
    public async Task CambiarRol_ShouldRedirect_WhenRoleNotExists()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(r => r.RoleExistsAsync("INVALID_ROLE"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CambiarRol(1, "INVALID_ROLE");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("UsuarioDetails");
        _tempDataMock.VerifySet(t => t["Error"] = "Rol no válido", Times.Once);
    }

    /// <summary>
    /// PRUEBA: CambiarRol cambia rol exitosamente.
    /// </summary>
    [Test]
    public async Task CambiarRol_ShouldChangeRoleSuccessfully()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.FindByIdAsync("1"))
            .ReturnsAsync(user);
        _roleManagerMock.Setup(r => r.RoleExistsAsync("USER"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "ADMIN" });
        _userManagerMock.Setup(u => u.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(user, "USER"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.CambiarRol(1, "USER");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("UsuarioDetails");
        _tempDataMock.VerifySet(t => t["Success"] = "Rol cambiado a USER", Times.Once);
    }

    /// <summary>
    /// PRUEBA: EliminarUsuario redirige si no existe.
    /// </summary>
    [Test]
    public async Task EliminarUsuario_ShouldRedirect_WhenUserNotFound()
    {
        // Arrange
        _contextMock.Setup(c => c.Users.FindAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.EliminarUsuario(999);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Usuarios");
        _tempDataMock.VerifySet(t => t["Error"] = "Usuario no encontrado", Times.Once);
    }

    /// <summary>
    /// PRUEBA: EliminarProducto elimina exitosamente.
    /// </summary>
    [Test]
    public async Task EliminarProducto_ShouldDeleteSuccessfully()
    {
        // Arrange
        var adminUser = new User { Id = 1, UserName = "admin" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(adminUser);
        _productServiceMock.Setup(s => s.DeleteAsync(1, adminUser.Id, true))
            .ReturnsAsync(Result.Success<bool, TiendaDawWeb.Errors.DomainError>(true));

        // Act
        var result = await _controller.EliminarProducto(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Productos");
        _tempDataMock.VerifySet(t => t["Success"] = "Producto eliminado correctamente", Times.Once);
    }

    /// <summary>
    /// PRUEBA: EliminarProducto muestra error si falla.
    /// </summary>
    [Test]
    public async Task EliminarProducto_ShouldShowError_WhenFails()
    {
        // Arrange
        var adminUser = new User { Id = 1, UserName = "admin" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(adminUser);
        _productServiceMock.Setup(s => s.DeleteAsync(1, adminUser.Id, true))
            .ReturnsAsync(Result.Failure<bool, TiendaDawWeb.Errors.DomainError>(
                new TiendaDawWeb.Errors.NotFoundError("Producto no encontrado")));

        // Act
        var result = await _controller.EliminarProducto(1);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Productos");
        _tempDataMock.VerifySet(t => t["Error"] = "Producto no encontrado", Times.Once);
    }

    /// <summary>
    /// PRUEBA: EliminarProducto muestra error si producto no encontrado.
    /// </summary>
    [Test]
    public async Task EliminarProducto_ShouldShowError_WhenProductNotFound()
    {
        // Arrange
        var adminUser = new User { Id = 1, UserName = "admin" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(adminUser);
        _productServiceMock.Setup(s => s.DeleteAsync(999, adminUser.Id, true))
            .ReturnsAsync(Result.Failure<bool, TiendaDawWeb.Errors.DomainError>(
                new TiendaDawWeb.Errors.NotFoundError("Producto no encontrado")));

        // Act
        var result = await _controller.EliminarProducto(999);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Productos");
        _tempDataMock.VerifySet(t => t["Error"] = "Producto no encontrado", Times.Once);
    }
}
