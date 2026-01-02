# 06. I18n - Globalización y Localización Estratégica

Dominar las culturas no es solo traducir palabras; es entender cómo el mundo escribe los números.

## 1. Archivos de Recursos (.resx)
Usamos el sistema oficial de recursos de .NET.
- `SharedResource.es.resx` -> Versión española.
- `SharedResource.en.resx` -> Versión inglesa.
**Importante**: El nombre del archivo debe coincidir con la clave del diccionario que usas en el código.

## 2. El Separador Decimal (Comas vs Puntos)
En España usamos `10,50`. En USA `10.50`.
**El Problema**: El navegador envía un texto al servidor. Si el servidor espera un punto y recibe una coma, leerá "0" o "1050".
**La Solución**: Hemos implementado un **Custom Model Binder** (`DecimalModelBinder`) que limpia el texto y parsea el número usando la cultura del navegador del usuario.

## 3. Localización de Modelos y Mensajes
Usa el atributo `[Display(Name = "Clave", ResourceType = typeof(MiRecurso))]` en tus clases. .NET buscará esa clave y la traducirá automáticamente en los labels de Razor.

## 4. Rutas Absolutas en el Cambio de Idioma
Debido a la etiqueta `<base>` de Blazor, los enlaces relativos de idioma fallan. 
**La Solución Magistral**:
`href="@(Context.Request.Path)?lang=es"`. Usamos el Path actual de la petición para recargar la misma página con el nuevo parámetro de idioma.
