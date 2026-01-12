using Microsoft.Playwright;

namespace TiendaDawWeb.Tests.E2E.Extensions;

/// <summary>
///     Extensiones para Playwright.
///     Contiene métodos de ayuda para localizar elementos por su TestId.
/// </summary>
public static class PlaywrightExtensions {
    /// <summary>
    ///     Busca un elemento por su atributo data-testid en la página
    /// </summary>
    public static ILocator TestId(this IPage page, string id) {
        return page.GetByTestId(id);
    }

    // <summary>
    /// Busca un elemento por su atributo data-testid dentro de un locator
    /// </summary>
    public static ILocator TestId(this ILocator locator, string id) {
        return locator.GetByTestId(id);
    }
}