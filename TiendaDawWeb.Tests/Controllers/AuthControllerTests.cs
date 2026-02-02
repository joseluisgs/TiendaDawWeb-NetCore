using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Models;
using TiendaDawWeb.ViewModels;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Controllers;

/// <summary>
/// OBJETIVO: Validar el comportamiento del controlador de autenticación.
/// LO QUE BUSCA: Asegurar que login, registro y logout funcionan correctamente.
/// </summary>
[TestFixture]
public class AuthControllerTests
{
    private Mock<UserManager<User>> _userManagerMock = null!;
    private Mock<SignInManager<User>> _signInManagerMock = null!;
    private Mock<ILogger<AuthController>> _loggerMock = null!;
    private AuthController _controller = null!;

    [SetUp]
    public void Setup()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        var contextAccessorMock = new Mock<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<User>>();
        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new HttpContextAccessor(),
            contextAccessorMock.Object,
            null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _controller?.Dispose();
    }

    /// <summary>
    /// PRUEBA: Login GET muestra la vista.
    /// </summary>
    [Test]
    public void Login_Get_ShouldReturnView()
    {
        // Act
        var result = _controller.Login();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: Login GET con returnUrl.
    /// </summary>
    [Test]
    public void Login_Get_ShouldReturnViewWithReturnUrl()
    {
        // Act
        var result = _controller.Login("/product");

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewData["ReturnUrl"].Should().Be("/product");
    }

    /// <summary>
    /// PRUEBA: Login con modelo inválido retorna la vista con errores.
    /// </summary>
    [Test]
    public async Task Login_Post_ShouldReturnView_WhenModelIsInvalid()
    {
        // Arrange
        var model = new LoginViewModel { Email = "", Password = "" };
        _controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<LoginViewModel>();
    }

    /// <summary>
    /// PRUEBA: Login con usuario no existente muestra error.
    /// </summary>
    [Test]
    public async Task Login_Post_ShouldShowError_WhenUserNotFound()
    {
        // Arrange
        var model = new LoginViewModel { Email = "notfound@test.com", Password = "password" };
        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.Should().ContainKey("");
        _signInManagerMock.Verify(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
    }

    /// <summary>
    /// PRUEBA: Login exitoso redirige a la página pública.
    /// </summary>
    [Test]
    public async Task Login_Post_ShouldRedirect_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test@test.com" };
        var model = new LoginViewModel { Email = "test@test.com", Password = "password", ReturnUrl = null };

        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(s => s.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        // Setup Url to avoid null reference
        _controller.Url = new Mock<IUrlHelper>().Object;

        // Act
        var result = await _controller.Login(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Public");
    }

    /// <summary>
    /// PRUEBA: Login fallido muestra error.
    /// </summary>
    [Test]
    public async Task Login_Post_ShouldShowError_WhenSignInFails()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test@test.com" };
        var model = new LoginViewModel { Email = "test@test.com", Password = "wrongpassword" };

        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(user);

        _signInManagerMock.Setup(s => s.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        // Act
        var result = await _controller.Login(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.Should().ContainKey("");
    }

    /// <summary>
    /// PRUEBA: Register GET muestra la vista.
    /// </summary>
    [Test]
    public void Register_Get_ShouldReturnView()
    {
        // Act
        var result = _controller.Register();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: Register con modelo inválido retorna la vista.
    /// </summary>
    [Test]
    public async Task Register_Post_ShouldReturnView_WhenModelIsInvalid()
    {
        // Arrange
        var model = new RegisterViewModel { Email = "", Password = "" };
        _controller.ModelState.AddModelError("Email", "Required");

        // Act
        var result = await _controller.Register(model);

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    /// <summary>
    /// PRUEBA: Register con email existente muestra error.
    /// </summary>
    [Test]
    public async Task Register_Post_ShouldShowError_WhenEmailExists()
    {
        // Arrange
        var existingUser = new User { Id = 1, Email = "existing@test.com" };
        var model = new RegisterViewModel
        {
            Email = "existing@test.com",
            Password = "Password123!",
            Nombre = "Test",
            Apellidos = "User"
        };

        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.Should().ContainKey("");
        _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// PRUEBA: Registro exitoso crea usuario y redirige.
    /// </summary>
    [Test]
    public async Task Register_Post_ShouldCreateUserAndRedirect_WhenSuccessful()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Email = "new@test.com",
            Password = "Password123!",
            Nombre = "New",
            Apellidos = "User"
        };

        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync((User?)null);

        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);

        _signInManagerMock.Setup(s => s.SignInAsync(It.IsAny<User>(), false))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Public");
    }

    /// <summary>
    /// PRUEBA: Registro fallido muestra errores de Identity.
    /// </summary>
    [Test]
    public async Task Register_Post_ShouldShowErrors_WhenCreationFails()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Email = "new@test.com",
            Password = "weak",
            Nombre = "New",
            Apellidos = "User"
        };

        var errors = new[] { new IdentityError { Description = "Password too weak" } };
        var failedResult = IdentityResult.Failed(errors);

        _userManagerMock.Setup(u => u.FindByEmailAsync(model.Email))
            .ReturnsAsync((User?)null);

        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(failedResult);

        // Act
        var result = await _controller.Register(model);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _controller.ModelState.Should().ContainKey("");
        _signInManagerMock.Verify(s => s.SignInAsync(It.IsAny<User>(), false), Times.Never);
    }

    /// <summary>
    /// PRUEBA: Logout cierra sesión y redirige.
    /// </summary>
    [Test]
    public async Task Logout_ShouldSignOutAndRedirect()
    {
        // Act
        var result = await _controller.Logout();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Public");
        _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }

    /// <summary>
    /// PRUEBA: AccessDenied muestra la vista.
    /// </summary>
    [Test]
    public void AccessDenied_ShouldReturnView()
    {
        // Act
        var result = _controller.AccessDenied();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }
}
