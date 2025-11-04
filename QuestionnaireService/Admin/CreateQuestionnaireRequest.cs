using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace QuestionnaireService.Admin;

public sealed class CreateQuestionnaireRequest
{
    [Required]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public JsonElement Content { get; set; }

    public string? UpdatedBy { get; set; }
}
