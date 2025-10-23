# Feature

Introduce data persistence for questionnaires using Entity Framework Core backed by PostgreSQL.

## Functional requirements

- Persist questionnaire data using Entity Framework Core so administrators can assign and track questionnaires.
- Expose a `QuestionnaireDbContext` that the web API and background workloads resolve through dependency injection.
- Keep database connection details externalized via Aspire connection bindings or environment-specific configuration so credentials are never hard coded.
- Ensure the Aspire host orchestrates a PostgreSQL database container that the QuestionnaireService can reach.

## Technical requirements

- Target EF Core 9.x with the Npgsql provider aligned to the .NET 9 SDK.
- Configure `QuestionnaireDbContext` with `UseNpgsql` (or an injected `NpgsqlDataSource`) and register it through `AddDbContext` using pooled connections where appropriate.
- Resolve the connection string from Aspire-provided bindings at runtime and fail-fast when no binding is available to catch misconfiguration early.
- Centralize entity configurations using the EF Core fluent API to keep schema definitions discoverable.
- Enable EF Core design-time tooling and migrations to manage schema evolution without manual SQL.
- Surface database connection health via ASP.NET Core health checks so the host can detect connectivity issues.

## Test scenarios

### DbContext resolves with PostgreSQL provider

1. Launch the QuestionnaireService through the Aspire host so PostgreSQL is available.
2. Resolve `QuestionnaireDbContext` from the service provider in an integration test.
3. Assert `dbContext.Database.ProviderName` equals `Npgsql.EntityFrameworkCore.PostgreSQL`.

### Database connectivity health check reports healthy

1. Launch the QuestionnaireService through the Aspire host so PostgreSQL is available.
2. Issue `GET /healthz`.
3. Assert the HTTP status is 200 and the `postgresql` entry in the payload reports `Healthy`.
