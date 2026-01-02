# 05. EF Core y Persistencia: De la Memoria a la Realidad

Entity Framework Core es el "mago" que transforma tus clases en tablas.

## 1. El DbContext y el Modelo de Datos
- **Entidades**: Clases C# puras (POCOs) con atributos que definen reglas de BD (ej. `[Key]`, `[MaxLength]`).
- **Relaciones**: Uso de `virtual` para habilitar el Lazy Loading (carga perezosa de datos relacionados).

## 2. Base de Datos In-Memory
Ideal para este proyecto educativo. 
```csharp
options.UseInMemoryDatabase("WalaDawDb")
```
- Cada vez que reinicias el servidor, la base de datos se borra y se recrea con el **SeedData**. Esto garantiza que todos los alumnos trabajen en el mismo punto de partida siempre.

## 3. Background Services: El Trabajo sucio
Tareas como "limpiar carritos cada hora" no deben esperar al usuario.
- **Hosted Services**: Heredan de `BackgroundService`.
- **Desafío Singleton vs Scoped**: Como un BackgroundService vive siempre, no puede inyectar el `DbContext`. Debe crear un "Scope" manual cada vez que necesite tocar la base de datos.

## 4. Seed Data Profesional
En `Data/SeedData.cs` poblamos la base de datos.
- **Tip Senior**: Usa `UserManager` para crear usuarios. Nunca escribas los passwords a mano en el código, deja que el framework los hashee por seguridad.
- **Verificación de existencia**: Siempre comprueba si la base de datos ya tiene datos antes de inyectar los de prueba para evitar duplicados.
