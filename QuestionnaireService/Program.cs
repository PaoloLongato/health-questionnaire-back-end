using QuestionnaireService.Status;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ServiceStatusProvider>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarUi();

app.MapGet("/status", (ServiceStatusProvider provider) =>
    Results.Ok(provider.GetStatus()))
    .WithName("GetStatus")
    .WithSummary("Returns service metadata for QuestionnaireService.")
    .Produces<ServiceStatus>();

app.MapHealthChecks("/healthz");

app.Run();

public partial class Program;
