#nullable disable
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.RazorPages.Pages.Admin;

namespace TiendaDawWeb.Tests.RazorPages.Admin;

public class AdminIndexModelTests
{
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
}

public class AdminPageModelTests
{
    [Test]
    public void AdminBasePageModel_CanBeInstantiated()
    {
        typeof(IndexModel).Should().BeDerivedFrom<PageModel>();
    }
}
