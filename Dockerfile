# Multi-stage build for .NET 10 application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["TiendaDawWeb.csproj", "./"]
RUN dotnet restore "TiendaDawWeb.csproj"

# Copy everything else and build
COPY . .
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

# Create directory for uploads
RUN mkdir -p upload-dir && chmod 777 upload-dir

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "TiendaDawWeb.dll"]
