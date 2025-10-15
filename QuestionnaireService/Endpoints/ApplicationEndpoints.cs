using Microsoft.AspNetCore.Mvc;
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
}
