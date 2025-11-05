using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestionnaireService.Data;
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
    public async Task QuestionnaireEndpoint_ReturnsPersistedData_WhenIdExists()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();
            dbContext.Questionnaires.Add(new Questionnaire
            {
                Id = id,
                Title = "Nutrition Survey",
                Description = "Tracks dietary habits",
                CreatedUtc = now.AddDays(-1),
                UpdatedUtc = now,
                UpdatedBy = "admin@example.com",
                Content = JsonSerializer.SerializeToDocument(new { sections = new[] { "s1" }, version = 3 })
            });

            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/questionnaires/{id}");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<QuestionnaireResponse>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(payload);
        Assert.Equal(id, payload!.Id);
        Assert.Equal("Nutrition Survey", payload.Title);
        Assert.Equal("Tracks dietary habits", payload.Description);
        Assert.Equal("admin@example.com", payload.UpdatedBy);
        Assert.Equal(JsonValueKind.Array, payload.Content.GetProperty("sections").ValueKind);
        Assert.Equal(3, payload.Content.GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task QuestionnaireEndpoint_ReturnsNotFound_WhenMissing()
    {
        var response = await _client.GetAsync($"/questionnaires/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task QuestionnaireEndpoint_ReturnsNotFound_WhenSoftDeleted()
    {
        var id = Guid.NewGuid();

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();
            dbContext.Questionnaires.Add(new Questionnaire
            {
                Id = id,
                Title = "Archived",
                CreatedUtc = DateTime.UtcNow.AddDays(-10),
                UpdatedUtc = DateTime.UtcNow.AddDays(-5),
                IsDeleted = true,
                Content = JsonSerializer.SerializeToDocument(new { archived = true })
            });

            await dbContext.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"/questionnaires/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

    private sealed record QuestionnaireResponse(
        Guid Id,
        string Title,
        string? Description,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        string? UpdatedBy,
        JsonElement Content);
}
