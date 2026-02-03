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
using TiendaDawWeb.Services.Storage;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de perfil de usuario.
/// LO QUE BUSCA: Asegurar que las operaciones de perfil funcionan correctamente.
/// </summary>
[TestFixture]
public class ProfileControllerTests
{
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<ApplicationDbContext> _contextMock = null!;
    private Mock<IStorageService> _storageServiceMock = null!;
    private Mock<ILogger<ProfileController>> _loggerMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private ProfileController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        _contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _storageServiceMock = new Mock<IStorageService>();
        _loggerMock = new Mock<ILogger<ProfileController>>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _controller = new ProfileController(
            _userManagerMock.Object,
            _contextMock.Object,
            _storageServiceMock.Object,
            _loggerMock.Object)
        {
            TempData = _tempDataMock.Object
        };

        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = context };
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Index redirige a login si no hay usuario.
    /// </summary>
    [Test]
    public async Task Index_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Index();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: Edit GET redirige a login si no hay usuario.
    /// </summary>
    [Test]
    public async Task Edit_Get_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Edit();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: Edit POST actualiza perfil exitosamente.
    /// </summary>
    [Test]
    public async Task Edit_Post_ShouldRedirectToIndex_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test", Nombre = "Test", Apellidos = "User" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.Edit("Nuevo Nombre", "Nuevos Apellidos", null);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Success"] = "Perfil actualizado correctamente", Times.Once);
    }

    /// <summary>
    /// PRUEBA: Edit POST muestra error si falta nombre y apellido.
    /// </summary>
    [Test]
    public async Task Edit_Post_ShouldShowError_WhenNameAndSurnameAreEmpty()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.Edit("", "", null);

        // Assert
        _tempDataMock.VerifySet(t => t["Error"] = "El nombre y apellidos son obligatorios", Times.Once);
    }

    /// <summary>
    /// PRUEBA: Edit POST muestra error si archivo muy grande.
    /// </summary>
    [Test]
    public async Task Edit_Post_ShouldShowError_WhenFileTooLarge()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6MB
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        // Act
        var result = await _controller.Edit("Nombre", "Apellido", fileMock.Object);

        // Assert
        _tempDataMock.VerifySet(t => t["Error"] = "El archivo es demasiado grande. Máximo 5MB", Times.Once);
    }

    /// <summary>
    /// PRUEBA: Edit POST muestra error si tipo de archivo no válido.
    /// </summary>
    [Test]
    public async Task Edit_Post_ShouldShowError_WhenInvalidFileType()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1000);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        // Act
        var result = await _controller.Edit("Nombre", "Apellido", fileMock.Object);

        // Assert
        _tempDataMock.VerifySet(t => t["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF)", Times.Once);
    }

    /// <summary>
    /// PRUEBA: DeleteAvatar elimina avatar correctamente.
    /// </summary>
    [Test]
    public async Task DeleteAvatar_ShouldRedirectToEdit_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test", Avatar = "old-avatar.jpg" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _storageServiceMock.Setup(s => s.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<bool, TiendaDawWeb.Errors.DomainError>(true));
        _userManagerMock.Setup(u => u.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.DeleteAvatar();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Edit");
        _tempDataMock.VerifySet(t => t["Success"] = "Avatar eliminado correctamente", Times.Once);
    }

    /// <summary>
    /// PRUEBA: ChangePassword GET muestra formulario.
    /// </summary>
    [Test]
    public void ChangePassword_Get_ShouldReturnView()
    {
        // Act
        var result = _controller.ChangePassword();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: ChangePassword POST redirige a login si no hay usuario.
    /// </summary>
    [Test]
    public async Task ChangePassword_Post_ShouldRedirectToLogin_WhenUserNotFound()
    {
        // Arrange
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.ChangePassword("old", "new", "new");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Login");
        redirectResult.ControllerName.Should().Be("Auth");
    }

    /// <summary>
    /// PRUEBA: ChangePassword POST muestra error si faltan campos.
    /// </summary>
    [Test]
    public async Task ChangePassword_Post_ShouldShowError_WhenFieldsEmpty()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.ChangePassword("", "", "");

        // Assert
        _tempDataMock.VerifySet(t => t["Error"] = "Todos los campos son obligatorios", Times.Once);
    }

    /// <summary>
    /// PRUEBA: ChangePassword POST muestra error si contraseñas no coinciden.
    /// </summary>
    [Test]
    public async Task ChangePassword_Post_ShouldShowError_WhenPasswordsNotMatch()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.ChangePassword("1234", "different", "5678");

        // Assert
        _tempDataMock.VerifySet(t => t["Error"] = "Las contraseñas no coinciden", Times.Once);
    }

    /// <summary>
    /// PRUEBA: ChangePassword POST muestra error si contraseña muy corta.
    /// </summary>
    [Test]
    public async Task ChangePassword_Post_ShouldShowError_WhenPasswordTooShort()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.ChangePassword("123", "123", "45");

        // Assert - Returns view with error when password is too short
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: ChangePassword POST cambia contraseña exitosamente.
    /// </summary>
    [Test]
    public async Task ChangePassword_Post_ShouldRedirectToIndex_WhenSuccess()
    {
        // Arrange
        var user = new User { Id = 1, UserName = "test" };
        _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);
        _userManagerMock.Setup(u => u.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.ChangePassword("old123", "new1234", "new1234");

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        _tempDataMock.VerifySet(t => t["Success"] = "Contraseña cambiada correctamente", Times.Once);
    }
}
