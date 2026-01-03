# 17 - Patrón de Mapeo de Objetos: Clean Controllers

En este volumen aprendemos a desacoplar la lógica de presentación de la lógica de dominio mediante el uso de **Mapeadores (Mappers)**.

---

## 1. El Problema: Controladores "Sucios"

Cuando un controlador tiene que convertir manualmente una entidad de base de datos (`Product`) a un objeto para la vista (`ProductViewModel`), acaba lleno de código repetitivo como este:

```csharp
var vm = new ProductViewModel {
    Nombre = p.Nombre,
    Precio = p.Precio,
    // ... así con 15 campos más
};
```

Este enfoque rompe el principio **DRY (Don't Repeat Yourself)** y hace que los controladores sean difíciles de leer y mantener.

---

## 2. La Solución: Métodos de Extensión

Hemos implementado un **Mapeador Manual** usando métodos de extensión de C#. Esto permite "añadir" funcionalidades a nuestras clases sin tocarlas directamente.

### El Mapeador (`Mappers/ProductMapper.cs`)
```csharp
public static class ProductMapper {
    public static ProductViewModel ToViewModel(this Product p) {
        return new ProductViewModel { ... };
    }
}
```

### El Uso en el Controlador
Ahora, pasar de una entidad a un ViewModel es tan sencillo como:
```csharp
return View(product.ToViewModel());
```

---

## 3. Por qué no usar AutoMapper (Senior Tip)

Aunque existen librerías como AutoMapper que hacen esto automáticamente por reflexión, en este proyecto hemos optado por el mapeo manual por tres razones:
1.  **Rendimiento**: El mapeo manual es el más rápido posible en .NET (sin overhead de reflexión).
2.  **Depuración**: Si un campo no se mapea, el compilador te avisa. Con librerías automáticas, el error solo se ve al ejecutar (Runtime).
3.  **Aprendizaje**: Permite al alumno tener control total sobre qué datos viajan a la vista y por qué.

---

## 4. Conclusión

El patrón de mapeo es fundamental para mantener la **Arquitectura en Capas**. El controlador ahora solo se encarga de orquestar (recibir peticiones, llamar a servicios y devolver respuestas), dejando la "fontanería" de los datos a los mappers.
