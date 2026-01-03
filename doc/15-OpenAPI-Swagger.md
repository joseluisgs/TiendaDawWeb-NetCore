# 15 - Documentación de API: OpenAPI y Swagger

En el desarrollo profesional, no basta con crear una API; hay que documentarla para que otros desarrolladores (o robots) sepan cómo usarla. En este volumen activamos **Swagger** para WalaDaw.

---

## 1. ¿Qué es Swagger / OpenAPI?

- **OpenAPI**: Es el estándar (la especificación) para describir APIs RESTful.
- **Swagger**: Es el conjunto de herramientas que utiliza esa especificación para generar una interfaz visual interactiva.

---

## 2. Por qué es vital para el Alumno

Tradicionalmente, para probar un endpoint como `/api/ratings`, tendrías que:
1.  Abrir una herramienta externa como Postman o Insomnia.
2.  Configurar la URL, el método (POST/GET) y el cuerpo JSON.
3.  Gestionar las cookies de autenticación.

**Con Swagger**, todo esto ocurre dentro del propio navegador en la ruta `/swagger`.

---

## 3. Implementación en WalaDaw

### Registro del generador (`Program.cs`)
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "WalaDaw API", 
        Version = "v1" 
    });
});
```

### Activación de la UI
Solo lo activamos en modo **Desarrollo** por seguridad:
```csharp
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

---

## 4. Cómo probar la API

1.  Inicia la aplicación (`dotnet watch`).
2.  Navega a: `http://localhost:5000/swagger`.
3.  Verás todos los controladores marcados como `[ApiController]` (como `FavoriteApiController` y `RatingApiController`).
4.  Haz clic en **"Try it out"**, rellena los datos y pulsa **"Execute"**. Verás la respuesta JSON real del servidor.

---

## 5. Conclusión

Swagger convierte nuestra aplicación en un sistema auto-documentado. Es la herramienta definitiva para depurar errores en las llamadas asíncronas (AJAX) antes incluso de escribir el código de JavaScript.
