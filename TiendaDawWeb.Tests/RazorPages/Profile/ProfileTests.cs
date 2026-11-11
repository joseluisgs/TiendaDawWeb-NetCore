#nullable disable
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.RazorPages.Pages.Profile;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Tests.RazorPages.Profile;

public class ProfileIndexModelTests {
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly IndexModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public ProfileIndexModelTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);

        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new IndexModel(
            _mockUserManager.Object,
            _mockContext.Object) {
            PageContext = pageContext
        };
    }

    [Test]
    public void ProfileIndexModel_CanBeInstantiated() {
        var model = new IndexModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProfileIndexModel_HasUserProfileProperty() {
        var model = new IndexModel(null!, null!);
        model.UserProfile.Should().Be(default(User));
    }

    [Test]
    public void ProfileIndexModel_UsuarioProperty_ReturnsUserProfile() {
        var model = new IndexModel(null!, null!);
        model.Usuario.Should().BeSameAs(model.UserProfile);
    }

    [Test]
    public async Task OnGetAsync_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        IActionResult result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)!.PageName.Should().Be("/Auth/Login");
    }
}

public class PasskeysModelTests {
    private readonly Mock<ILogger<PasskeysModel>> _mockLogger;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly PasskeysModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public PasskeysModelTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);
        _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
            new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!,
            null!);
        _mockLogger = new Mock<ILogger<PasskeysModel>>();

        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new PasskeysModel(
            _mockUserManager.Object,
            _mockSignInManager.Object,
            _mockLogger.Object) {
            PageContext = pageContext
        };
    }

    [Test]
    public void PasskeysModel_CanBeInstantiated() {
        var model = new PasskeysModel(null!, null!, null!);
        model.Should().NotBeNull();
        model.Passkeys.Should().BeEmpty();
    }

    [Test]
    public async Task OnGetAsync_RedirectsToLogin_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        IActionResult result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        ((RedirectToPageResult)result).PageName.Should().Be("/Auth/Login");
    }

    [Test]
    public async Task OnGetAsync_ReturnsPageWithPasskeys_WhenUserFound() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.GetPasskeysAsync(user))
            .ReturnsAsync(new List<UserPasskeyInfo>());

        IActionResult result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
        _model.Passkeys.Should().BeEmpty();
    }

    [Test]
    public async Task OnPostCreationOptions_ReturnsNotFound_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        IActionResult result = await _model.OnPostCreationOptionsAsync();

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task OnPostCreationOptions_ReturnsJson_WhenUserFound() {
        var user = new User { Id = 1, UserName = "testuser", Email = "test@test.com" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockUserManager.Setup(u => u.GetUserIdAsync(user)).ReturnsAsync("1");
        _mockSignInManager.Setup(s => s.MakePasskeyCreationOptionsAsync(It.IsAny<PasskeyUserEntity>()))
            .ReturnsAsync("{}");

        IActionResult result = await _model.OnPostCreationOptionsAsync();

        result.Should().BeOfType<ContentResult>();
        ((ContentResult)result).ContentType.Should().Be("application/json");
    }

    [Test]
    public async Task OnPostAttestation_ReturnsBadRequest_WhenCredentialInvalid() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);
        _mockSignInManager.Setup(s => s.PerformPasskeyAttestationAsync(It.IsAny<string>()))
            .ReturnsAsync(PasskeyAttestationResult.Fail(new PasskeyException("reto no válido")));

        IActionResult result = await _model.OnPostAttestationAsync("credencial-mala");

        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task OnPostEliminar_ReturnsRedirectWithError_WhenIdNotBase64() {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal)).ReturnsAsync(user);

        // En runtime el framework inyecta el TempData; en unit test hay que ponerlo a mano.
        _model.TempData = CrearTempData(_model.PageContext.HttpContext);

        IActionResult result = await _model.OnPostEliminarAsync("no-es-base64!!");

        result.Should().BeOfType<RedirectToPageResult>();
        _model.TempData["Error"].Should().Be("El identificador de la passkey no es válido");
    }

    /// <summary>
    ///     TempData de prueba: en runtime lo inyecta el framework con su proveedor.
    /// </summary>
    private static TempDataDictionary CrearTempData(HttpContext httpContext) {
        var provider = new Mock<ITempDataProvider>();
        provider.Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());
        return new TempDataDictionary(httpContext, provider.Object);
    }
}
