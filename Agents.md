# Project outline

A dot net application that serves client applications via REST endpoints. Users can interact with the project both in an authenticated way and in an anonymous way.  The overall purpose is to administer questionnaires.  In the authenticated way they are allowed to have two roles: questionnaire administrator and questionnaire user. The administrator can instruct the system ot administer a set questionnaire to the specific user.  In the future, administrators will be able to upload arbitrary questionnaires. Questionnaires are represente by an JSON object, following a schema defined somewhere else (out of scope for this project).

## Project level requirements

- .NET 9.0.305 up to next minor
- dotnet sdk matching the version specified above
- `Aspire` orchestration (manifest: 8.2.2/8.0.100 source: SDK 9.0.300)
- Docker used for local testing
- `Scalar` documentation
- Web API name: QuestionnaireService
- App host name: QuestionnaireHost

## Coding process

- Requirements are set before coding. No requirements, no coding
- Pause for user feedback
- Test scenarios are set before coding. No test scenarios, no coding
- Pause for user feedback
- Make or modify a class at a time
- Pause for user feedback at each class
- Make and / or run unit tests
- Propose modifications and pause for user feedback
- If the class impacts existing endpoints: run integration tests
- A new endpoint requires integration test run only after it's been wired up to the rest of the project
- Only make or change one endpoint at a time
- Separate out infrastructure changes in their own workstream
- Never delete files before user confirmation

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

### Integration testing strategy

- Each new endpoint must be tested in a local container
- Happy path and corner cases must be covered

### Unit testing strategy

- Each class must be unit tested
- Each method must be unit tested
- Happy path and corner cases must be covered
