using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuestionnaireService.Data;

internal sealed class QuestionnaireEntityConfiguration : IEntityTypeConfiguration<Questionnaire>
{
    public void Configure(EntityTypeBuilder<Questionnaire> builder)
    {
        builder.ToTable("questionnaires");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Title)
            .IsRequired();

        builder.Property(q => q.Description);

        builder.Property(q => q.Content)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(q => q.CreatedUtc)
            .IsRequired();

        builder.Property(q => q.UpdatedUtc)
            .IsRequired();

        builder.Property(q => q.UpdatedBy);

        builder.Property(q => q.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(q => q.Title)
            .HasDatabaseName("IX_questionnaires_title");

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}
