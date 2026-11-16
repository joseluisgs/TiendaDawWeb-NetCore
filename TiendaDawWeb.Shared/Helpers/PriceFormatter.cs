namespace TiendaDawWeb.Shared.Helpers;

/// <summary>
///     Helper para formatear precios con la moneda de la cultura actual
///     (es-ES → "1.199,99 €", en-US → "$1,199.00")
/// </summary>
public static class PriceFormatter {
    /// <summary>
    ///     Formatea un precio con la moneda de la cultura actual (CurrentCulture).
    ///     Ejemplos: es-ES → 1999.99 -> "1.999,99 €" · en-US → "$1,999.99"
    /// </summary>
    public static string FormatPrice(decimal price) {
        // Tomamos la cultura por defecto que hay en el sistema (cookie de idioma)
        CultureInfo culture = CultureInfo.CurrentCulture;
        return price.ToString("C2", culture); // C2 = Currency con 2 decimales
    }

    /// <summary>
    ///     Formatea un precio con separador de miles y símbolo € EXPLÍCITO:
    ///     NO sigue la cultura (euro fijo). Solo donde se requiera euro siempre.
    ///     Ejemplo: 1999.99 -> "1.999,99 €"
    /// </summary>
    public static string FormatPriceWithSymbol(decimal price) {
        return $"{price:N2} €"; // N2 = Number con 2 decimales + símbolo €
    }
}
