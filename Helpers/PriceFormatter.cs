using System.Globalization;

namespace TiendaDawWeb.Helpers;

/// <summary>
/// Helper para formatear precios con símbolo de euro
/// </summary>
public static class PriceFormatter
{
    /// <summary>
    /// Formatea un precio usando la cultura española con símbolo €
    /// Ejemplo: 1999.99 -> "1.999,99 €"
    /// </summary>
    public static string FormatPrice(decimal price)
    {
        // Tomamos la cultura por defector que hay en el sistema
        var culture = CultureInfo.CurrentCulture;
        return price.ToString("C2", culture); // C2 = Currency con 2 decimales
    }
    
    /// <summary>
    /// Formatea un precio con separador de miles y símbolo € explícito
    /// Ejemplo: 1999.99 -> "1.999,99 €"
    /// </summary>
    public static string FormatPriceWithSymbol(decimal price)
    {
        return $"{price:N2} €"; // N2 = Number con 2 decimales + símbolo €
    }
}
