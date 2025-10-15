using System.Text.Json;

namespace QuestionnaireService.Questionnaires;

public sealed record QuestionnaireDetails(Guid Id, string Title, string Description, JsonElement Data);
