# 🚀 Docker Deployment - TiendaDawWeb

Este proyecto incluye configuración Docker para desplegar tanto la aplicación MVC como Razor Pages.

## 📋 Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac)
- [Docker Engine](https://docs.docker.com/engine/install/) (Linux)

## 🐳 Servicios Disponibles

| Servicio | Puerto | Descripción |
|----------|--------|-------------|
| MVC | 5000 | Aplicación ASP.NET Core MVC |
| Razor Pages | 5002 | Aplicación ASP.NET Core Razor Pages |

## ⚡ Inicio Rápido

```bash
# Construir y ejecutar ambos servicios
docker-compose up --build

# O en modo producción (sin logs)
docker-compose up -d --build

# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down
```

## 🏗️ Construcción Individual

```bash
# Solo MVC
docker build -f TiendaDawWeb.Mvc/Dockerfile -t tiendadaw-mvc .

# Solo Razor Pages
docker build -f TiendaDawWeb.RazorPages/Dockerfile -t tiendadaw-razorpages .
```

## 🔧 Configuración de Email

Los contenedores requieren configuración SMTP para enviar emails. Configura las variables de entorno:

```yaml
environment:
  - Email__SmtpHost=smtp.tuservidor.com
  - Email__SmtpPort=587
  - Email__SmtpUser=tu_usuario
  - Email__SmtpPass=tu_password
```

O crea un archivo `.env`:

```bash
# .env
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__SmtpUser=tu@gmail.com
Email__SmtpPass=tu_app_password
```

## 🌐 URLs de Acceso

| Entorno | URL |
|---------|-----|
| Desarrollo | http://localhost:5000 (MVC) / http://localhost:5002 (Razor Pages) |
| Producción | http://tu-dominio.com (MVC) / http://tu-dominio.com:5002 (Razor Pages) |

## 📁 Volúmenes

| Volumen | Descripción |
|---------|-------------|
| `mvc-uploads` | Imágenes de productos (MVC) |
| `razor-uploads` | Imágenes de productos (Razor Pages) |

## 🔒 Seguridad

- Los contenedores ejecutan como usuario no-root
- Health checks integrados
- Timezone configurado (Europe/Madrid)

## 📊 Logs y Monitoreo

```bash
# Ver logs de un servicio específico
docker-compose logs mvc
docker-compose logs razor-pages

# Ver logs en tiempo real
docker-compose logs -f

# Stats de contenedores
docker stats
```

## 🧪 Desarrollo con Docker

Para desarrollo rápido:

```bash
# Ejecutar sin cache
docker-compose build --no-cache

# Verificar que los contenedores están healthy
docker-compose ps
```

## 🐛 Solución de Problemas

```bash
# Verificar puertos ocupados
netstat -ano | findstr :5000
netstat -ano | findstr :5002

# Reiniciar contenedores
docker-compose restart

# Limpiar todo y comenzar de nuevo
docker-compose down -v
docker-compose up --build
```
