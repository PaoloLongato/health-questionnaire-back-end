# Feature

Persist questionnaires in PostgreSQL so administrators can assign and manage them while clients continue reading historical content.

## Functional requirements

- Store each questionnaire with its identifier, title, description, JSON payload, and auditing metadata (created/updated timestamps).
- Support soft deletion: a questionnaire flagged as deleted stays queryable by identifier (for historical reads) but must not appear in list operations.
- Ensure questionnaire JSON remains retrievable exactly as provided so downstream clients can render past versions.
- Record who performed the last modification when the information is available (optional field for future enrichment).

## Technical requirements

- Model a `Questionnaire` entity in EF Core with the following columns:
  - `Id` (`uuid`, PK)
  - `Title` (`text`, required)
  - `Description` (`text`, optional)
  - `Content` (`jsonb`, required) storing the questionnaire JSON
  - `CreatedUtc` (`timestamp with time zone`, required)
  - `UpdatedUtc` (`timestamp with time zone`, required)
  - `UpdatedBy` (`text`, optional) to capture the last modifier identity
  - `IsDeleted` (`boolean`, required, defaults to `false`)
- Configure EF Core mappings via fluent configuration and expose the entity through `QuestionnaireDbContext`.
- Generate and check in migrations so Aspire can apply them on startup.
- Default queries (e.g., `DbSet<Questionnaire>`) must filter out soft-deleted rows using a global query filter.
- Provide methods to mark a questionnaire as deleted without removing the row physically.
- Validate that required fields (title, content, created/updated timestamps) are present before saving and surface failures clearly, while allowing `UpdatedBy` to remain null.

## Test scenarios

### Migration creates questionnaire table

Apply the initial migration to a fresh PostgreSQL instance and verify the `questionnaires` table exists with the expected columns, including `IsDeleted`.

### Soft-deleted questionnaire excluded from lists but still accessible by ID

Create two questionnaires, soft-delete one, and query all active questionnaires—only the non-deleted entry should appear. Fetching by ID should return both entries, with the deleted one flagged appropriately.

### Stored content round-trips JSON accurately

Persist a questionnaire whose payload includes nested arrays/objects. Reload it and assert the retrieved JSON matches the original payload byte-for-byte.

### Validation rejects missing required fields

Attempt to save questionnaires missing title, content, or timestamps via the EF entity (or repository) and assert EF Core/database constraints throw, preventing bad rows from being persisted.

### UpdatedBy remains optional

Persist a questionnaire without specifying `UpdatedBy` and verify the column remains null, then update it with a value and ensure it saves correctly.

### Soft delete is idempotent

Soft-delete a questionnaire twice and assert the second call does not throw and the `IsDeleted` flag remains true.
