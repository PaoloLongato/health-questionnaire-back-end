using QuestionnaireService.Questionnaires;

namespace QuestionnaireService.UnitTests.Questionnaires;

public sealed class QuestionnairePlaceholderProviderTests
{
    [Fact]
    public void GetPlaceholder_ReturnsDetailsWithGivenId()
    {
        var provider = new QuestionnairePlaceholderProvider();
        var id = Guid.NewGuid();

        var details = provider.GetPlaceholder(id);

        Assert.Equal(id, details.Id);
        Assert.False(string.IsNullOrWhiteSpace(details.Title));
        Assert.False(string.IsNullOrWhiteSpace(details.Description));
        Assert.True(details.Data.TryGetProperty("sections", out var sections));
        Assert.True(sections.ValueKind == System.Text.Json.JsonValueKind.Array);
    }
}
