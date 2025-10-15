using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
    }
}
