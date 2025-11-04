# Feature

Expose a public endpoint that returns all active questionnaires with their metadata so clients can discover available surveys without downloading the full JSON payload.

## Functional requirements

- Clients can call the endpoint without authentication to receive a list of questionnaires that are not soft-deleted.
- Each item in the list includes only metadata. The JSON content is intentionally omitted to keep responses lightweight.
- Results are ordered from newest to oldest based on `UpdatedUtc` so clients see the freshest questionnaires first.

## Technical requirements

- Implement `GET /questionnaires` inside the existing application endpoints module.
- Query the `QuestionnaireDbContext` with the global filter in place so soft-deleted rows are automatically excluded.
- Project the results into a lightweight DTO (id, title, description, updated timestamps, updatedBy) and return HTTP `200 OK` with the list.
- The endpoint should return an empty array when no questionnaires exist; this is not considered an error.
- Ensure Scalar/OpenAPI metadata describes the endpoint and the response shape.

## Test scenarios

### Returns all active questionnaires ordered by UpdatedUtc descending

1. Seed multiple questionnaires with varying `UpdatedUtc` values and one soft-deleted entry.
2. Call `GET /questionnaires`.
3. Assert the response is `200 OK`, includes only the active questionnaires, and they are ordered newest to oldest.

### Returns empty list when no questionnaires exist

1. Ensure the database has no questionnaires.
2. Call `GET /questionnaires`.
3. Assert the response is `200 OK` with an empty array.
