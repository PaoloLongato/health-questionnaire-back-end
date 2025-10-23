using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class PostgresWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("questionnairetests")
        .WithUsername("postgres")
        .WithPassword("postgres")
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

        return base.CreateHost(builder);
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
