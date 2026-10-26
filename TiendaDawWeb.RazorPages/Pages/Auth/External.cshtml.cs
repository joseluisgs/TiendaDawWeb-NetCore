using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Auth;

/// <summary>
///     Login externo OAuth 2.0 (Google, GitHub, Microsoft) para Razor Pages.
///     Redirige al proveedor. El callback se procesa en ExternalCallback.
/// </summary>
[IgnoreAntiforgeryToken]
public class ExternalModel(
    IAuthenticationSchemeProvider schemeProvider,
    ILogger<ExternalModel> logger
) : PageModel
{
    /// <summary>
    ///     Inicia login externo: redirige al proveedor OAuth.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(string provider, string? returnUrl = null) {
        var scheme = await schemeProvider.GetSchemeAsync(provider);
        if (scheme is null) {
            logger.LogWarning("Proveedor OAuth no configurado: {Provider}", provider);
            return NotFound($"Proveedor '{provider}' no está configurado");
        }

        var redirectUrl = Url.Page("/Auth/ExternalCallback", new { returnUrl });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        logger.LogInformation("Iniciando login externo con {Provider}", provider);
        return Challenge(properties, provider);
    }
}
