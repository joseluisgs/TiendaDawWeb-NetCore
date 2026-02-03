using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Profile;

[Authorize]
public class IndexModel(
    UserManager<User> userManager,
    ApplicationDbContext context
) : PageModel {
    public User UserProfile { get; set; } = default!;
    public User Usuario { get { return UserProfile; } }

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        await context.Entry(user)
            .Collection(u => u.Products)
            .LoadAsync();
        await context.Entry(user)
            .Collection(u => u.Purchases)
            .LoadAsync();
        await context.Entry(user)
            .Collection(u => u.Favorites)
            .LoadAsync();

        UserProfile = user;
        return Page();
    }
}
