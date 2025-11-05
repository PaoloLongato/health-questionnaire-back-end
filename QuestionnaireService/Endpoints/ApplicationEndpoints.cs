using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestionnaireService.Data;
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

        app.MapGet("/questionnaires/{id}", async Task<IResult> (string id, QuestionnaireDbContext dbContext) =>
            {
                if (!Guid.TryParse(id, out var questionnaireId))
                {
                    return Results.Problem(
                        title: "Invalid questionnaire identifier",
                        detail: "The questionnaire ID must be a GUID.",
                        statusCode: StatusCodes.Status400BadRequest);
                }

                var questionnaire = await dbContext.Questionnaires
                    .FirstOrDefaultAsync(q => q.Id == questionnaireId);

                if (questionnaire is null)
                {
                    return Results.NotFound();
                }

                var response = new QuestionnaireDetailsResponse(
                    questionnaire.Id,
                    questionnaire.Title,
                    questionnaire.Description,
                    questionnaire.CreatedUtc,
                    questionnaire.UpdatedUtc,
                    questionnaire.UpdatedBy,
                    questionnaire.Content.RootElement.Clone());

                return Results.Ok(response);
            })
        .WithName("GetQuestionnaireById")
        .WithSummary("Returns a questionnaire for the given identifier.")
        .Produces<QuestionnaireDetailsResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }

    public sealed record QuestionnaireListItem(Guid Id, string Title, string? Description, DateTime UpdatedUtc, string? UpdatedBy);

    public sealed record QuestionnaireDetailsResponse(
        Guid Id,
        string Title,
        string? Description,
        DateTime CreatedUtc,
        DateTime UpdatedUtc,
        string? UpdatedBy,
        JsonElement Content);
}
