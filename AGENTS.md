# Repository Guidelines

## Project Structure & Module Organization
This solution is organized by app and test layers:
- `src/Server`: ASP.NET Core host, endpoints, EF Core data access, auth, and infrastructure. Reusable CRUD behavior (base handlers, endpoints, validators) lives under `Infrastructure/CRUD/`.
- `src/Client`: Blazor WebAssembly UI (`Features/*`, `Components/*`, `Layout/*`). Also: `Pages/`, `Dispatching/` (e.g. `HttpRequestSender`, `RequestEndpointMapper`), `Extensions/`, `Infrastructure/`.
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
- `dotnet test blazor-auto-vsa.slnx --collect:\"XPlat Code Coverage\"`: collect coverage via Coverlet.
- `docker compose up --build`: run app dependencies/containers.

## Command contracts (Shared)
Commands are **write** contracts: include only properties the server **persists or applies** for that operation (including fields required for validation or business rules that are part of the write).

- Do **not** add properties used only for UI state, cascading selects, or convenience when they are not written by the handler (for example a redundant parent key when a single foreign key already identifies the row). Resolve those on the client (e.g. with a read query) or derive them server-side from the persisted key.

## Client UI conventions

Small rules that have been debugged into existence. Follow them to avoid regressions.

### Async UI state
- Use `AsyncState` / `AsyncState<T>` for any async work that affects rendering. Create them via `UseAsyncState()` / `UseAsyncState<T>()` on `BaseComponent` so subscription and disposal are automatic.
- Bind `Disabled` / loading UI to `state.IsPending`; bind content to `state.Data`. Do not keep parallel `bool _loading` flags.

### Cascading selects in dialogs
- Do **not** put `Required="true"` on a `FluentSelect` whose value is set programmatically (e.g. preselected from a parent). Rely on the command validator (e.g. `GroupId > 0`) and render `<FluentValidationMessage For="..." />` next to the control.
- Centralize dialog initialization in a single `AlignSelectionWithContentAsync` method with two explicit branches: resolve-from-existing (edit mode) and preselect-first (add mode, `Content.Id == 0` / key == 0). Guard programmatic selection with an `_isResolvingDialogSelection` flag so `SelectedOptionChanged` callbacks don't clobber state.

### Paged grids
- Use `PagedGridController<T>` + `PagedDataGrid<T>`. The controller is the single source of truth for `Items`, `TotalCount`, `IsPending`, `HasItems`, `HasNoResults`, and `CanPaginate`; the view reads those directly instead of duplicating flags.
- After a dialog add/edit, call `grid.RefreshAsync()` (no page reset) so the user stays on the current page and filter. Use `RefreshAsync(resetToFirstPage: true)` only when filters change.

### Shared vs. server-only validation
- Shared validators (in `src/Shared/**`) run on both client and server; keep them lenient enough that add-mode dialogs (where IDs are `0`) pass. Put strict database-level rules (e.g. `Id > 0`, uniqueness, existence checks) in a server-only `*ServerValidator` that `Include`s the shared one.

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
