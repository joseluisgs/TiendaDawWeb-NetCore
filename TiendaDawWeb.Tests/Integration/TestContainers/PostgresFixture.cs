using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Testcontainers.PostgreSql;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Tests.Integration.TestContainers;

/// <summary>
/// Fixture que levanta PostgreSQL en Docker para tests de integración.
/// Se ejecuta una vez por suite (OneTimeSetUp/OneTimeTearDown).
/// </summary>
public sealed class PostgresFixture
{
    private PostgreSqlContainer? _container;

    public string ConnectionString => _container!.GetConnectionString();

    public async Task StartAsync()
    {
        _container = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("tiendadaw_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _container.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }

    /// <summary>
    /// Crea un DbContext con base de datos unique por test (aislamiento total).
    /// </summary>
    public ApplicationDbContext CreateIsolatedContext(string dbName)
    {
        var isolatedCs = ConnectionString.Replace("tiendadaw_test", dbName);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(isolatedCs)
            .Options;

        return new ApplicationDbContext(options);
    }
}
