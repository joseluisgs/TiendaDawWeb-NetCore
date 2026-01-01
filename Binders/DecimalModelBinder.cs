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

        // Validar que solo hay un separador decimal (coma o punto)
        var commaCount = value.Count(c => c == ',');
        var dotCount = value.Count(c => c == '.');
        
        if (commaCount + dotCount > 1)
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"El valor '{value}' no es un número decimal válido.");
            return Task.CompletedTask;
        }

        // Intentar parsear con cultura actual (español: coma)
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out var result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        // Intentar parsear con cultura invariante (inglés: punto)
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        // Intentar reemplazar coma por punto y parsear
        var normalized = value.Replace(',', '.');
        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            bindingContext.Result = ModelBindingResult.Success(result);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"El valor '{value}' no es un número decimal válido.");

        return Task.CompletedTask;
    }
}
