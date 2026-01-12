# 11 - Persistencia Volátil: SQLite In-Memory vs InMemoryDatabase

En este proyecto, hemos evolucionado de `InMemoryDatabase` (el proveedor por defecto de Microsoft para pruebas rápidas) hacia una configuración de **SQLite en memoria con conexión persistente**.

---

## 1. ¿Por qué el cambio? (Rigor Técnico)

Aunque ambos proveedores viven en la memoria RAM y se borran al cerrar la aplicación, existen diferencias críticas:

| Característica | InMemoryDatabase | SQLite In-Memory |
| :--- | :--- | :--- |
| **Tipo de Motor** | Diccionarios de C# | Motor SQL Relacional Real |
| **Transacciones** | Ignoradas (No-op) | **Soportadas (Rollback real)** |
| **Claves Foráneas** | Ignoradas | **Enforzadas (Rigor referencial)** |
| **Generación de IDs** | Básica | Secuencial SQL estándar |

Para un proyecto que enseña **transacciones serializables** en el proceso de compra, `InMemoryDatabase` era insuficiente porque permitía "compras de mentira" que no validaban realmente la concurrencia.

---

## 2. El Desafío del Ciclo de Vida

Por defecto, una base de datos SQLite In-Memory desaparece en cuanto se cierra la conexión. En una web normal, el `DbContext` se abre y cierra en cada click, lo que borraría la base de datos constantemente.

### La Solución "Keep-Alive"
En `Program.cs`, hemos implementado un patrón de **Conexión Persistente**:
1.  Creamos un objeto `SqliteConnection` al arrancar.
2.  Lo abrimos manualmente (`.Open()`).
3.  Le decimos a EF Core que use esa conexión específica en lugar de crear una nueva.

Mientras la variable `keepAliveConnection` viva (que será hasta que el proceso de la web se detenga), la base de datos en RAM permanecerá intacta.

---

## 3. Integración con SeedData

Como la base de datos empieza vacía en cada inicio del servidor, dependemos de `SeedData.InitializeAsync()`. 
- **Ventaja:** Siempre empezamos con un entorno limpio y predecible.
- **Inconveniente:** Si creas un producto a mano y reinicias el servidor, ese producto desaparecerá.

---

## 4. Conclusión para el Alumno

Usar SQLite In-Memory nos permite tener **lo mejor de los dos mundos**:
- La **velocidad y limpieza** de no tener archivos físicos en el ordenador del laboratorio.
- El **rigor y profesionalidad** de un motor SQL que valida transacciones y relaciones de tablas.

Es el paso previo ideal antes de saltar a una base de datos persistente en disco como SQL Server o PostgreSQL.
