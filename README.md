# Alakai FestivalManager

Multi-tenant SaaS platform for managing dance and music festivals. Built with .NET 9, Blazor Server, Entity Framework Core and SQL Server.

## Projects

| Project | Port (dev) | Purpose |
|---|---|---|
| `Alakai.FestivalManager.Api` | 7157 | REST API — all business logic and data access |
| `Alakai.FestivalManager.Admin` | 7033 | Blazor Server — Admin panel + User Panel |
| `Alakai.FestivalManager.Application` | — | Use cases, handlers, services, validators |
| `Alakai.FestivalManager.Domain` | — | Entities, enums, domain exceptions |
| `Alakai.FestivalManager.Infrastructure` | — | EF Core, repositories, external integrations |
| `Alakai.FestivalManager.Tests` | — | xUnit unit tests |

## Quick Start

### Prerequisites
- .NET 9 SDK
- SQL Server (local or remote)
- Visual Studio 2022 / Rider / VS Code

### 1. Clone and restore
```bash
git clone <repo-url>
cd Alakai.FestivalManager
dotnet restore
```

### 2. Configure the API
Copy `appsettings.json` to `appsettings.Development.json` in `Alakai.FestivalManager.Api` and set:
- `ConnectionStrings:DefaultConnection` — your SQL Server connection string
- `Jwt:SecretKey` — any random string ≥ 32 characters
- `Email:*` — your SMTP credentials

### 3. Run migrations
```bash
cd Alakai.FestivalManager.Infrastructure
dotnet ef database update --startup-project ../Alakai.FestivalManager.Api
```

### 4. Start both projects
```bash
# Terminal 1
dotnet run --project Alakai.FestivalManager.Api

# Terminal 2
dotnet run --project Alakai.FestivalManager.Admin
```

Open `https://localhost:7033` for the Admin panel.  
API Swagger: `https://localhost:7157/swagger`

## Architecture

See [docs/architecture.md](docs/architecture.md)

## Configuration Reference

See [docs/configuration.md](docs/configuration.md)

## Production Deployment

See [docs/production-checklist.md](docs/production-checklist.md)

## Modules

See [docs/modules.md](docs/modules.md)

## Testing

See [docs/testing.md](docs/testing.md)

## Clients

The platform is multi-tenant. Each client deployment has its own:
- SQL Server database
- API instance
- Admin instance
- Domain/subdomain

Current clients: **La Jam Barcelona**, **Swim Out Costa Brava**.