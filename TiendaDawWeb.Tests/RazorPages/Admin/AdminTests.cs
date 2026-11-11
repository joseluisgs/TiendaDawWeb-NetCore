#nullable disable
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using TiendaDawWeb.RazorPages.Pages.Admin;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;

namespace TiendaDawWeb.Tests.RazorPages.Admin;

public class AdminIndexModelTests {
    private readonly ClaimsPrincipal _adminUser;
    private readonly Mock<ApplicationDbContext> _mockContext;

    public AdminIndexModelTests() {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminIndexModel_CanBeInstantiated() {
        var model = new IndexModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminIndexModel_HasViewModelProperty() {
        var model = new IndexModel(null!);
        model.ViewModel.Should().NotBeNull();
    }

    [Test]
    public void AdminIndexModel_ViewModel_HasDefaultValues() {
        var model = new IndexModel(null!);
        model.ViewModel.TotalUsuarios.Should().Be(0);
        model.ViewModel.TotalProductos.Should().Be(0);
        model.ViewModel.TotalCompras.Should().Be(0);
    }
}

public class AdminUsuariosModelTests {
    private readonly ClaimsPrincipal _adminUser;
    private readonly Mock<ApplicationDbContext> _mockContext;

    public AdminUsuariosModelTests() {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminUsuariosModel_CanBeInstantiated() {
        var model = new UsuariosModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminUsuariosModel_HasUsuariosProperty() {
        var model = new UsuariosModel(null!);
        model.Usuarios.Should().NotBeNull();
    }
}

public class AdminProductosModelTests {
    private readonly ClaimsPrincipal _adminUser;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<UserManager<User>> _mockUserManager;

    public AdminProductosModelTests() {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockProductService = new Mock<IProductService>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!,
            null!, null!);

        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminProductosModel_CanBeInstantiated() {
        var model = new ProductosModel(null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminProductosModel_HasProductosProperty() {
        var model = new ProductosModel(null!, null!, null!);
        model.Productos.Should().NotBeNull();
    }
}

public class AdminComprasModelTests {
    private readonly ClaimsPrincipal _adminUser;
    private readonly Mock<ApplicationDbContext> _mockContext;

    public AdminComprasModelTests() {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminComprasModel_CanBeInstantiated() {
        var model = new ComprasModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminComprasModel_HasComprasProperty() {
        var model = new ComprasModel(null!);
        model.Compras.Should().NotBeNull();
    }
}

public class AdminVentasModelTests {
    private readonly ClaimsPrincipal _adminUser;
    private readonly Mock<ApplicationDbContext> _mockContext;

    public AdminVentasModelTests() {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminVentasModel_CanBeInstantiated() {
        var model = new VentasModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminVentasModel_HasVentasProperty() {
        var model = new VentasModel(null!);
        model.Ventas.Should().NotBeNull();
    }
}
