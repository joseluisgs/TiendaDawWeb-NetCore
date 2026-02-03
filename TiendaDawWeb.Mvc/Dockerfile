# Multi-stage build for .NET 10 application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
# Build context must be solution root
COPY ["TiendaDawWeb.Web/TiendaDawWeb.csproj", "TiendaDawWeb.Web/"]
RUN dotnet restore "TiendaDawWeb.Web/TiendaDawWeb.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/TiendaDawWeb.Web"
RUN dotnet build "TiendaDawWeb.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "TiendaDawWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Create directory for uploads ensuring permissions
RUN mkdir -p wwwroot/uploads && chmod 777 wwwroot/uploads

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "TiendaDawWeb.dll"]