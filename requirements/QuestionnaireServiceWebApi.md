# Feature

Create the initial QuestionnaireService web API that mobile and other clients can call.

## Functional requirements

- Expose a REST API named QuestionnaireService that mobile and other HTTP clients can reach anonymously.
- Provide a `GET /status` endpoint that reports the service name, environment, and version so clients can confirm connectivity.
- Provide a health check surfaced at `GET /healthz` so hosting infrastructure and containers can verify liveness.
- Host Scalar API documentation at `/scalar` and ensure the documented endpoints above are visible.
- Make the service discoverable from the QuestionnaireHost Aspire application.

## Technical requirements

- Implement the service as a new ASP.NET Core project named `QuestionnaireService` targeting .NET 9.
- Configure minimal API endpoints grouped under `/` using conventional ASP.NET Core routing.
- Register and use a singleton service that supplies the status response (service name, environment, version) so it can be unit tested without hitting HTTP.
- Enable ASP.NET Core health checks and map them to `/healthz`.
- Register Scalar using `MapScalar` so documentation stays in sync with minimal API metadata.
- Update `health-questionnaire-back-end.sln` and `QuestionnaireHost` to orchestrate the new service via Aspire.

## Test scenarios

### Status endpoint returns service metadata

- Call `GET /status` and assert the body includes the expected service name and non-empty environment and version values.

### Health endpoint reports healthy

- Call `GET /healthz` and assert the response is HTTP 200 with overall status `Healthy`.
