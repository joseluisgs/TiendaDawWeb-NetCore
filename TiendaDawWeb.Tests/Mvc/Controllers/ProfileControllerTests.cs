using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class ProfileControllerTests {
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IStorageService> _mockStorageService;
    private readonly Mock<ILogger<ProfileController>> _mockLogger;
    private readonly ProfileController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public ProfileControllerTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
            new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!, null!);

        _mockContext = new Mock<ApplicationDbContext>(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>());
        _mockStorageService = new Mock<IStorageService>();
        _mockLogger = new Mock<ILogger<ProfileController>>();

        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new ProfileController(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockContext.Object,
            _mockStorageService.Object,
            _mockLogger.Object) {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
        _controller.Dispose();
    }

    #region Index Tests

    [Test]
    public async Task Index_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Index();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Edit (GET) Tests

    [Test]
    public async Task Edit_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Edit();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task Edit_ReturnsView_WhenUserFound() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);

        var result = await _controller.Edit();

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().Be(user);
    }

    #endregion

    #region ChangePassword (GET) Tests

    [Test]
    public void ChangePassword_ReturnsView() {
        var result = _controller.ChangePassword();

        result.Should().BeOfType<ViewResult>();
    }

    #endregion

    #region Edit (POST) Tests

    [Test]
    public async Task EditPost_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Edit("John", "Doe", null);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Passkeys Tests

    [Test]
    public async Task Passkeys_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Passkeys();

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    [Test]
    public async Task Passkeys_ReturnsViewWithList_WhenUserFound() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.GetPasskeysAsync(user))
            .ReturnsAsync(new List<UserPasskeyInfo>());

        var result = await _controller.Passkeys();

        result.Should().BeOfType<ViewResult>();
        ((ViewResult)result).Model.Should().BeOfType<List<UserPasskeyInfo>>().Which.Should().BeEmpty();
    }

    [Test]
    public async Task PasskeyCreationOptions_ReturnsNotFound_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.PasskeyCreationOptions();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task PasskeyCreationOptions_ReturnsJson_WhenUserFound() {
        var user = new User { Id = 1, UserName = "testuser", Email = "test@test.com" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.GetUserIdAsync(user)).ReturnsAsync("1");
        _mockSignInManager.Setup(s => s.MakePasskeyCreationOptionsAsync(It.IsAny<PasskeyUserEntity>()))
            .ReturnsAsync("{}");

        var result = await _controller.PasskeyCreationOptions();

        result.Should().BeOfType<ContentResult>();
        ((ContentResult)result).ContentType.Should().Be("application/json");
    }

    [Test]
    public async Task PasskeyEliminar_ReturnsRedirectWithError_WhenIdNotBase64() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);

        // En runtime el framework inyecta el TempData; en unit test hay que ponerlo a mano.
        _controller.TempData = CrearTempData(_controller.HttpContext);

        var result = await _controller.PasskeyEliminar("no-es-base64!!");

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be("Passkeys");
        _controller.TempData["Error"].Should().Be("El identificador de la passkey no es válido");
    }

    [Test]
    public async Task PasskeyEliminar_RemovesPasskey_WhenIdValid() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.RemovePasskeyAsync(user, It.IsAny<byte[]>()))
            .ReturnsAsync(IdentityResult.Success);

        _controller.TempData = CrearTempData(_controller.HttpContext);

        // Id en base64url de 8 bytes: "abcdefgh" -> 8 bytes válidos
        var result = await _controller.PasskeyEliminar("YWJjZGVmZ2g");

        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be("Passkeys");
        _controller.TempData["Success"].Should().Be("Passkey eliminada");
    }

    #endregion

    /// <summary>
    ///     TempData de prueba: en runtime lo inyecta el framework con su proveedor.
    /// </summary>
    private static TempDataDictionary CrearTempData(HttpContext httpContext) {
        var provider = new Mock<ITempDataProvider>();
        provider.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object?>());
        return new TempDataDictionary(httpContext, provider.Object);
    }
}
