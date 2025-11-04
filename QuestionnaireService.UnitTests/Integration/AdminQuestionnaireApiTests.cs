using System.Net;
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
        var payload = new CreateQuestionnaireRequest
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
        var payload = new CreateQuestionnaireRequest
        {
            Title = null,
            Content = JsonDocument.Parse("""{"example":true}""").RootElement.Clone()
        };

        var response = await _client.PostAsJsonAsync("/admin/questionnaires", payload);
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
    }
}
