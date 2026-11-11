#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.RazorPages.Pages.Auth;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Web.RazorPages.Pages.Auth;
using IdentitySignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace TiendaDawWeb.Tests.RazorPages.Auth;

public class AccessDeniedModelTests {
    [Test]
    public void OnGet_DoesNotThrow() {
        var model = new AccessDeniedModel();
        Action act = () => model.OnGet();
        act.Should().NotThrow();
    }

    [Test]
    public void AccessDeniedModel_CanBeInstantiated() {
        var model = new AccessDeniedModel();
        model.Should().NotBeNull();
    }

    [Test]
    public void AccessDeniedModel_HasCorrectAttributes() {
        typeof(AccessDeniedModel).Should().BeDerivedFrom<PageModel>();
    }
}

public class LogoutModelTests {
    private readonly Mock<ILogger<LogoutModel>> _mockLogger;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public LogoutModelTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);
        _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
            new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!,
            null!);
        _mockLogger = new Mock<ILogger<LogoutModel>>();
    }

    [Test]
    public void LogoutModel_CanBeInstantiated() {
        var model = new LogoutModel(_mockSignInManager.Object);
        model.Should().NotBeNull();
    }
}

public class LoginModelTests {
    private readonly Mock<ILogger<LoginModel>> _mockLogger;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public LoginModelTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);
        _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
            new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!,
            null!);
        _mockLogger = new Mock<ILogger<LoginModel>>();
    }

    [Test]
    public void LoginModel_CanBeInstantiated() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Should().NotBeNull();
    }

    [Test]
    public void LoginModel_HasEmailProperty() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Email.Should().BeEmpty();
    }

    [Test]
    public void LoginModel_HasPasswordProperty() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Password.Should().BeEmpty();
    }

    [Test]
    public void LoginModel_HasRememberMeProperty() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.RememberMe.Should().BeFalse();
    }

    [Test]
    public void LoginModel_HasReturnUrlProperty() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.ReturnUrl.Should().BeNull();
    }

    [Test]
    public void OnGet_SetsReturnUrl() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.OnGet("/some-return-url");
        model.ReturnUrl.Should().Be("/some-return-url");
    }

    [Test]
    public void OnGet_SetsReturnUrlToNull_WhenNullProvided() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.OnGet();
        model.ReturnUrl.Should().BeNull();
    }

    [Test]
    public async Task OnPostAsync_ReturnsPage_WhenModelStateInvalid() {
        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.ModelState.AddModelError("Error", "Invalid model");

        IActionResult result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Test]
    public async Task OnPostAsync_ReturnsPage_WhenUserNotFound() {
        _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Email = "test@example.com";
        model.Password = "password";

        IActionResult result = await model.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        model.ModelState.Should().ContainKey(string.Empty);
    }

    [Test]
    public async Task OnPostPasskeyRequestOptionsAsync_ReturnsJson_WhenNoEmail() {
        _mockSignInManager.Setup(s => s.MakePasskeyRequestOptionsAsync(It.IsAny<User>()))
            .ReturnsAsync("{}");

        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        IActionResult result = await model.OnPostPasskeyRequestOptionsAsync(null);

        result.Should().BeOfType<ContentResult>();
        ((ContentResult)result).ContentType.Should().Be("application/json");
    }

    [Test]
    public async Task OnPostPasskeyRequestOptionsAsync_LooksUpUser_WhenEmailProvided() {
        var user = new User { Id = 1, UserName = "test@example.com", Email = "test@example.com" };
        _mockUserManager.Setup(u => u.FindByEmailAsync("test@example.com")).ReturnsAsync(user);
        _mockSignInManager.Setup(s => s.MakePasskeyRequestOptionsAsync(user))
            .ReturnsAsync("{}");

        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        IActionResult result = await model.OnPostPasskeyRequestOptionsAsync("test@example.com");

        result.Should().BeOfType<ContentResult>();
        _mockSignInManager.Verify(s => s.MakePasskeyRequestOptionsAsync(user), Times.Once);
    }

    [Test]
    public async Task OnPostPasskeyAssertionAsync_ReturnsUnauthorized_WhenSignatureInvalid() {
        _mockSignInManager.Setup(s => s.PasskeySignInAsync(It.IsAny<string>()))
            .ReturnsAsync(IdentitySignInResult.Failed);

        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        IActionResult result = await model.OnPostPasskeyAssertionAsync("credencial-mala");

        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Test]
    public async Task OnPostPasskeyAssertionAsync_ReturnsJsonWithDestino_WhenSignatureValid() {
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };

        _mockSignInManager.Setup(s => s.PasskeySignInAsync(It.IsAny<string>()))
            .ReturnsAsync(IdentitySignInResult.Success);

        var model = new LoginModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object) {
            PageContext = pageContext
        };

        // En runtime el framework inyecta el IUrlHelper; en unit test se mockea.
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper.Setup(u => u.IsLocalUrl("/carrito")).Returns(true);
        model.Url = urlHelper.Object;

        IActionResult result = await model.OnPostPasskeyAssertionAsync("credencial-valida", "/carrito");

        JsonResult json = result.Should().BeOfType<JsonResult>().Subject;
        json.Value!.ToString().Should().Contain("destino = /carrito");
    }
}

public class RegisterModelTests {
    private readonly Mock<ILogger<RegisterModel>> _mockLogger;
    private readonly Mock<SignInManager<User>> _mockSignInManager;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public RegisterModelTests() {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);
        _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object,
            new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<User>>().Object, null!, null!, null!,
            null!);
        _mockLogger = new Mock<ILogger<RegisterModel>>();
    }

    [Test]
    public void RegisterModel_CanBeInstantiated() {
        var model = new RegisterModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Should().NotBeNull();
    }

    [Test]
    public void RegisterModel_HasInputProperty() {
        var model = new RegisterModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Input.Should().BeNull();
    }

    [Test]
    public void RegisterModel_HasNombreProperty() {
        var model = new RegisterModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Nombre.Should().BeNull();
    }

    [Test]
    public void RegisterModel_HasPasswordProperty() {
        var model = new RegisterModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.Password.Should().BeNull();
    }

    [Test]
    public void RegisterModel_HasConfirmPasswordProperty() {
        var model = new RegisterModel(_mockUserManager.Object, _mockSignInManager.Object, _mockLogger.Object);
        model.ConfirmPassword.Should().BeNull();
    }
}
