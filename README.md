# Health Questionnaire Back-end

A small .NET project exploring programmatic workflows with CODEX, formal development steps (requirements, design, implementation), unit and integration testing, and experimentation with the Aspire orchestration framework.

## Goals

- Experiment with using CODEX to assist .NET development tasks.
- Define and follow formal development steps: requirements → design → implementation → tests → review.
- Constrain the coding agent to follow the steps and, possibly, validate each step with the user before proceeding to the next step.
- Evaluate Aspire for orchestration.
- Provide a back end for generic JSON payloads representing a health questionnaire

## Development workflow

1. Requirements: capture funcional and technical reuquirements as well as testing scenarios.
2. Design: produce small, testable components and API contracts.
3. Implementation: keep commits focused.
4. Tests: write unit tests for logic and integration tests for end-to-end scenarios.
5. Review & merge: you should run build + tests before merging.

### Using CODEX

General rules and workflow are contained in `AGENTS.md`. Although generally the agent will follow the workflow, it has the tendency to implement all steps of the worflow in one go.  

Mitigants:

- Keep the scope of your work small.
- Remind the agent that it should respect manual input checkpoints.

Known inssues:

- CODEX does not seem able to run tests, it encounters a time out. Manual run gives no issue.

## Testing

- Run all tests:

  ```shell
  dotnet test
  ```

## Contributing

- Open an issue to discuss changes or propose features.
- Follow the development workflow and include tests for new behavior.

## License

GNU AGPL v3
