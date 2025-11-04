using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QuestionnaireService.Data;

public sealed class DesignTimeQuestionnaireDbContextFactory : IDesignTimeDbContextFactory<QuestionnaireDbContext>
{
    public QuestionnaireDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QuestionnaireDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__QuestionnaireDb")
            ?? "Host=localhost;Port=5432;Database=questionnaire_design;Username=postgres;Password=postgres;Trust Server Certificate=true;";

        optionsBuilder.UseNpgsql(connectionString);

        return new QuestionnaireDbContext(optionsBuilder.Options);
    }
}
