using Microsoft.AspNetCore.Razor.TagHelpers;

namespace TiendaDawWeb.Mvc.TagHelpers;

/// <summary>
///     Tag Helper para formatear precios con símbolo de euro
///     Uso: <price value="199.99"></price> o <price value="199.99" />
/// </summary>
[HtmlTargetElement("price", Attributes = "value")]
public class PriceTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Attributes.SetAttribute("class", "price");
        output.Attributes.SetAttribute("data-testid", "product-price");
        
        var formattedPrice = Value.ToString("C2", new System.Globalization.CultureInfo("es-ES"));
        output.Content.SetContent(formattedPrice);
    }
}
