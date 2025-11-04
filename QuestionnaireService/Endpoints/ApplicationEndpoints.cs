using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionnaireService.Data;
using QuestionnaireService.Questionnaires;
using QuestionnaireService.Status;

namespace QuestionnaireService.Endpoints;

public sealed class ApplicationEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/status", (ServiceStatusProvider provider) =>
                Results.Ok(provider.GetStatus()))
            .WithName("GetStatus")
            .WithSummary("Returns service metadata for QuestionnaireService.")
            .Produces<ServiceStatus>();

        app.MapGet("/questionnaires", async Task<IResult> (QuestionnaireDbContext dbContext) =>
            {
                var items = await dbContext.Questionnaires
                    .OrderByDescending(q => q.UpdatedUtc)
                    .Select(q => new QuestionnaireListItem(q.Id, q.Title, q.Description, q.UpdatedUtc, q.UpdatedBy))
                    .ToListAsync();

                return Results.Ok(items);
            })
            .WithName("ListQuestionnaires")
            .WithSummary("Lists available questionnaires with metadata.")
            .Produces<List<QuestionnaireListItem>>();

        app.MapGet("/questionnaires/{id}", (
            string id,
            QuestionnairePlaceholderProvider provider) =>
        {
            if (!Guid.TryParse(id, out var questionnaireId))
            {
                return Results.Problem(
                    title: "Invalid questionnaire identifier",
                    detail: "The questionnaire ID must be a GUID.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var placeholder = provider.GetPlaceholder(questionnaireId);
            return Results.Ok(placeholder);
        })
        .WithName("GetQuestionnaireById")
        .WithSummary("Returns a placeholder questionnaire for the given identifier.")
        .Produces<QuestionnaireDetails>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    public sealed record QuestionnaireListItem(Guid Id, string Title, string? Description, DateTime UpdatedUtc, string? UpdatedBy);
}
