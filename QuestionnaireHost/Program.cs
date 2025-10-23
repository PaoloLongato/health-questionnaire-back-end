var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("questionnaire-db")
    .AddDatabase("QuestionnaireDb");

var service = builder
    .AddProject<Projects.QuestionnaireService>("questionnaire-service")
    .WithReference(postgres);

builder.Build().Run();
