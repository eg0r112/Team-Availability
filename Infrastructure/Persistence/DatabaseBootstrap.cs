using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TeamAvailability.Infrastructure.Persistence;

public static class DatabaseBootstrap
{
    public static async Task EnsureDatabaseExistsAsync(IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database ?? throw new InvalidOperationException("Database name is missing in connection string.");

        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var check = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        check.Parameters.AddWithValue("name", databaseName);

        var exists = await check.ExecuteScalarAsync(cancellationToken) is not null;
        if (exists)
            return;

        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await create.ExecuteNonQueryAsync(cancellationToken);
    }
}
