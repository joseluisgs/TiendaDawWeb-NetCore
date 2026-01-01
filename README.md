# WalaDaw 🛒

![logo](./logo.svg)

[![.NET](https://img.shields.io/badge/.NET-10-blue)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-blue)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![C#](https://img.shields.io/badge/C%23-14-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-7-blue)](https://docs.microsoft.com/en-us/ef/core/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Ejemplo didáctico de web dinámicas con .NET Core y ASP.NET Core MVC.**

Una aplicación web de comercio electrónico de segunda mano con características avanzadas de seguridad,
internacionalización y gestión de usuarios.

## 🎯 Descripción

WalaDaw es un marketplace moderno desarrollado con Spring Boot que permite a los usuarios:

- Comprar y vender productos de segunda mano
- Gestionar perfiles de usuario con avatares
- Sistema de valoraciones y comentarios
- Panel de administración completo
- Subida de archivos e imágenes

## 📑 Tabla de Contenidos

- [WalaDaw 🛒](#waladaw-)
  - [🎯 Descripción](#-descripción)
  - [📑 Tabla de Contenidos](#-tabla-de-contenidos)
  - [✨ Características](#-características)
    - [Funcionalidades Principales](#funcionalidades-principales)
    - [Productos 2024-2025](#productos-2024-2025)
  - [🚀 Tecnologías](#-tecnologías)
  - [🏃‍♂️ Inicio Rápido](#️-inicio-rápido)
    - [Desarrollo Local](#desarrollo-local)
    - [Docker (Producción)](#docker-producción)
  - [📂 Estructura del Proyecto](#-estructura-del-proyecto)
  - [🐳 Docker](#-docker)
    - [Volúmenes de Datos](#volúmenes-de-datos)
    - [Comandos Docker Útiles](#comandos-docker-útiles)
  - [👥 Usuarios Demo](#-usuarios-demo)
  - [🔒 Seguridad](#-seguridad)
  - [🌐 Características](#-características-1)
    - [Para Usuarios](#para-usuarios)
    - [Para Administradores](#para-administradores)
  - [📊 Monitorización](#-monitorización)
  - [🚀 Despliegue en Producción](#-despliegue-en-producción)
    - [Variables de Entorno](#variables-de-entorno)
  - [📚 Documentación](#-documentación)
    - [Tutoriales Incluidos](#tutoriales-incluidos)
  - [📝 Licencia](#-licencia)
  - [👨‍💻 Autor](#-autor)
    - [Contacto](#contacto)
  - [Licencia de uso](#licencia-de-uso)

## ✨ Características

### Funcionalidades Principales

- 🛍️ **Marketplace de Segunda Mano**: Compra y vende productos usados
- 🔐 **Sistema de Roles**: ADMIN, USER, MODERATOR con permisos diferenciados
- 🌍 **Internacionalización**: Soporte completo para Español e Inglés
- 📧 **Notificaciones por Email**: Confirmación automática asíncrona de compras con templates HTML
- 📊 **Dashboard Administrativo**: Estadísticas y gráficos con Chart.js
- 🔍 **Búsqueda Avanzada**: Filtros por nombre, categoría y precio
- 🖼️ **Gestión de Imágenes**: Subida, validación y redimensionado automático
- 📱 **Responsive Design**: Bootstrap 5.3 optimizado para todos los dispositivos
- ⚡ **Cache Inteligente**: Mejora de rendimiento con Spring Cache
- 📄 **Generación de PDFs**: Facturas automáticas con cálculo de IVA y diseño profesional
- ❤️ **Sistema de Favoritos**: Añade productos a favoritos con AJAX
- ⭐ **Valoraciones y Ratings**: Sistema completo de reviews con estrellas y comentarios
- 🛡️ **Seguridad CSRF**: Protección completa contra ataques Cross-Site Request Forgery

### Productos 2024-2025

La aplicación incluye productos actuales y relevantes:

- 📱 **Smartphones**: iPhone 15 Pro Max, Samsung Galaxy S24 Ultra, Google Pixel 8 Pro
- 💻 **Laptops**: MacBook Pro M3
- 🎧 **Audio**: AirPods Pro 2ª Generación
- 🎮 **Gaming**: Steam Deck OLED

## 🚀 Tecnologías

- **.NET 10 Core con C#14** - Plataforma principal
- **ASP.NET Core MVC** - Framework web
- **Razor Pages** - Motor de vistas
- **Entity Framework Core** - ORM
- **InMemory Database** - Base de datos en memoria para desarrollo
- **ASP.NET Identity** - Gestión de usuarios y roles
- **Blazor Server** - Componentes interactivos
- **SignalR** - Comunicación en tiempo real
- **Bootstrap 5** - UI Framework
- **Docker** - Containerización

## 🏃‍♂️ Inicio Rápido

### Desarrollo Local

```bash
# Clonar repositorio


# Ejecutar aplicación


# Acceder a la aplicación

```

### Docker (Producción)

```bash
# Construir y ejecutar con Docker Compose
docker-compose up -d

# Ver logs


# Parar servicios
docker-compose down
```

## 📂 Estructura del Proyecto

```

```

## 🐳 Docker

### Volúmenes de Datos

El proyecto utiliza volúmenes Docker para persistencia:

- **upload-data**: Archivos subidos por usuarios (`./upload-dir`)
- **database-data**: Base de datos H2 (archivos `.mv.db`)

### Comandos Docker Útiles

```bash
# Ver volúmenes
docker volume ls

# Inspeccionar volumen


# Backup base de datos


# Restaurar base de datos

```

## 👥 Usuarios Demo

| Usuario | Email             | Password | Rol   |
|---------|-------------------|----------|-------|
| Admin   | admin@waladaw.com | admin123 | ADMIN |
| Juan    | juan@waladaw.com  | user123  | USER  |
| María   | maria@waladaw.com | user123  | USER  |

## 🔒 Seguridad

- Autenticación basada en formularios
- Autorización por roles (ADMIN, USER)
- Protección CSRF habilitada
- Validación de subida de archivos
- Sanitización de nombres de archivo

## 🌐 Características

### Para Usuarios

- ✅ Registro y login seguro
- ✅ Perfil con avatar personalizable
- ✅ Publicar productos con imágenes
- ✅ Sistema de valoraciones
- ✅ Gestión de favoritos
- ✅ Carrito de compras

### Para Administradores

- ✅ Panel de control completo
- ✅ Gestión de usuarios
- ✅ Moderación de contenido
- ✅ Estadísticas detalladas
- ✅ Configuración del sistema

## 📊 Monitorización

```bash
# Health check


# Métricas (si Actuator está habilitado)

```

## 🚀 Despliegue en Producción

### Variables de Entorno

```bash
# Docker Compose

# Base de datos (opcional para PostgreSQL/MySQL)

```

## 📚 Documentación

### Tutoriales Incluidos



## 📝 Licencia

Este proyecto es un ejemplo educativo con fines didácticos.

## 👨‍💻 Autor

Codificado con :sparkling_heart: por [José Luis González Sánchez](https://twitter.com/JoseLuisGS_)

[![Twitter](https://img.shields.io/twitter/follow/JoseLuisGS_?style=social)](https://twitter.com/JoseLuisGS_)
[![GitHub](https://img.shields.io/github/followers/joseluisgs?style=social)](https://github.com/joseluisgs)
[![GitHub](https://img.shields.io/github/stars/joseluisgs?style=social)](https://github.com/joseluisgs)

### Contacto

<p>
  Cualquier cosa que necesites házmelo saber por si puedo ayudarte 💬.
</p>
<p>
 <a href="https://joseluisgs.dev" target="_blank">
        <img src="https://joseluisgs.github.io/img/favicon.png" 
    height="30">
    </a>  &nbsp;&nbsp;
    <a href="https://github.com/joseluisgs" target="_blank">
        <img src="https://distreau.com/github.svg" 
    height="30">
    </a> &nbsp;&nbsp;
        <a href="https://twitter.com/JoseLuisGS_" target="_blank">
        <img src="https://i.imgur.com/U4Uiaef.png" 
    height="30">
    </a> &nbsp;&nbsp;
    <a href="https://www.linkedin.com/in/joseluisgonsan" target="_blank">
        <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/c/ca/LinkedIn_logo_initials.png/768px-LinkedIn_logo_initials.png" 
    height="30">
    </a>  &nbsp;&nbsp;
    <a href="https://g.dev/joseluisgs" target="_blank">
        <img loading="lazy" src="https://googlediscovery.com/wp-content/uploads/google-developers.png" 
    height="30">
    </a>  &nbsp;&nbsp;
<a href="https://www.youtube.com/@joseluisgs" target="_blank">
        <img loading="lazy" src="https://upload.wikimedia.org/wikipedia/commons/e/ef/Youtube_logo.png" 
    height="30">
    </a>  
</p>

## Licencia de uso

Este repositorio y todo su contenido está licenciado bajo licencia **Creative Commons**, si desea saber más, vea
la [LICENSE](https://joseluisgs.dev/docs/license/). Por favor si compartes, usas o modificas este proyecto cita a su
autor, y usa las mismas condiciones para su uso docente, formativo o educativo y no comercial.

<a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/"><img alt="Licencia de Creative Commons" style="border-width:0" src="https://i.creativecommons.org/l/by-nc-sa/4.0/88x31.png" /></a><br /><span xmlns:dct="http://purl.org/dc/terms/" property="dct:title">
JoseLuisGS</span>
by <a xmlns:cc="http://creativecommons.org/ns#" href="https://joseluisgs.dev/" property="cc:attributionName" rel="cc:attributionURL">
José Luis González Sánchez</a> is licensed under
a <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/4.0/">Creative Commons
Reconocimiento-NoComercial-CompartirIgual 4.0 Internacional License</a>.<br />Creado a partir de la obra
en <a xmlns:dct="http://purl.org/dc/terms/" href="https://github.com/joseluisgs" rel="dct:source">https://github.com/joseluisgs</a>.
