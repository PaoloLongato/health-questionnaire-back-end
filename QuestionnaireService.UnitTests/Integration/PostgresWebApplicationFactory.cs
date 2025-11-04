using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestionnaireService.Data;
using Testcontainers.PostgreSql;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("questionnairetests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithCleanUp(true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("pg_isready -U postgres"))
        .Build();

    private bool _started;

    public string ConnectionString => _container.GetConnectionString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (!_started)
        {
            _container.StartAsync().GetAwaiter().GetResult();
            _started = true;
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__QuestionnaireDb", ConnectionString);

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();
        db.Database.Migrate();

        return host;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        if (_started)
        {
            await _container.DisposeAsync();
            Environment.SetEnvironmentVariable("ConnectionStrings__QuestionnaireDb", null);
        }
    }
}
