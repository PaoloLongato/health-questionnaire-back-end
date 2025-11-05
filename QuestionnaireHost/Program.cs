var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres("questionnaire-db")
    .AddDatabase("QuestionnaireDb");

var migrator = builder
    .AddProject<Projects.QuestionnaireMigrator>("questionnaire-migrator")
    .WithReference(postgres);

var service = builder
    .AddProject<Projects.QuestionnaireService>("questionnaire-service")
    .WithReference(postgres)
    .WithReference(migrator);

builder.Build().Run();
