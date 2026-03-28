# Repository Guidelines

## Project Structure & Module Organization
This solution is organized by app and test layers:
- `src/Server`: ASP.NET Core host, endpoints, EF Core data access, auth, and infrastructure. Reusable CRUD behavior (base handlers, endpoints, validators) lives under `Infrastructure/CRUD/`.
- `src/Client`: Blazor WebAssembly UI (`Features/*`, `Components/*`, `Layout/*`). Also: `Pages/`, `Dispatching/` (e.g. `HttpRequestSender`, `RequestEndpointMapper`), `Extensions/`, `Infrastructure/`. UI components use `Microsoft.FluentUI.AspNetCore.Components` v5 APIs.
- `src/Shared`: contracts, request/response models, validators, and cross-layer abstractions.
- `tests/Unit`: fast unit tests for handlers, validators, dispatching, and shared behavior.
- `tests/Integration`: API/infrastructure tests using `WebApplicationFactory`; they use an in-memory database and the `Testing` environment—no external DB required.
- `scripts`: helper scripts for EF migrations.

Keep feature code grouped by domain (for example `Features/Products/Create`, `Get`, `List`, `Update`, `Delete`) across `Server`, `Client`, and `Shared`.

**Request/handler architecture:** Requests use `IRequest`/`IRequestHandler` (Shared). The client sends via `IRequestSender` over HTTP; the server dispatches in-process. Handlers and server-only validators live on the Server; shared validators and DTOs live in Shared.

## Build, Test, and Development Commands
- `dotnet restore blazor-auto-vsa.slnx`: restore all solution dependencies.
- `dotnet build blazor-auto-vsa.slnx -c Debug`: compile all projects.
- `dotnet run --project src/Server/Server.csproj`: start the full stack from the server host.
- `dotnet watch --project src/Server/Server.csproj`: run with hot reload during development.
- `dotnet test tests/Unit/Unit.Tests.csproj`: run unit tests.
- `dotnet test tests/Integration/Integration.Tests.csproj`: run integration tests.
  - Note: this project currently references `tests/TestCommon/TestCommon.csproj`, which is missing in this snapshot and can produce restore/build warnings.
- `dotnet test blazor-auto-vsa.slnx --collect:\"XPlat Code Coverage\"`: collect coverage via Coverlet.
- `docker compose up --build`: run app dependencies/containers.

## Coding Style & Naming Conventions
Use C# conventions already present in the repo:
- 4-space indentation, nullable enabled, implicit usings enabled.
- `PascalCase` for types, components, methods, and public members.
- `camelCase` for locals and parameters; interfaces use `I` prefix (for example `IRequestSender`).
- Place files near their feature area and use descriptive names (`CreateProductCommandValidator.cs`).

## Testing Guidelines
Testing stack: xUnit + FluentAssertions + Moq.
- Name test files as `*Tests.cs`.
- Follow behavior-style method names: `Handle_should_apply_custom_paging`.
- Prefer focused unit tests in `tests/Unit`; use `tests/Integration` for end-to-end endpoint and pipeline behavior.

## Commit & Pull Request Guidelines
Recent history uses short, imperative commit subjects (for example `tests`, `clean`, `folder structure`).
- Keep commit titles brief and action-oriented.
- Scope each commit to a single logical change.
- PRs should include: purpose, affected areas (`Client`/`Server`/`Shared`), test evidence (`dotnet test` output), and screenshots for UI changes.
