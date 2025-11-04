using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using QuestionnaireService.Admin;
using QuestionnaireService.Data;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class AdminQuestionnaireApiTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public AdminQuestionnaireApiTests(PostgresWebApplicationFactory factory, Xunit.Abstractions.ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task CreateQuestionnaire_PersistsAndReturnsCreated()
    {
        var payload = new QuestionnaireWriteRequest
        {
            Title = "Sleep Habits Survey",
            Description = "Collects data about sleep patterns",
            Content = JsonDocument.Parse("""{"nights":7,"questions":[{"id":"q1"}]}""").RootElement.Clone(),
            UpdatedBy = "admin@example.com"
        };

        var response = await _client.PostAsJsonAsync("/admin/questionnaires", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.Created)
        {
            _output.WriteLine($"CreateQuestionnaire response: {responseBody}");
        }

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var resource = JsonSerializer.Deserialize<QuestionnaireResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(resource);
        Assert.Equal(payload.Title, resource!.Title);
        Assert.Equal(payload.Description, resource.Description);
        Assert.Equal(payload.UpdatedBy, resource.UpdatedBy);
        Assert.False(resource.IsDeleted);
        Assert.Equal(7, resource.Content.GetProperty("nights").GetInt32());

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

        var stored = await dbContext.Questionnaires.FindAsync(resource.Id);
        Assert.NotNull(stored);
        Assert.Equal(resource.Title, stored!.Title);
    }

    [Fact]
    public async Task CreateQuestionnaire_MissingTitle_ReturnsValidationProblem()
    {
        using var invalidPayload = JsonContent.Create(new
        {
            title = (string?)null,
            content = new { example = true }
        });
        var response = await _client.PostAsync("/admin/questionnaires", invalidPayload);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            _output.WriteLine($"Validation failure response: {responseBody}");
        }

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = JsonSerializer.Deserialize<ValidationProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(problem);
        Assert.Contains("Title", problem!.Errors.Keys, StringComparer.OrdinalIgnoreCase);

        // Missing content - send payload without content property
        var minimalContent = JsonContent.Create(new { Title = "Valid" });
        response = await _client.PostAsync("/admin/questionnaires", minimalContent);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var missingContentPayload = new StringContent("""{ "title": "Valid" }""", System.Text.Encoding.UTF8, "application/json");
        response = await _client.PostAsync("/admin/questionnaires", missingContentPayload);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateQuestionnaire_ReplacesStoredData()
    {
        var createPayload = new QuestionnaireWriteRequest
        {
            Title = "Initial",
            Content = JsonDocument.Parse("""{"version":1}""").RootElement.Clone()
        };

        var createResponse = await _client.PostAsJsonAsync("/admin/questionnaires", createPayload);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<QuestionnaireResponse>(createBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        var updatePayload = new QuestionnaireWriteRequest
        {
            Title = "Updated",
            Description = "New description",
            Content = JsonDocument.Parse("""{"version":2,"active":true}""").RootElement.Clone(),
            UpdatedBy = "editor@example.com"
        };

        var updateResponse = await _client.PutAsJsonAsync($"/admin/questionnaires/{created.Id}", updatePayload);
        var updateBody = await updateResponse.Content.ReadAsStringAsync();
        if (updateResponse.StatusCode != HttpStatusCode.OK)
        {
            _output.WriteLine($"Update response: {updateBody}");
        }

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = JsonSerializer.Deserialize<QuestionnaireResponse>(updateBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(updated);
        Assert.Equal(updatePayload.Title, updated!.Title);
        Assert.Equal(updatePayload.Description, updated.Description);
        Assert.Equal(updatePayload.UpdatedBy, updated.UpdatedBy);
        Assert.Equal(2, updated.Content.GetProperty("version").GetInt32());

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();
        var stored = await dbContext.Questionnaires.FindAsync(created.Id);
        Assert.NotNull(stored);
        Assert.Equal("Updated", stored!.Title);
        Assert.Equal("New description", stored.Description);
        Assert.Equal("editor@example.com", stored.UpdatedBy);
    }

    [Fact]
    public async Task UpdateQuestionnaire_MissingEntity_ReturnsNotFound()
    {
        var payload = new QuestionnaireWriteRequest
        {
            Title = "Updated",
            Content = JsonDocument.Parse("""{"version":3}""").RootElement.Clone()
        };

        var response = await _client.PutAsJsonAsync($"/admin/questionnaires/{Guid.NewGuid()}", payload);
        var responseBody = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.NotFound)
        {
            _output.WriteLine($"Update missing response: {responseBody}");
        }

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
