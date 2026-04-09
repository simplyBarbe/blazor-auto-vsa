![License](https://img.shields.io/badge/license-MIT-green)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)

# Blazor Auto VSA

A clean starter for **Blazor Web Apps** with **`RenderMode.Auto`** and **Vertical Slice Architecture**.

It gives you a practical base to build feature-first apps with shared contracts, validation, auth, tests, and Docker support.

## Features

- ⚡ Blazor Auto rendering (Server + WebAssembly)
- 🧩 Vertical Slice organization by feature
- 🔁 Shared request/handler model across Client and Server
- ✅ FluentValidation pipeline (shared + server-side rules)
- 🗃️ EF Core + PostgreSQL + migrations + seeding
- 🔐 ASP.NET Core Identity cookie authentication
- 🧪 Unit and integration test setup
- 📘 OpenAPI + Scalar in development
- 🐳 Docker and docker-compose support

## Technology Stack

| Area | Technology |
|---|---|
| Runtime | .NET |
| UI | Blazor Web App + WebAssembly |
| Components | Microsoft Fluent UI |
| Validation | FluentValidation |
| API Docs | OpenAPI + Scalar |
| Data | EF Core + Npgsql |
| Mapping | AutoMapper |
| Logging | Serilog |
| Testing | xUnit + FluentAssertions + Moq |

## Project Structure

The solution is split by boundary (`Server`, `Client`, `Shared`) and by test type (`Unit`, `Integration`), while business logic is grouped by feature.

<details>
<summary>Folder tree (trimmed for readability)</summary>

```text
.
├── .github/
│   └── workflows/
├── scripts/
├── src/
│   ├── Client/
│   │   ├── Features/
│   │   ├── Dispatching/
│   │   ├── Components/
│   │   ├── Extensions/
│   │   └── Infrastructure/
│   ├── Server/
│   │   ├── Features/
│   │   ├── Infrastructure/
│   │   ├── Domain/
│   │   └── Extensions/
│   └── Shared/
│       ├── Core/
│       ├── Features/
│       └── Domain/
├── tests/
│   ├── Unit/
│   └── Integration/
├── docker-compose.yml
└── blazor-auto-vsa.slnx
```

</details>

### Vertical Slice in this repo

- `Shared/Features/<Feature>/<UseCase>`: contracts, DTOs, shared validators.
- `Server/Features/<Feature>/<UseCase>`: handlers, endpoints, server-only validators.
- `Client/Features/<Feature>`: UI and route mappings.
- `Server/Infrastructure/CRUD/*`: reusable CRUD base classes.

## Getting Started

### Prerequisites

- .NET SDK 10
- Docker Desktop (optional)
- PostgreSQL (if running without Docker)
- Python 3 + `dotnet-ef` (optional, only for migration scripts)

### Clone and run

```bash
git clone https://github.com/simplyBarbe/blazor-auto-vsa.git
cd blazor-auto-vsa
dotnet restore blazor-auto-vsa.slnx
dotnet build blazor-auto-vsa.slnx -c Debug
dotnet run --project src/Server/Server.csproj
```

Default local URLs:
- `http://localhost:5062`
- `https://localhost:7125`

### Hot reload

```bash
dotnet watch --project src/Server/Server.csproj
```

## Add a Feature (Vertical Slice)

Use `Products` as the reference slice.

1. Add request/DTO/validator in `src/Shared/Features/<Feature>/<UseCase>`.
2. Add endpoint/handler (and optional server validator) in `src/Server/Features/<Feature>/<UseCase>`.
3. Add route mapping in `src/Client/Features/<Feature>/*Routes.cs`.
4. Add UI in `src/Client/Features/<Feature>/Components`.
5. Add unit/integration tests.

Example route mapping:

```csharp
map.Map<CreateProductCommand>("/api/products", HttpMethod.Post);
```

## Configuration

Server config files:
- `src/Server/appsettings.json`
- `src/Server/appsettings.Development.json`

Client config files:
- `src/Client/wwwroot/appsettings.json`
- `src/Client/wwwroot/appsettings.Development.json`

Main keys:

| Key | Purpose |
|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `UseInMemoryDatabase` | In-memory DB mode (tests) |
| `Serilog:*` | Logging configuration |

Seeded at startup:
- sample products
- roles: `Admin`, `User`
- admin user: `admin@example.com` / `Admin123!`

## Testing

```bash
dotnet test tests/Unit/Unit.Tests.csproj
dotnet test tests/Integration/Integration.Tests.csproj
dotnet test blazor-auto-vsa.slnx --collect:"XPlat Code Coverage"
```

## Deploy

```bash
docker compose up --build
```

- Dockerfile: `src/Server/Dockerfile`
- Compose files: `docker-compose.yml`, `docker-compose.override.yml`

## Support

If you find **Blazor Auto VSA** useful, please consider giving it a star on GitHub.

For questions or support, please open an issue on GitHub.

## License

This project is licensed under the **MIT License**.
