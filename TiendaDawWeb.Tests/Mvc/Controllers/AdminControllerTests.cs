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
    [Test]
    public void Constructor_CreatesInstance()
    {
        var mockContext = new Mock<ApplicationDbContext>(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>());
        var userStoreMock = new Mock<IUserStore<User>>();
        var mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<long>>>();
        var mockRoleManager = new Mock<RoleManager<IdentityRole<long>>>(roleStoreMock.Object, null!, null!, null!, null!);
        var mockPurchaseService = new Mock<IPurchaseService>();
        var mockProductService = new Mock<IProductService>();
        var mockLogger = new Mock<ILogger<AdminController>>();

        var controller = new AdminController(
            mockContext.Object,
            mockUserManager.Object,
            mockRoleManager.Object,
            mockPurchaseService.Object,
            mockProductService.Object,
            mockLogger.Object);

        controller.Should().NotBeNull();
    }

    [Test]
    public void AdminController_InheritsFromController()
    {
        typeof(AdminController).Should().BeDerivedFrom<Microsoft.AspNetCore.Mvc.Controller>();
    }

    [Test]
    public void AdminController_HasAdminAuthorizeAttribute()
    {
        var attributes = typeof(AdminController).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        attributes.Should().NotBeEmpty();
        var authAttr = attributes.First() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        authAttr!.Roles.Should().Contain("ADMIN");
    }

    [Test]
    public void AdminController_HasAdminRouteAttribute()
    {
        var controllerType = typeof(AdminController);
        var routeAttributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), true);
        routeAttributes.Should().NotBeEmpty();
        var routeAttr = routeAttributes.First() as Microsoft.AspNetCore.Mvc.RouteAttribute;
        routeAttr!.Template.Should().Be("admin");
    }
}
