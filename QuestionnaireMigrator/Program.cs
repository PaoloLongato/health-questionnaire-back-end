using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestionnaireService.Data;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:QuestionnaireDb"]
    ?? throw new InvalidOperationException("PostgreSQL connection string not configured.");

builder.Services.AddDbContext<QuestionnaireDbContext>(options =>
    options.UseNpgsql(connectionString));

using var host = builder.Build();

await using var scope = host.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<QuestionnaireDbContext>();

Console.WriteLine("Applying database migrations...");
await dbContext.Database.MigrateAsync();
Console.WriteLine("Database migrations completed successfully.");
