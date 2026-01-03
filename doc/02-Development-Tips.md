# 12 - Guía de Productividad: Hot Reload y Trucos de Desarrollo

Para ser un desarrollador eficiente en .NET 10, no basta con saber programar; hay que saber dominar las herramientas para reducir el ciclo de feedback (el tiempo entre que escribes código y lo ves funcionando).

---

## 1. El súper poder: Hot Reload (Recarga en Caliente)

Hot Reload permite aplicar cambios en el código mientras la aplicación está corriendo **sin reiniciar el servidor**.

### ¿Por qué es vital en este proyecto?
Como usamos **SQLite In-Memory**, si reiniciamos la aplicación, **perdemos los datos** que hayamos creado manualmente. Con Hot Reload:
- Cambias una Vista Razor o Componente Blazor: Los datos de la RAM **siguen vivos**.
- Cambias la lógica de un Controlador: Los datos de la RAM **siguen vivos**.

---

## 2. Uso profesional de la CLI: `dotnet watch`

El comando `watch` vigila tus archivos y aplica Hot Reload automáticamente.

### El comando correcto desde la raíz:
Como esta solución tiene varios proyectos (Web y Tests), debes especificar cuál quieres vigilar:
```bash
dotnet watch --project TiendaDawWeb.Web
```

### Comandos útiles dentro de `watch`:
Mientras `dotnet watch` está corriendo, puedes pulsar teclas en la terminal:
- `r`: Fuerza un reinicio completo (útil si la base de datos se queda en un estado inconsistente).
- `b`: Fuerza una compilación (build).

---

## 3. Trucos para JetBrains Rider 🛠️

Rider es un IDE de alto rendimiento. Aquí tienes cómo exprimirlo:

1.  **El Rayo Amarillo:** Cuando la app corre, verás un icono de un rayo en la barra superior. Al pulsarlo, Rider inyecta los cambios actuales (Apply Changes).
2.  **Hot Reload Automático:**
    - Ve a `Settings` -> `Build, Execution, Deployment` -> `Hot Reload`.
    - Activa **"Apply hot reload changes on save"**. Ahora, cada vez que hagas `Ctrl+S`, la web se actualizará sola.
3.  **Terminal Integrada:** No salgas del IDE. Usa la terminal de Rider (Alt+F12) para lanzar el `dotnet watch`. Rider reconocerá los enlaces y errores de compilación directamente.

---

## 4. Trucos para Visual Studio 2022 🟦

1.  **Icono de la Llama:** Usa el botón de la llama de fuego naranja para aplicar cambios.
2.  **Ctrl + F5:** Inicia siempre la aplicación con `Ctrl + F5` (Sin Depurar). El Hot Reload es mucho más estable y rápido cuando el debugger no está enganchado.

---

## 5. El "Limbo" de la Persistencia (SQLite Tip)

Recuerda:
- **Cambio de UI/Lógica:** Hot Reload funciona -> **Datos persistentes**.
- **Cambio de Estructura (clases, modelos, Program.cs):** Requiere Reinicio -> **Datos borrados (vuelve a actuar SeedData)**.
