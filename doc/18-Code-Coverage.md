# 19 - Métricas de Calidad: Cobertura de Código (Code Coverage)

En este volumen aprendemos a medir la efectividad de nuestra suite de pruebas. No basta con tener muchos tests; necesitamos saber qué porcentaje de nuestra lógica de negocio está realmente protegida contra errores.

---

## 1. El Ecosistema de Cobertura en .NET

Para obtener métricas precisas en **WalaDaw**, utilizamos un flujo de trabajo basado en dos herramientas estándar:

1.  **Coverlet**: Un recolector de datos ligero que se ejecuta junto a `dotnet test`. Analiza qué líneas de código se "iluminan" mientras los tests corren.
2.  **ReportGenerator**: Una herramienta que transforma los archivos técnicos XML de Coverlet en un dashboard HTML interactivo y visual.

---

## 2. Cómo generar el Informe de Cobertura

Para obtener el informe unificado que incluya tanto los tests unitarios como los E2E, sigue estos pasos:

### Paso 1: Ejecutar los tests con recolección
```bash
dotnet test --collect:"XPlat Code Coverage"
```
*Esto generará archivos `coverage.cobertura.xml` en las carpetas `TestResults/` de cada proyecto de prueba.*

### Paso 2: Generar el dashboard visual
```bash
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

---

## 3. Interpretación del Informe (Dashboard)

Al abrir `CoverageReport/index.html`, encontrarás las siguientes métricas clave:

### A. Line Coverage (Cobertura de Líneas)
Es el porcentaje de líneas de código ejecutadas por al menos un test. 
- **Verde (> 80%)**: Saludable.
- **Amarillo (50-80%)**: Precaución, hay lógica crítica sin probar.
- **Rojo (< 50%)**: Riesgo alto de regresiones.

### B. Branch Coverage (Cobertura de Ramas)
Mide si has probado todos los caminos posibles de un `if` o `switch`. Es la métrica más honesta, ya que detecta si probaste el caso de éxito pero olvidaste el de error.

### C. Cyclomatic Complexity (Complejidad Ciclomática)
Indica qué tan difícil es de entender y mantener un método. Si un método tiene un número muy alto, es un candidato ideal para ser refactorizado en métodos más pequeños.

---

## 4. Visualización en el Código

Una de las mayores ventajas de esta herramienta es la capacidad de ver el código fuente marcado:
- **Líneas Verdes**: Código "seguro", verificado por tus tests.
- **Líneas Rojas**: Código "ciego", cualquier cambio aquí podría romper la app sin que te enteres.
- **Líneas Naranjas**: Ramas parciales (ej. se probó el `if` pero no el `else`).

---

## 5. Conclusión del Experto

La cobertura de código no es un objetivo de "llegar al 100%", sino una herramienta de diagnóstico. Úsala para identificar las zonas oscuras de tu `ProductService` o `PurchaseService` y prioriza el testing en las partes que manejan dinero o seguridad del usuario.
