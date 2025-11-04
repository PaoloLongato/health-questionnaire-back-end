using System.Text.Json;

namespace QuestionnaireService.Data;

public sealed class Questionnaire
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required JsonDocument Content { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
}
