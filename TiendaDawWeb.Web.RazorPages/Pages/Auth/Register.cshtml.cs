using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Mappers;

namespace TiendaDawWeb.Web.RazorPages.Pages.Auth;

public class RegisterModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<RegisterModel> logger
) : PageModel {
    [BindProperty]
    public RegisterViewModel Input { get; set; } = default!;

    [BindProperty]
    public string Nombre { get; set; } = default!;

    [BindProperty]
    public string Apellidos { get; set; } = default!;

    [BindProperty]
    public string Email { get; set; } = default!;

    [BindProperty]
    public string Password { get; set; } = default!;

    [BindProperty]
    public string ConfirmPassword { get; set; } = default!;

    [BindProperty]
    public string? Avatar { get; set; }

    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid)
            return Page();

        if (Input == null) {
            Input = new RegisterViewModel {
                Nombre = Nombre,
                Apellidos = Apellidos,
                Email = Email,
                Password = Password,
                ConfirmPassword = ConfirmPassword,
                Avatar = Avatar
            };
        }

        var existingUser = await userManager.FindByEmailAsync(Input.Email);
        if (existingUser != null) {
            ModelState.AddModelError(string.Empty, "Ya existe un usuario con este email");
            return Page();
        }

        var user = Input.ToEntity();

        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded) {
            logger.LogInformation("Nuevo usuario registrado: {Email}", Input.Email);
            await signInManager.SignInAsync(user, false);
            return RedirectToPage("/Public/Index");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}
