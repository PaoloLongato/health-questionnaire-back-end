using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using QuestionnaireService.Admin;
using QuestionnaireService.Data;

namespace QuestionnaireService.Endpoints;

public sealed class AdminEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/admin/questionnaires")
            .WithTags("AdminQuestionnaires");

        group.MapPost("", async Task<IResult> (QuestionnaireWriteRequest request, QuestionnaireDbContext dbContext) =>
            {
                if (!MinimalValidation.TryValidate(request, out var validationErrors))
                {
                    return Results.ValidationProblem(validationErrors);
                }

                if (request.Content.ValueKind == JsonValueKind.Undefined)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        [nameof(request.Content)] = ["Content is required."]
                    });
                }

                var now = DateTime.UtcNow;
                var contentDocument = JsonSerializer.SerializeToDocument(request.Content);
                var questionnaire = new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title!,
                    Description = request.Description,
                    Content = contentDocument,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                    UpdatedBy = request.UpdatedBy,
                    IsDeleted = false
                };

                dbContext.Questionnaires.Add(questionnaire);
                await dbContext.SaveChangesAsync();

                var response = new QuestionnaireResponse(
                    questionnaire.Id,
                    questionnaire.Title,
                    questionnaire.Description,
                    questionnaire.Content.RootElement.Clone(),
                    questionnaire.CreatedUtc,
                    questionnaire.UpdatedUtc,
                    questionnaire.UpdatedBy,
                    questionnaire.IsDeleted);

                return Results.Created($"/admin/questionnaires/{questionnaire.Id}", response);
            })
            .WithName("CreateQuestionnaire")
            .WithSummary("Create a questionnaire")
            .WithDescription("Creates a new questionnaire for administrative users.");

        group.MapPut("{id:guid}", async Task<IResult> (Guid id, QuestionnaireWriteRequest request, QuestionnaireDbContext dbContext) =>
            {
                if (!MinimalValidation.TryValidate(request, out var validationErrors))
                {
                    return Results.ValidationProblem(validationErrors);
                }

                if (request.Content.ValueKind == JsonValueKind.Undefined)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        [nameof(request.Content)] = ["Content is required."]
                    });
                }

                var questionnaire = await dbContext.Questionnaires
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(q => q.Id == id);

                if (questionnaire is null || questionnaire.IsDeleted)
                {
                    return Results.NotFound();
                }

                var now = DateTime.UtcNow;
                questionnaire.Content.Dispose();
                questionnaire.Title = request.Title!;
                questionnaire.Description = request.Description;
                questionnaire.Content = JsonSerializer.SerializeToDocument(request.Content);
                questionnaire.UpdatedUtc = now;
                questionnaire.UpdatedBy = request.UpdatedBy;

                await dbContext.SaveChangesAsync();

                var response = new QuestionnaireResponse(
                    questionnaire.Id,
                    questionnaire.Title,
                    questionnaire.Description,
                    questionnaire.Content.RootElement.Clone(),
                    questionnaire.CreatedUtc,
                    questionnaire.UpdatedUtc,
                    questionnaire.UpdatedBy,
                    questionnaire.IsDeleted);

                return Results.Ok(response);
            })
            .WithName("UpdateQuestionnaire")
            .WithSummary("Update a questionnaire")
            .WithDescription("Replaces an existing questionnaire for administrative users.");
    }
}
