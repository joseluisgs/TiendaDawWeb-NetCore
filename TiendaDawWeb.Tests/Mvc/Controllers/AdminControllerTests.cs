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
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class AdminControllerTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<RoleManager<IdentityRole<long>>> _mockRoleManager;
    private readonly Mock<IPurchaseService> _mockPurchaseService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ILogger<AdminController>> _mockLogger;
    private readonly AdminController _controller;
    private readonly ClaimsPrincipal _adminPrincipal;

    public AdminControllerTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>());
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<long>>>();
        _mockRoleManager = new Mock<RoleManager<IdentityRole<long>>>(roleStoreMock.Object, null!, null!, null!, null!);
        
        _mockPurchaseService = new Mock<IPurchaseService>();
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<AdminController>>();
        
        _adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _adminPrincipal };
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor);

        _controller = new AdminController(
            _mockContext.Object,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockPurchaseService.Object,
            _mockProductService.Object,
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

    [Test]
    public void Constructor_CreatesInstance()
    {
        _controller.Should().NotBeNull();
    }
}
