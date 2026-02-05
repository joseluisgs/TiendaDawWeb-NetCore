#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Security.Claims;
using TiendaDawWeb.RazorPages.Pages.Profile;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Tests.RazorPages.Profile;

public class ProfileIndexModelTests
{
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly Mock<ApplicationDbContext> _mockContext;
    private readonly IndexModel _model;
    private readonly ClaimsPrincipal _userPrincipal;

    public ProfileIndexModelTests()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        _mockUserManager = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        
        _mockContext = new Mock<ApplicationDbContext>(new Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext>());
        
        _userPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }, "mock"));

        var httpContext = new DefaultHttpContext { User = _userPrincipal };
        var pageContext = new PageContext { HttpContext = httpContext };

        _model = new IndexModel(
            _mockUserManager.Object,
            _mockContext.Object)
        {
            PageContext = pageContext
        };
    }

    [Test]
    public void ProfileIndexModel_CanBeInstantiated()
    {
        var model = new IndexModel(null!, null!);
        model.Should().NotBeNull();
    }

    [Test]
    public void ProfileIndexModel_HasUserProfileProperty()
    {
        var model = new IndexModel(null!, null!);
        model.UserProfile.Should().Be(default(User));
    }

    [Test]
    public void ProfileIndexModel_UsuarioProperty_ReturnsUserProfile()
    {
        var model = new IndexModel(null!, null!);
        model.Usuario.Should().BeSameAs(model.UserProfile);
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
}
