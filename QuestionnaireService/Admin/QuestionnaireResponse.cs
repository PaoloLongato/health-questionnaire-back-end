using System.Text.Json;

namespace QuestionnaireService.Admin;

public sealed record QuestionnaireResponse(
    Guid Id,
    string Title,
    string? Description,
    JsonElement Content,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    string? UpdatedBy,
    bool IsDeleted);
