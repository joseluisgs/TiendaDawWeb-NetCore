#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Purchase;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.RazorPages.Purchase;

public class PurchaseIndexModelTests
{
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly IndexModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public PurchaseIndexModelTests()
    {
        _mockPurchaseService = new Mock<IPurchaseService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new IndexModel(
            _mockPurchaseService.Object,
            _mockUserManager.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void PurchaseIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void PurchaseIndexModel_HasPurchasesProperty()
    {
        var model = new IndexModel(null!, null!);
        model.Purchases.Should().NotBeNull();
    }

    [Test]
    public async Task OnGetAsync_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)!.PageName.Should().Be("/Auth/Login");
    }
}

public class PurchaseConfirmacionModelTests
{
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly ClaimsPrincipal _authenticatedUser;

    public PurchaseConfirmacionModelTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        _mockPurchaseService = new Mock<IPurchaseService>();

        _authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "TestAuthType"));
    }

    [Test]
    public void ConfirmacionModel_CanBeInstantiated()
    {
        var model = new ConfirmacionModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ConfirmacionModel_HasPurchaseProperty()
    {
        var model = new ConfirmacionModel(null!, null!);
        model.Purchase.Should().Be(default(PurchaseModel));
    }

    [Test]
    public async Task OnGetAsync_RedirectsToLogin_WhenUserNotAuthenticated()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((User)null!);

        var model = new ConfirmacionModel(_mockPurchaseService.Object, _mockUserManager.Object);
        var result = await model.OnGetAsync(1);

        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)!.PageName.Should().Be("/Auth/Login");
    }
}
