# Multi-stage build for TiendaDawWeb MVC
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["TiendaDawWeb.Mvc/TiendaDawWeb.Mvc.csproj", "TiendaDawWeb.Mvc/"]
COPY ["TiendaDawWeb.Shared/TiendaDawWeb.Shared.csproj", "TiendaDawWeb.Shared/"]
COPY ["TiendaDawWeb.Shared.Blazor/TiendaDawWeb.Shared.Blazor.csproj", "TiendaDawWeb.Shared.Blazor/"]

# Restore dependencies
RUN dotnet restore "TiendaDawWeb.Mvc/TiendaDawWeb.Mvc.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TiendaDawWeb.Mvc"
RUN dotnet build "TiendaDawWeb.Mvc.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TiendaDawWeb.Mvc.csproj" -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install tzdata for timezone support
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*

# Set timezone
ENV TZ=Europe/Madrid

# Create non-root user
RUN groupadd -r appgroup && useradd -r -g appgroup appuser && chown -R appuser:appgroup /app
USER appuser

# Expose port
EXPOSE 5000

# Copy published files
COPY --from=publish --chown=appuser:appgroup /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads && chown -R appuser:appgroup /app/wwwroot

# Environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "TiendaDawWeb.Mvc.dll"]
