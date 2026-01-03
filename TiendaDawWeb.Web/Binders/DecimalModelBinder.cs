using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace TiendaDawWeb.Binders;

public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        // 🟢 SOPORTAR AMBOS: Normalizar coma a punto para ser robustos en entornos localizados
        if (value.Contains(','))
        {
            value = value.Replace(',', '.');
        }

        if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, 
            CultureInfo.InvariantCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            "El precio debe ser un número válido con punto como separador. Ejemplo: 19.99");

        return Task.CompletedTask;
    }
}
