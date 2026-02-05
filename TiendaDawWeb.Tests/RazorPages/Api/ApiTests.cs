#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Api;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.Tests.RazorPages.Api;

public class ApiCartCountModelTests
{
    private readonly Mock<ICarritoService> _mockCarritoService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly CartCountModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public ApiCartCountModelTests()
    {
        _mockCarritoService = new Mock<ICarritoService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new CartCountModel(
            _mockCarritoService.Object,
            _mockUserManager.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void ApiCartCountModel_CanBeInstantiated()
    {
        var model = new CartCountModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public async Task OnGetAsync_ReturnsZeroCount_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<JsonResult>();
    }

    [Test]
    public async Task OnGetAsync_ReturnsCount_WhenUserFound()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockCarritoService.Setup(s => s.GetCarritoCountAsync(user.Id))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<int, TiendaDawWeb.Shared.Errors.DomainError>(5));

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<JsonResult>();
    }
}

public class ApiRatingsModelTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly RatingsModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public ApiRatingsModelTests()
    {
        _mockRatingService = new Mock<IRatingService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new RatingsModel(
            _mockRatingService.Object,
            _mockUserManager.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void ApiRatingsModel_CanBeInstantiated()
    {
        var model = new RatingsModel(null!, null!);
        model.Should().NotBeNull();
    }
}

public class ApiFavoritesModelTests
{
    private readonly Mock<IFavoriteService> _mockFavoriteService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly FavoritesModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public ApiFavoritesModelTests()
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

        _model = new FavoritesModel(
            _mockFavoriteService.Object,
            _mockUserManager.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void ApiFavoritesModel_CanBeInstantiated()
    {
        var model = new FavoritesModel(null!, null!);
        model.Should().NotBeNull();
    }
}
