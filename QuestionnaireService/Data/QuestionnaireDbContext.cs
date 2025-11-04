using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace QuestionnaireService.Data;

public sealed class QuestionnaireDbContext(DbContextOptions<QuestionnaireDbContext> options) : DbContext(options)
{
    public DbSet<Questionnaire> Questionnaires => Set<Questionnaire>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new QuestionnaireEntityConfiguration());
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateQuestionnaires();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateQuestionnaires();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateQuestionnaires()
    {
        foreach (var entry in ChangeTracker.Entries<Questionnaire>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (string.IsNullOrWhiteSpace(entry.Entity.Title))
            {
                throw new InvalidOperationException("Questionnaire title is required.");
            }

            if (entry.Entity.Content is null)
            {
                throw new InvalidOperationException("Questionnaire content is required.");
            }

            if (entry.Entity.CreatedUtc == default)
            {
                throw new InvalidOperationException("Questionnaire must have CreatedUtc set.");
            }

            if (entry.Entity.UpdatedUtc == default)
            {
                throw new InvalidOperationException("Questionnaire must have UpdatedUtc set.");
            }
        }
    }
}
