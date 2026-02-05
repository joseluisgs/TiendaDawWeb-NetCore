#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Admin;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.ViewModels;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Tests.RazorPages.Admin;

public class AdminIndexModelTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly ClaimsPrincipal _adminUser;

    public AdminIndexModelTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminIndexModel_HasViewModelProperty()
    {
        var model = new IndexModel(null!);
        model.ViewModel.Should().NotBeNull();
    }

    [Test]
    public void AdminIndexModel_ViewModel_HasDefaultValues()
    {
        var model = new IndexModel(null!);
        model.ViewModel.TotalUsuarios.Should().Be(0);
        model.ViewModel.TotalProductos.Should().Be(0);
        model.ViewModel.TotalCompras.Should().Be(0);
    }
}

public class AdminUsuariosModelTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly ClaimsPrincipal _adminUser;

    public AdminUsuariosModelTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminUsuariosModel_CanBeInstantiated()
    {
        var model = new UsuariosModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminUsuariosModel_HasUsuariosProperty()
    {
        var model = new UsuariosModel(null!);
        model.Usuarios.Should().NotBeNull();
    }
}

public class AdminProductosModelTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly ClaimsPrincipal _adminUser;

    public AdminProductosModelTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        _mockProductService = new Mock<IProductService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminProductosModel_CanBeInstantiated()
    {
        var model = new ProductosModel(null!, null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminProductosModel_HasProductosProperty()
    {
        var model = new ProductosModel(null!, null!, null!);
        model.Productos.Should().NotBeNull();
    }
}

public class AdminComprasModelTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly ClaimsPrincipal _adminUser;

    public AdminComprasModelTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminComprasModel_CanBeInstantiated()
    {
        var model = new ComprasModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminComprasModel_HasComprasProperty()
    {
        var model = new ComprasModel(null!);
        model.Compras.Should().NotBeNull();
    }
}

public class AdminVentasModelTests
{
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly ClaimsPrincipal _adminUser;

    public AdminVentasModelTests()
    {
        _mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
        
        _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "ADMIN")
        }, "TestAuthType"));
    }

    [Test]
    public void AdminVentasModel_CanBeInstantiated()
    {
        var model = new VentasModel(null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void AdminVentasModel_HasVentasProperty()
    {
        var model = new VentasModel(null!);
        model.Ventas.Should().NotBeNull();
    }
}
