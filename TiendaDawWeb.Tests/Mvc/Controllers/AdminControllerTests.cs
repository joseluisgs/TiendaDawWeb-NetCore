#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Controllers;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Tests.Mvc.Controllers;

public class AdminControllerTests {
    [Test]
    public void Constructor_CreatesInstance() {
        var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        var userStoreMock = new Mock<IUserStore<User>>();
        var mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!,
            null!, null!, null!);
        var roleStoreMock = new Mock<IRoleStore<IdentityRole<long>>>();
        var mockRoleManager =
            new Mock<RoleManager<IdentityRole<long>>>(roleStoreMock.Object, null!, null!, null!, null!);
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
    public void AdminController_InheritsFromController() {
        typeof(AdminController).Should().BeDerivedFrom<Controller>();
    }

    [Test]
    public void AdminController_HasAdminAuthorizeAttribute() {
        object[] attributes = typeof(AdminController).GetCustomAttributes(typeof(AuthorizeAttribute), true);
        attributes.Should().NotBeEmpty();
        var authAttr = attributes.First() as AuthorizeAttribute;
        authAttr!.Roles.Should().Contain("ADMIN");
    }

    [Test]
    public void AdminController_HasAdminRouteAttribute() {
        Type controllerType = typeof(AdminController);
        object[] routeAttributes = controllerType.GetCustomAttributes(typeof(RouteAttribute), true);
        routeAttributes.Should().NotBeEmpty();
        var routeAttr = routeAttributes.First() as RouteAttribute;
        routeAttr!.Template.Should().Be("admin");
    }
}
