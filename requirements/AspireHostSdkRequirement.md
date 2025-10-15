# Feature

Ensure QuestionnaireHost references Aspire.AppHost.Sdk

## Functional requirements

- QuestionnaireHost must build without Aspire SDK errors.
- Developers can run the host with `dotnet run --project QuestionnaireHost/QuestionnaireHost.csproj`.

## Technical requirements

- Add the `<Sdk Name="Aspire.AppHost.Sdk" Version="9.0.0" />` element to `QuestionnaireHost/QuestionnaireHost.csproj` immediately after the root `<Project>` declaration.
- Keep existing project properties and package references intact.

## Test scenarios

### Build host succeeds

- Run `dotnet build QuestionnaireHost/QuestionnaireHost.csproj` and observe a successful build without ASPIRE007 error.
