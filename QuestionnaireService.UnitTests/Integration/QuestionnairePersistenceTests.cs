using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuestionnaireService.Data;
using Testcontainers.PostgreSql;

namespace QuestionnaireService.UnitTests.Integration;

public sealed class QuestionnairePersistenceTests
{
    private static PostgreSqlContainer CreateContainer(string databaseName) =>
        new PostgreSqlBuilder()
            .WithDatabase(databaseName)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    [Fact]
    public async Task Migration_CreatesQuestionnairesTable_WithExpectedColumns()
    {
        await using var container = CreateContainer($"questionnaires_{Guid.NewGuid():N}");
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<QuestionnaireDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        await using (var context = new QuestionnaireDbContext(options))
        {
            await context.Database.MigrateAsync();
        }

        await using (var verificationContext = new QuestionnaireDbContext(options))
        {
            await using var connection = (Npgsql.NpgsqlConnection)verificationContext.Database.GetDbConnection();
            await connection.OpenAsync();

            await using (var tableCommand = new Npgsql.NpgsqlCommand("SELECT table_name::text FROM information_schema.tables WHERE table_schema = 'public'", connection))
            await using (var reader = await tableCommand.ExecuteReaderAsync())
            {
                var tableNames = new List<string>();
                while (await reader.ReadAsync())
                {
                    tableNames.Add(reader.GetString(0));
                }

                Assert.Contains("questionnaires", tableNames);
            }

            await using (var columnCommand = new Npgsql.NpgsqlCommand(
                         """
                         SELECT column_name::text, data_type
                         FROM information_schema.columns
                         WHERE table_schema = 'public' AND table_name = 'questionnaires'
                         """, connection))
            await using (var columnReader = await columnCommand.ExecuteReaderAsync())
            {
                var columns = new List<(string ColumnName, string DataType)>();
                while (await columnReader.ReadAsync())
                {
                    columns.Add((columnReader.GetString(0), columnReader.GetString(1)));
                }

                var normalized = columns
                    .Select(c => (Name: c.ColumnName.ToLowerInvariant(), DataType: c.DataType))
                    .ToList();

                Assert.Contains(normalized, c => c.Name == "title" && c.DataType == "text");
                Assert.Contains(normalized, c => c.Name == "content" && c.DataType == "jsonb");
                Assert.Contains(normalized, c => c.Name == "isdeleted" && c.DataType == "boolean");
                Assert.Contains(normalized, c => c.Name == "updatedby" && c.DataType == "text");
            }
        }
    }

    [Fact]
    public async Task SoftDelete_FiltersFromQueryButAccessibleById()
    {
        await using var container = CreateContainer($"questionnaires_{Guid.NewGuid():N}");
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<QuestionnaireDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        await using (var context = new QuestionnaireDbContext(options))
        {
            await context.Database.MigrateAsync();

            var activeContent = JsonDocument.Parse("""{"version":1}""");
            var deletedContent = JsonDocument.Parse("""{"version":2}""");

            context.Questionnaires.AddRange(
                new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = "Active questionnaire",
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                    Content = activeContent
                },
                new Questionnaire
                {
                    Id = Guid.NewGuid(),
                    Title = "Deleted questionnaire",
                    CreatedUtc = DateTime.UtcNow,
                    UpdatedUtc = DateTime.UtcNow,
                    Content = deletedContent,
                    IsDeleted = true
                });

            await context.SaveChangesAsync();
        }

        await using (var activeContext = new QuestionnaireDbContext(options))
        {
            var active = await activeContext.Questionnaires.ToListAsync();
            Assert.Single(active);
            Assert.Equal("Active questionnaire", active[0].Title);

            var all = await activeContext.Questionnaires
                .IgnoreQueryFilters()
                .ToListAsync();

            Assert.Equal(2, all.Count);
            Assert.Contains(all, q => q.IsDeleted);
        }
    }

    [Fact]
    public async Task SoftDelete_IsIdempotent()
    {
        await using var container = CreateContainer($"questionnaires_{Guid.NewGuid():N}");
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<QuestionnaireDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        var questionnaireId = Guid.NewGuid();

        await using (var context = new QuestionnaireDbContext(options))
        {
            await context.Database.MigrateAsync();
            context.Questionnaires.Add(new Questionnaire
            {
                Id = questionnaireId,
                Title = "Idempotent",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                Content = JsonDocument.Parse("""{"idempotent":true}""")
            });
            await context.SaveChangesAsync();
        }

        await using (var deleteContext = new QuestionnaireDbContext(options))
        {
            var questionnaire = await deleteContext.Questionnaires
                .IgnoreQueryFilters()
                .SingleAsync(q => q.Id == questionnaireId);

            questionnaire.IsDeleted = true;
            await deleteContext.SaveChangesAsync();

            questionnaire.IsDeleted = true;
            await deleteContext.SaveChangesAsync();
        }

        await using (var verificationContext = new QuestionnaireDbContext(options))
        {
            var questionnaire = await verificationContext.Questionnaires
                .IgnoreQueryFilters()
                .SingleAsync(q => q.Id == questionnaireId);

            Assert.True(questionnaire.IsDeleted);
        }
    }

    [Fact]
    public async Task Validation_RejectsMissingRequiredFields()
    {
        await using var container = CreateContainer($"questionnaires_{Guid.NewGuid():N}");
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<QuestionnaireDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        await using var context = new QuestionnaireDbContext(options);
        await context.Database.MigrateAsync();

        var questionnaire = new Questionnaire
        {
            Id = Guid.NewGuid(),
            Title = "",
            Content = null!,
            CreatedUtc = default,
            UpdatedUtc = default
        };

        context.Questionnaires.Add(questionnaire);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Contains("Questionnaire title is required", exception.Message);
    }

    [Fact]
    public async Task UpdatedBy_RemainsOptional()
    {
        await using var container = CreateContainer($"questionnaires_{Guid.NewGuid():N}");
        await container.StartAsync();

        var options = new DbContextOptionsBuilder<QuestionnaireDbContext>()
            .UseNpgsql(container.GetConnectionString())
            .Options;

        var questionnaireId = Guid.NewGuid();

        await using (var context = new QuestionnaireDbContext(options))
        {
            await context.Database.MigrateAsync();

            context.Questionnaires.Add(new Questionnaire
            {
                Id = questionnaireId,
                Title = "Optional updater",
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                Content = JsonDocument.Parse("""{"optional":true}"""),
                UpdatedBy = null
            });

            await context.SaveChangesAsync();
        }

        await using (var verificationContext = new QuestionnaireDbContext(options))
        {
            var entity = await verificationContext.Questionnaires
                .IgnoreQueryFilters()
                .SingleAsync(q => q.Id == questionnaireId);

            Assert.Null(entity.UpdatedBy);

            entity.UpdatedBy = "admin@example.com";
            entity.UpdatedUtc = DateTime.UtcNow;

            await verificationContext.SaveChangesAsync();
        }

        await using (var postUpdateContext = new QuestionnaireDbContext(options))
        {
            var entity = await postUpdateContext.Questionnaires
                .IgnoreQueryFilters()
                .SingleAsync(q => q.Id == questionnaireId);

            Assert.Equal("admin@example.com", entity.UpdatedBy);
        }
    }
}
