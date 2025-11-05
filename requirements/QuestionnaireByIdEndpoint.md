# Feature

Expose a questionnaire retrieval endpoint that returns persisted questionnaire data to client applications.

## Functional requirements

- Provide an endpoint for retrieving a single questionnaire.
- Leave the endpoint anonymous.

## Technical requirements

- Provide a `GET /questionnaires/{id}` endpoint under the application endpoints.
- When the identifier matches an existing questionnaire that is not soft-deleted, return HTTP 200 with the persisted metadata (`id`, `title`, `description`, `updatedUtc`, `updatedBy`) and the questionnaire JSON content.
- When the identifier is valid but the questionnaire does not exist or has been soft-deleted, return HTTP 404.
- When the identifier is invalid, return HTTP 400 with a problem response explaining the validation failure.
- Accept questionnaire identifiers as GUID strings.
- Validate IDs using `Guid.TryParse` before hitting the database.
- Extend the `ApplicationEndpoints` module to register the minimal API route.
- Resolve the questionnaire via `QuestionnaireDbContext` (letting the global query filter exclude soft-deleted rows).
- Map the entity to a response DTO that includes metadata and the stored JSON content (convert `JsonDocument` to `JsonElement`).
- Return 404 when no questionnaire is found; return 400 before hitting the database if the ID is not a GUID.

## Test scenarios

### Existing questionnaire returns persisted data

1. Seed a questionnaire with known metadata and JSON content.
2. Issue `GET /questionnaires/{id}` with the seeded ID.
3. Assert HTTP 200 and verify the response matches the stored metadata and JSON content.

### Missing questionnaire returns 404

1. Issue `GET /questionnaires/{guid}` with a GUID that is not in the database.
2. Assert HTTP 404.

### Soft-deleted questionnaire is hidden

1. Seed a questionnaire, mark it soft-deleted, then request it by ID.
2. Assert HTTP 404.

### Invalid questionnaire ID is rejected

1. Issue `GET /questionnaires/invalid`.
2. Assert HTTP 400 with a problem details payload mentioning the invalid identifier.
