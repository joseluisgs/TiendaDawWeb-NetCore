using System.Data;
using System.Data.Common;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
///     Completación del esquema de Identity en bases de datos que ya existen.
///     🎓 EnsureCreated solo crea la base de datos si no existe: en una BD ya poblada
///     (producción) no añade las tablas nuevas del modelo actual. Este parche crea la
///     tabla que falta (AspNetUserPasskeys, esquema 3 de Identity) sin migraciones y
///     sin tocar los datos existentes, de modo que el arranque único cubre el esquema
///     completo tanto en una BD nueva como en una vieja.
/// </summary>
public static class IdentitySchemaPatch {
    /// <summary>Tabla de passkeys que añade el esquema 3 de Identity.</summary>
    private const string TablaPasskeys = "AspNetUserPasskeys";

    /// <summary>DDL de la tabla de passkeys para SQLite, con el índice de usuario.</summary>
    private const string DdlSqlite = """
                                     CREATE TABLE IF NOT EXISTS "AspNetUserPasskeys" (
                                         "CredentialId" BLOB NOT NULL CONSTRAINT "PK_AspNetUserPasskeys" PRIMARY KEY,
                                         "UserId" INTEGER NOT NULL,
                                         "Data" TEXT NOT NULL,
                                         CONSTRAINT "FK_AspNetUserPasskeys_AspNetUsers_UserId" FOREIGN KEY ("UserId")
                                             REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
                                     );
                                     CREATE INDEX IF NOT EXISTS "IX_AspNetUserPasskeys_UserId" ON "AspNetUserPasskeys" ("UserId");
                                     """;

    /// <summary>DDL de la tabla de passkeys para PostgreSQL, con el índice de usuario.</summary>
    private const string DdlPostgres = """
                                       CREATE TABLE IF NOT EXISTS "AspNetUserPasskeys" (
                                           "CredentialId" bytea NOT NULL,
                                           "UserId" bigint NOT NULL,
                                           "Data" text NOT NULL,
                                           CONSTRAINT "PK_AspNetUserPasskeys" PRIMARY KEY ("CredentialId"),
                                           CONSTRAINT "FK_AspNetUserPasskeys_AspNetUsers_UserId" FOREIGN KEY ("UserId")
                                               REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
                                       );
                                       CREATE INDEX IF NOT EXISTS "IX_AspNetUserPasskeys_UserId" ON "AspNetUserPasskeys" ("UserId");
                                       """;

    /// <summary>
    ///     Crea la tabla de passkeys si falta y el proveedor es relacional (SQLite o
    ///     PostgreSQL). Idempotente: si la tabla ya está, no hace nada; en una BD nueva
    ///     EnsureCreated la ha creado y este método se limita a comprobarlo.
    /// </summary>
    /// <param name="context">Contexto de la aplicación.</param>
    /// <returns>Task</returns>
    public static async Task CompletarAsync(ApplicationDbContext context) {
        if (await TablaExisteAsync(context)) {
            return;
        }

        string? ddl = context.Database.IsSqlite() ? DdlSqlite
            : context.Database.IsNpgsql() ? DdlPostgres
            : null;

        // InMemory (tests) u otro proveedor: no hay esquema relacional que tocar.
        if (ddl is null) {
            return;
        }

        await context.Database.ExecuteSqlRawAsync(ddl);
        Log.Information("🧹 Esquema actualizado: creada la tabla {Tabla}", TablaPasskeys);
    }

    /// <summary>
    ///     Comprueba en el catálogo del proveedor si la tabla ya existe.
    /// </summary>
    /// <param name="context">Contexto de la aplicación.</param>
    /// <returns>true si la tabla está creada.</returns>
    private static async Task<bool> TablaExisteAsync(ApplicationDbContext context) {
        DbConnection conexion = context.Database.GetDbConnection();
        bool abierta = conexion.State == ConnectionState.Open;
        if (!abierta) {
            await conexion.OpenAsync();
        }

        try {
            using DbCommand comando = conexion.CreateCommand();
            comando.CommandText = context.Database.IsSqlite()
                ? $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{TablaPasskeys}'"
                : $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{TablaPasskeys}'";

            object? resultado = await comando.ExecuteScalarAsync();
            return Convert.ToInt32(resultado) > 0;
        } finally {
            if (!abierta) {
                await conexion.CloseAsync();
            }
        }
    }
}
