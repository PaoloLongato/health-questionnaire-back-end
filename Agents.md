# Project outline

A dot net application that serves client applications via REST endpoints. Users can interact with the project both in an authenticated way and in an anonymous way.  The overall purpose is to administer questionnaires.  In the authenticated way they are allowed to have two roles: questionnaire administrator and questionnaire user. The administrator can instruct the system ot administer a set questionnaire to the specific user.  In the future, administrators will be able to upload arbitrary questionnaires. Questionnaires are represente by an JSON object, following a schema defined somewhere else (out of scope for this project).

## Project level requirements

- .NET 9.0.305 up to next minor
- dotnet sdk matching the version specified above
- `Aspire` orchestration (manifest: 8.2.2/8.0.100 source: SDK 9.0.300)
- `Docker` as a runtime container
- `Scalar` documentation
- `PostgreSQL` via `Entity Framework`
- Web API name: QuestionnaireService
- App host name: QuestionnaireHost

## Coding process

### Coding steps in order of execution

BDD → ATDD → TDD → Red → Green → Refactor

1 - Requirements are set
2 - Test scenarios are set after requirements are set
3 - Testing code is written after test scenarios are set
4 - Write or refactor code, one class at a time
5 - Run tests
6 - If tests don't pass, go to 4

### Strict rules

- Between each of the coding steps, pause and wait for user feedback
- Initial tests are written using dummy implementation and extensive comments
- After modifying or adding a class, pause, explain why and wait for user feedback
- Never delete files before user confirmation
- Run all tests using `dotnet test` and present `⚙️ testing`
- Present test results as `🔴 failure` for failure, `🟢 success` for success
- If you cannot run tests automatically, ask the user to run manually, provide instructions and ask to copy / paste logs of the results

## Requirements folder structure

| Folder | Content |
| :-- | :- |
| ./requirements | `index.md`, a file containing links to all other feature files and brief descriptions |
| ./requirements | `<feature name>.md`, requirements files |
| ./documentation | `index.md`, a file containing links to all other documentation files and brief descriptions |
| ./documentation | `<documentation file name>.md`, a file containing a piece of the documentation |

## Requirement files template

```markdown
# Feature 

Describe

## Functional requirements

Describe what it needs to achieve

## Technical requirements

Describe how it needs to achieve the what

## Test scenarios

### <Case Description>

Describe the test to be performed

### <Case Description>

Describe the test to be performed
...
```

## Coding Conventions

- TBD

## Testing Requirements

Tests live in their own `QuestionnaireService.UnitTests.csproj`

### Integration testing strategy

- Each new endpoint must be tested in a local container
- Happy path and corner cases must be covered

### Unit testing strategy

- Mocks should be used, so that the user can run tests during coding
- Each class must be unit tested
- Each method must be unit tested
- Happy path and corner cases must be covered
