# Feature

Add administrative endpoints that let privileged callers create, update, and soft-delete questionnaires stored in PostgreSQL.

## Functional requirements

- Administrators can create a new questionnaire by calling an admin endpoint that returns the newly assigned identifier and timestamps.
- Administrators can replace the contents of an existing questionnaire; if the questionnaire does not exist or has been soft-deleted, they receive a not-found response.
- Administrators can remove a questionnaire from active circulation by issuing a delete request; deleting an already removed questionnaire is harmless.
- Clients receive clear error messages when they submit invalid questionnaire data.
- Until authentication is introduced, these endpoints remain publicly callable but are documented as admin-only operations.

## Technical requirements

- Implement `POST /admin/questionnaires`, `PUT /admin/questionnaires/{id}`, and `DELETE /admin/questionnaires/{id}` within the existing `AdminEndpoints` module using minimal APIs.
- Request payload for create/update must include `title`, optional `description`, `content` JSON, and optional `updatedBy`. Validate required fields and return HTTP `400` when invalid.
- `POST` generates `Guid.NewGuid()` for the questionnaire, sets `CreatedUtc` and `UpdatedUtc` to `DateTime.UtcNow`, persists via `QuestionnaireDbContext`, and returns HTTP `201` with the stored representation.
- `PUT` retrieves the questionnaire (ignoring the global query filter), fails with `404` if the questionnaire is missing or soft-deleted, and updates mutable fields plus `UpdatedUtc`.
- `DELETE` retrieves the questionnaire ignoring the filter, sets `IsDeleted = true`, updates `UpdatedUtc`, and returns HTTP `204` regardless of prior delete state (idempotent).
- Ensure responses echo authoritative state (IDs, timestamps, `UpdatedBy`, `IsDeleted` when relevant) so clients can confirm the operation.
- Add appropriate metadata so Scalar/OpenAPI documents all three endpoints.

## Test scenarios

### Create questionnaire persists data and returns 201

1. POST to `/admin/questionnaires` with valid `title`, `content` JSON, and optional `description`/`updatedBy`.
2. Assert the response is `201 Created`, includes the new ID, and that the database row has matching data with `IsDeleted = false`.

### Create questionnaire fails validation

1. POST with missing `title` or `content`.
2. Assert the response is `400 Bad Request` with a clear validation message and that no row is written.

### Update questionnaire replaces stored content

1. Seed a questionnaire, then PUT to `/admin/questionnaires/{id}` with new `title`/`content`.
2. Assert the response is `200 OK`, timestamps change, and the database row reflects the new data.

### Update missing questionnaire returns 404

1. PUT to `/admin/questionnaires/{id}` where the ID does not exist (or is soft deleted).
2. Assert the response is `404 Not Found` and no new row is created.

### Delete questionnaire is idempotent

1. DELETE an existing questionnaire and assert `204 No Content` with `IsDeleted = true` in the database.
2. DELETE the same ID again and assert `204 No Content` with no further changes.

### Deleted questionnaires stay hidden from default queries

1. Create two questionnaires, delete one via the endpoint, then query `GET /questionnaires/{id}` (existing endpoint) and ensure only the active questionnaire is returned, while the deleted one still exists in the database with `IsDeleted = true`.
