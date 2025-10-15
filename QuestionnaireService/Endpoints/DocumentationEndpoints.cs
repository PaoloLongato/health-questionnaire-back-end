using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;

namespace QuestionnaireService.Endpoints;

public sealed class DocumentationEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapOpenApi();
        app.MapScalarUi();
    }
}
