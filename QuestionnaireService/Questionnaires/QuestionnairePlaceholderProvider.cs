using System.Text.Json;

namespace QuestionnaireService.Questionnaires;

public sealed class QuestionnairePlaceholderProvider
{
    private const string PlaceholderTitle = "Sample Questionnaire";
    private const string PlaceholderDescription = "This is placeholder questionnaire content.";

    private static readonly JsonDocument PlaceholderDocument = JsonDocument.Parse("""
        {
          "sections": [
            {
              "id": "section-1",
              "title": "Getting Started",
              "questions": [
                {
                  "id": "question-1",
                  "type": "text",
                  "prompt": "How are you feeling today?"
                }
              ]
            }
          ]
        }
        """);

    public QuestionnaireDetails GetPlaceholder(Guid id)
    {
        return new QuestionnaireDetails(
            id,
            PlaceholderTitle,
            PlaceholderDescription,
            PlaceholderDocument.RootElement.Clone());
    }
}
