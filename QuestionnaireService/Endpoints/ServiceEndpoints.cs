using Microsoft.AspNetCore.Routing;

namespace QuestionnaireService.Endpoints;

public sealed class ServiceEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/healthz");
    }
}
