using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using QuestionnaireService.Data;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:QuestionnaireDb"]
    ?? throw new InvalidOperationException("PostgreSQL connection string not configured.");

builder.Services.AddDbContext<QuestionnaireDbContext>(options =>
    options.UseNpgsql(connectionString));

using var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

const int maxAttempts = 12;
for (var attempt = 1; attempt <= maxAttempts; attempt++)
{
    try
    {
        if (await dbContext.Database.CanConnectAsync())
        {
            break;
        }
    }
    catch when (attempt < maxAttempts)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
        continue;
    }

    if (attempt == maxAttempts)
    {
        throw new InvalidOperationException("Unable to connect to PostgreSQL after multiple attempts.");
    }

    await Task.Delay(TimeSpan.FromSeconds(2));
}

await using var advisoryConnection = new NpgsqlConnection(connectionString);
await advisoryConnection.OpenAsync();
const long lockId = 0x51554d4954523031; // 'QUMITR01' in hex

await using (var acquire = new NpgsqlCommand("SELECT pg_advisory_lock(@lockId);", advisoryConnection))
{
    acquire.Parameters.AddWithValue("lockId", lockId);
    await acquire.ExecuteNonQueryAsync();
}

try
{
    Console.WriteLine("Applying database migrations...");
    await dbContext.Database.MigrateAsync();
    Console.WriteLine("Database migrations completed successfully.");
}
finally
{
    await using var release = new NpgsqlCommand("SELECT pg_advisory_unlock(@lockId);", advisoryConnection);
    release.Parameters.AddWithValue("lockId", lockId);
    await release.ExecuteNonQueryAsync();
}
