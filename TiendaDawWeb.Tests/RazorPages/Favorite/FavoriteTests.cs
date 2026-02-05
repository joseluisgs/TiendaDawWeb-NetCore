#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Favorite;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Tests.RazorPages.Favorite;

public class FavoriteIndexModelTests
{
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly IndexModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public FavoriteIndexModelTests()
    {
        _mockFavoriteService = new Mock<IFavoriteService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new IndexModel(
            _mockFavoriteService.Object,
            _mockUserManager.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void FavoriteIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void FavoriteIndexModel_HasProductsProperty()
    {
        var model = new IndexModel(null!, null!);
        model.Products.Should().NotBeNull();
    }

    [Test]
    public void FavoriteIndexModel_ProductosProperty_ReturnsProducts()
    {
        var model = new IndexModel(null!, null!);
        model.Productos.Should().BeSameAs(model.Products);
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

    [Test]
    public async Task OnGetAsync_ReturnsPage_WhenUserFound()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var products = new List<ProductModel>();
        
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockFavoriteService.Setup(s => s.GetUserFavoritesAsync(user.Id))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<IEnumerable<ProductModel>, TiendaDawWeb.Shared.Errors.DomainError>(products));

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }
}
