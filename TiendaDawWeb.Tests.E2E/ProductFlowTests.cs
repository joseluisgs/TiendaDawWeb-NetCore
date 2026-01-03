using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE FLUJO DE CATÁLOGO (E2E)
 * 
 * OBJETIVO: Asegurar que el usuario puede encontrar productos mediante el buscador.
 * TECNOLOGÍAS TESTEADAS: Motores de búsqueda (q), Vistas Razor, Rutas MVC.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductFlowTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Test: Búsqueda. Debe encontrar un iPhone y entrar en su ficha técnica.
    /// </summary>
    [Test]
    public async Task Search_ShouldFindIphoneAndNavigateToDetails()
    {
        // 1. Acción: Ir al escaparate público
        await Page.GotoAsync($"{BaseUrl}/Public");

        // 2. Acción: Buscar 'iPhone' en el campo central
        var searchInput = Page.Locator("main input[name='q']");
        await searchInput.FillAsync("iPhone");
        await Page.ClickAsync("main button[type='submit']");

        // 3. Verificación: Los resultados contienen el texto buscado
        var firstResultTitle = Page.Locator(".card-title").First;
        await Expect(firstResultTitle).ToContainTextAsync(new System.Text.RegularExpressions.Regex("iPhone", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        // 4. Acción: Navegar al detalle
        await firstResultTitle.ClickAsync();

        // 5. Verificación: URL de detalles y Título H1 visible
        await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex(".*/Product/Details/.*"));
        var h1 = Page.Locator("h1");
        await Expect(h1).ToBeVisibleAsync();
        await Expect(h1).ToContainTextAsync(new System.Text.RegularExpressions.Regex("iPhone", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }
}
