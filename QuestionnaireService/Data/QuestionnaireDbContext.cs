using Microsoft.EntityFrameworkCore;
using QuestionnaireService.Questionnaires;

namespace QuestionnaireService.Data;

public class QuestionnaireDbContext(DbContextOptions<QuestionnaireDbContext> options) : DbContext(options)
{
    public DbSet<QuestionnaireDetails> Questionnaires => Set<QuestionnaireDetails>();
}