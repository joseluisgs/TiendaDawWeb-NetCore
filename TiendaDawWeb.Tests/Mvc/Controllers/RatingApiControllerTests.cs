#nullable disable
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class RatingApiControllerTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<RatingApiController>> _mockLogger;
    private readonly RatingApiController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public RatingApiControllerTests()
    {
        _mockRatingService = new Mock<IRatingService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<RatingApiController>>();
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new RatingApiController(
            _mockRatingService.Object,
            _mockUserManager.Object,
            _mockLogger.Object)
        {
            ControllerContext = new ControllerContext(actionContext)
        };
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
    }

    #region Constructor Tests

    [Test]
    public void Constructor_CreatesInstance()
    {
        _controller.Should().NotBeNull();
    }

    [Test]
    public void RatingApiController_InheritsFromControllerBase()
    {
        typeof(RatingApiController).Should().BeDerivedFrom<ControllerBase>();
    }

    #endregion

    #region GetProductRatings Tests

    [Test]
    public async Task GetProductRatings_ReturnsBadRequest_WhenServiceFails()
    {
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(1))
            .ReturnsAsync(Result.Failure<IEnumerable<Rating>, DomainError>(RatingError.NotFound(1)));

        var result = await _controller.GetProductRatings(1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetProductRatings_ReturnsRatings_WhenSuccessful()
    {
        var ratings = new List<Rating>
        {
            new() { Id = 1, Puntuacion = 5, Comentario = "Great!" }
        };
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(1))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(ratings));

        var result = await _controller.GetProductRatings(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetProductRatings_ReturnsEmptyList_WhenNoRatings()
    {
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(999))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(new List<Rating>()));

        var result = await _controller.GetProductRatings(999);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region GetUserRating Tests

    [Test]
    public async Task GetUserRating_ReturnsUnauthorized_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.GetUserRating(1);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task GetUserRating_ReturnsOk_WhenUserFound_NoRating()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(1))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(new List<Rating>()));

        var result = await _controller.GetUserRating(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task GetUserRating_ReturnsOk_WhenUserFound_WithRating()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var rating = new Rating { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5 };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(1))
            .ReturnsAsync(Result.Success<IEnumerable<Rating>, DomainError>(new List<Rating> { rating }));

        var result = await _controller.GetUserRating(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region AddRating Tests

    [Test]
    public async Task AddRating_ReturnsUnauthorized_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var request = new AddRatingRequest(1, 5, "Great!");
        var result = await _controller.AddRating(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Test]
    public async Task AddRating_ReturnsBadRequest_WhenRatingInvalid()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);

        var request = new AddRatingRequest(1, 6, "Great!");
        var result = await _controller.AddRating(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task AddRating_ReturnsBadRequest_WhenServiceFails()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockRatingService.Setup(s => s.AddRatingAsync(1, 1, 5, "Great!"))
            .ReturnsAsync(Result.Failure<Rating, DomainError>(RatingError.AlreadyRated()));

        var request = new AddRatingRequest(1, 5, "Great!");
        var result = await _controller.AddRating(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task AddRating_ReturnsOk_WhenSuccessful()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var rating = new Rating { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockRatingService.Setup(s => s.AddRatingAsync(1, 1, 5, "Great!"))
            .ReturnsAsync(Result.Success<Rating, DomainError>(rating));

        var request = new AddRatingRequest(1, 5, "Great!");
        var result = await _controller.AddRating(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}
