using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestionnaireService.Data;

namespace QuestionnaireService.UnitTests.Integration;

[Collection("IntegrationTests")]
public sealed class PublicQuestionnaireApiTests : IClassFixture<PostgresWebApplicationFactory>
{
    private readonly PostgresWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PublicQuestionnaireApiTests(PostgresWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuestionnaires_ReturnsMetadataOrderedByUpdatedUtc()
    {
        var now = DateTime.UtcNow;

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

            db.Questionnaires.AddRange(
                new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = "Older",
                    CreatedUtc = now.AddDays(-2),
                    UpdatedUtc = now.AddDays(-2),
                    Content = JsonSerializer.SerializeToDocument(new { version = 1 })
                },
                new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = "Newest",
                    CreatedUtc = now,
                    UpdatedUtc = now,
                    UpdatedBy = "editor@example.com",
                    Content = JsonSerializer.SerializeToDocument(new { version = 2 })
                },
                new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = "Deleted",
                    CreatedUtc = now.AddDays(-1),
                    UpdatedUtc = now.AddDays(-1),
                    Content = JsonSerializer.SerializeToDocument(new { version = 3 }),
                    IsDeleted = true
                });

            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/questionnaires");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<QuestionnaireListItem[]>()
                     ?? Array.Empty<QuestionnaireListItem>();

        Assert.Equal(2, payload.Length);
        Assert.Equal("Newest", payload[0].Title);
        Assert.Equal("Older", payload[1].Title);
        Assert.Null(payload[0].Description);
        Assert.True(payload[0].UpdatedUtc >= payload[1].UpdatedUtc);
        Assert.Equal("editor@example.com", payload[0].UpdatedBy);
    }

    [Fact]
    public async Task GetQuestionnaires_ReturnsEmptyArray_WhenNoneExist()
    {
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

            db.Questionnaires.RemoveRange(db.Questionnaires.IgnoreQueryFilters());
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/questionnaires");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<QuestionnaireListItem[]>();
        Assert.NotNull(payload);
        Assert.Empty(payload!);
    }

    private sealed record QuestionnaireListItem(Guid Id, string Title, string? Description, DateTime UpdatedUtc, string? UpdatedBy);
}
