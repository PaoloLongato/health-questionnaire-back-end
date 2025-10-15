# Feature

Expose a placeholder questionnaire retrieval endpoint for client applications.

## Functional requirements

- Provide a `GET /questionnaires/{id}` endpoint under the application endpoints.
- Accept questionnaire identifiers as GUID strings to align with future persistence plans.
- When the identifier is a valid GUID, return HTTP 200 with a JSON payload that echoes the ID and includes placeholder questionnaire metadata.
- Questionnaire metadata must contain `id`, `title`, `description`, and a `data` field representing the questionnaire JSON document.
- When the identifier is invalid, return HTTP 400 with a problem response explaining the validation failure.
- Leave the endpoint anonymous; authentication will be added later.

## Technical requirements

- Extend the `ApplicationEndpoints` module to register the new minimal API route.
- Validate IDs using `Guid.TryParse` before invoking any business logic.
- Define a response record to represent the placeholder questionnaire payload; use `JsonElement` (or equivalent) for the `data` field so JSON remains structured, not raw text.
- Produce placeholder questionnaire data by loading/parsing a well-formed JSON document that can later be swapped with persisted data.

## Test scenarios

### Valid questionnaire ID returns placeholder data

- Issue `GET /questionnaires/{guid}` with a valid GUID and assert HTTP 200 plus a JSON body containing the same GUID and non-empty placeholder fields.

### Invalid questionnaire ID is rejected

- Issue `GET /questionnaires/invalid` and assert HTTP 400 with a problem details payload mentioning the invalid identifier.
