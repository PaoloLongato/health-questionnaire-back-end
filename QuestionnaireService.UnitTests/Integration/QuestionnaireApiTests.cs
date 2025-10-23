using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QuestionnaireService.Data;
using QuestionnaireService.Questionnaires;
using QuestionnaireService.Status;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class QuestionnaireApiTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QuestionnaireApiTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public void DbContext_UsesNpgsqlProvider()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", dbContext.Database.ProviderName);
    }

    [Fact]
    public async Task StatusEndpoint_ReturnsMetadata()
    {
        var status = await _client.GetFromJsonAsync<ServiceStatus>("/status");

        Assert.NotNull(status);
        Assert.Equal(typeof(Program).Assembly.GetName().Name, status!.Service);
        Assert.False(string.IsNullOrWhiteSpace(status.Environment));
        Assert.False(string.IsNullOrWhiteSpace(status.Version));
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/healthz");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", payload.GetProperty("status").GetString());

        var entries = payload.GetProperty("entries");
        Assert.True(entries.TryGetProperty("postgresql", out var dbEntry));
        Assert.Equal("Healthy", dbEntry.GetProperty("status").GetString());
    }

    [Fact]
    public async Task QuestionnaireEndpoint_ReturnsPlaceholder_WhenIdIsValidGuid()
    {
        var id = Guid.NewGuid();

        var details = await _client.GetFromJsonAsync<QuestionnaireDetails>($"/questionnaires/{id}");

        Assert.NotNull(details);
        Assert.Equal(id, details!.Id);
        Assert.False(string.IsNullOrWhiteSpace(details.Title));
        Assert.False(string.IsNullOrWhiteSpace(details.Description));
        Assert.True(details.Data.TryGetProperty("sections", out var sections));
        Assert.Equal(System.Text.Json.JsonValueKind.Array, sections.ValueKind);
    }

    [Fact]
    public async Task QuestionnaireEndpoint_ReturnsBadRequest_WhenIdIsInvalid()
    {
        var response = await _client.GetAsync("/questionnaires/not-a-guid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal("Invalid questionnaire identifier", problem!.Title);
    }
}
