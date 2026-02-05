#nullable disable
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

    [Test]
    public async Task GetProductRatings_ReturnsBadRequest_WhenServiceFails()
    {
        _mockRatingService.Setup(s => s.GetByProductoIdAsync(1))
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Failure<IEnumerable<Rating>, TiendaDawWeb.Shared.Errors.DomainError>(
                TiendaDawWeb.Shared.Errors.RatingError.NotFound(1)));

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
            .ReturnsAsync(CSharpFunctionalExtensions.Result.Success<IEnumerable<Rating>, TiendaDawWeb.Shared.Errors.DomainError>(ratings));

        var result = await _controller.GetProductRatings(1);

        result.Should().BeOfType<OkObjectResult>();
    }
}
