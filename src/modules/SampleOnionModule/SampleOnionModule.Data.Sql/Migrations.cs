using System.Data.Common;
using Dapper;

namespace ThermoFisher.SampleOnionModule.Data.Sql;

/// <summary>
/// Database creation and migration for the SampleOnion module
/// </summary>
public static class Migrations
{
    /// <summary>
    /// Create the database schema.
    /// </summary>
    /// <param name="dbConnection"></param>
    public async static Task InitializeOrMigrateSchemaAsync(
        DbConnection dbConnection,
        CancellationToken cancellationToken
    )
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS "sampleonionmodule"."samples" (
            "id"                UUID PRIMARY KEY,
            "name"              VARCHAR(200) NOT NULL,
            "description"       VARCHAR(2000),
            "created_date"      TIMESTAMP NOT NULL,
            "last_updated_date" TIMESTAMP NOT NULL
            );
            """;
        await dbConnection.ExecuteAsync(sql);
    }
}
