#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Tests.RazorPages.Auth;

public class AccessDeniedModelTests
{
    [Test]
    public void OnGet_DoesNotThrow()
    {
        var model = new TiendaDawWeb.RazorPages.Pages.Auth.AccessDeniedModel();
        var act = () => model.OnGet();
        act.Should().NotThrow();
    }

    [Test]
    public void AccessDeniedModel_CanBeInstantiated()
    {
        var model = new TiendaDawWeb.RazorPages.Pages.Auth.AccessDeniedModel();
        model.Should().NotBeNull();
    }

    [Test]
    public void AccessDeniedModel_HasCorrectAttributes()
    {
        typeof(TiendaDawWeb.RazorPages.Pages.Auth.AccessDeniedModel).Should().BeDerivedFrom<PageModel>();
    }
}

public class LogoutModelTests
{
    [Test]
    public void LogoutModel_HasCorrectAttributes()
    {
        typeof(TiendaDawWeb.Web.RazorPages.Pages.Auth.LogoutModel).Should().BeDerivedFrom<PageModel>();
    }

    [Test]
    public void LogoutModel_HasOnGetAsyncMethod()
    {
        var method = typeof(TiendaDawWeb.Web.RazorPages.Pages.Auth.LogoutModel)
            .GetMethod("OnGetAsync");
        method.Should().NotBeNull();
    }

    [Test]
    public void LogoutModel_HasOnPostAsyncMethod()
    {
        var method = typeof(TiendaDawWeb.Web.RazorPages.Pages.Auth.LogoutModel)
            .GetMethod("OnPostAsync");
        method.Should().NotBeNull();
    }
}
