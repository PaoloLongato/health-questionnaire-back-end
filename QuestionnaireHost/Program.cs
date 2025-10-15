var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.QuestionnaireService>("questionnaire-service");

builder.Build().Run();
