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
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class RatingControllerTests
{
    private readonly Mock<IRatingService> _mockRatingService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ILogger<RatingController>> _mockLogger;
    private readonly RatingController _controller;
    private readonly ClaimsPrincipal _userPrincipal;

    public RatingControllerTests()
    {
        _mockRatingService = new Mock<IRatingService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockLogger = new Mock<ILogger<RatingController>>();
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new RatingController(
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
        _controller.Dispose();
    }

    #region AddRating Tests

    [Test]
    public async Task AddRating_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var model = new RatingViewModel { ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };

        var result = await _controller.AddRating(model);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Details Tests

    [Test]
    public async Task Details_ReturnsView_WhenRatingFound()
    {
        var rating = new Rating { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };
        _mockRatingService.Setup(s => s.GetByIdAsync(1))
            .Returns(Task.FromResult(Result.Success<Rating, TiendaDawWeb.Shared.Errors.DomainError>(rating)));

        var result = await _controller.Details(1);

        result.Should().BeOfType<ViewResult>();
        (result as ViewResult)!.Model.Should().Be(rating);
    }

    #endregion

    #region Edit Tests

    [Test]
    public async Task Edit_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Edit(1);

        result.Should().BeOfType<RedirectToActionResult>();
    }

    [Test]
    public async Task Edit_ReturnsView_WhenOwner()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var rating = new Rating { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync(user);
        _mockRatingService.Setup(s => s.GetByIdAsync(1))
            .Returns(Task.FromResult(Result.Success<Rating, TiendaDawWeb.Shared.Errors.DomainError>(rating)));

        var result = await _controller.Edit(1);

        result.Should().BeOfType<ViewResult>();
        var viewModel = (result as ViewResult)!.Model as RatingViewModel;
        viewModel!.Puntuacion.Should().Be(5);
    }

    #endregion

    #region Edit (POST) Tests

    [Test]
    public async Task EditPost_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Edit(1, new RatingViewModel { ProductoId = 1, Puntuacion = 4 });

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion

    #region Delete Tests

    [Test]
    public async Task Delete_RedirectsToLogin_WhenUserNotFound()
    {
        _mockUserManager.Setup(u => u.GetUserAsync(_userPrincipal))
            .ReturnsAsync((User)null!);

        var result = await _controller.Delete(1, 1);

        result.Should().BeOfType<RedirectToActionResult>();
        (result as RedirectToActionResult)!.ActionName.Should().Be("Login");
    }

    #endregion
}
