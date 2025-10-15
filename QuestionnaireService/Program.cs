using QuestionnaireService.Endpoints;
using QuestionnaireService.Questionnaires;
using QuestionnaireService.Status;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<ServiceStatusProvider>();
builder.Services.AddSingleton<QuestionnairePlaceholderProvider>();

// Register each endpoint module so new functionality can be added without
// modifying Program.cs directly.
builder.Services.AddSingleton<IEndpointModule, DocumentationEndpoints>();
builder.Services.AddSingleton<IEndpointModule, ServiceEndpoints>();
builder.Services.AddSingleton<IEndpointModule, ApplicationEndpoints>();
builder.Services.AddSingleton<IEndpointModule, AdminEndpoints>();

var app = builder.Build();

// Resolve and execute every endpoint module to build the HTTP surface.
var endpointModules = app.Services.GetRequiredService<IEnumerable<IEndpointModule>>();
foreach (var module in endpointModules)
{
    module.MapEndpoints(app);
}

app.Run();

public partial class Program;
