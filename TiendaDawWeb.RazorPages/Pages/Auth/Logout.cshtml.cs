using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Web.RazorPages.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly SignInManager<User> signInManager;

    public LogoutModel(SignInManager<User> signInManager)
    {
        this.signInManager = signInManager;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await signInManager.SignOutAsync();
        await HttpContext.SignOutAsync();
        return Redirect("/Public");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await signInManager.SignOutAsync();
        await HttpContext.SignOutAsync();
        return Redirect("/Public");
    }
}
