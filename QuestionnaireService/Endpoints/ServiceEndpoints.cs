using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace QuestionnaireService.Endpoints;

public sealed class ServiceEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    status = report.Status.ToString(),
                    totalDuration = report.TotalDuration,
                    entries = report.Entries.ToDictionary(
                        entry => entry.Key,
                        entry => new
                        {
                            status = entry.Value.Status.ToString(),
                            description = entry.Value.Description,
                            duration = entry.Value.Duration,
                            tags = entry.Value.Tags
                        })
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        });
    }
}
