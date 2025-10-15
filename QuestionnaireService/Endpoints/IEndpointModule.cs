using Microsoft.AspNetCore.Routing;

namespace QuestionnaireService.Endpoints;

public interface IEndpointModule
{
    void MapEndpoints(IEndpointRouteBuilder app);
}
