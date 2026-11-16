using Microsoft.AspNetCore.Razor.TagHelpers;
using TiendaDawWeb.Shared.Helpers;

namespace TiendaDawWeb.RazorPages.TagHelpers;

/// <summary>
///     Tag Helper para formatear precios con la moneda de la cultura actual
///     Uso: <price value="199.99"></price> o <price value="199.99" />
/// </summary>
[HtmlTargetElement("price", Attributes = "value")]
public class PriceTagHelper : TagHelper {
    [HtmlAttributeName("value")] public decimal Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output) {
        output.TagName = "span";
        output.Attributes.SetAttribute("class", "price");
        output.Attributes.SetAttribute("data-testid", "product-price");

        // Moneda de la cultura de la petición (cookie de idioma) vía PriceFormatter:
        // es-ES → "1.199,99 €", en-US → "$1,199.00". Antes iba fijo a es-ES.
        output.Content.SetContent(PriceFormatter.FormatPrice(Value));
    }
}
