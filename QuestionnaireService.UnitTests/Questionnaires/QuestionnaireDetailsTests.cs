using System.Text.Json;
using QuestionnaireService.Questionnaires;

namespace QuestionnaireService.UnitTests.Questionnaires;

public sealed class QuestionnaireDetailsTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var id = Guid.NewGuid();
        const string title = "Sample Questionnaire";
        const string description = "Placeholder description";
        using var document = JsonDocument.Parse("""{"example":"value"}""");

        var details = new QuestionnaireDetails(id, title, description, document.RootElement.Clone());

        Assert.Equal(id, details.Id);
        Assert.Equal(title, details.Title);
        Assert.Equal(description, details.Description);
        Assert.Equal("value", details.Data.GetProperty("example").GetString());
    }
}
