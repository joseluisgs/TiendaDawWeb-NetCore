using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.Web.RazorPages.Pages.Auth;

public class LoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginModel> logger
) : PageModel {
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.FindByEmailAsync(Email);
        if (user == null) {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            Password,
            RememberMe,
            false);

        if (result.Succeeded) {
            logger.LogInformation("Usuario {Email} inició sesión", Email);
            return RedirectToLocal(ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
        return Page();
    }

    private IActionResult RedirectToLocal(string? returnUrl) {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToPage("/Public/Index");
    }
}
