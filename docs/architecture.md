# Architecture

Alakai FestivalManager follows **Clean Architecture** with strict layer dependencies.

## Layer Dependencies

```
Domain
  └── Application  (references Domain)
        └── Infrastructure  (references Application + Domain)
              └── Api  (references Application + Infrastructure)
                    └── Admin  (references Api via HTTP, not project reference)
```

The Admin (Blazor Server) communicates with the Api exclusively via `HttpClient` — it has no direct reference to Application or Infrastructure assemblies. This keeps them independently deployable.

## Project Responsibilities

### Domain
Pure C# — no dependencies on any framework.
- `Entities/` — all database entities inheriting `BaseEntity` (Id, CreatedAt, UpdatedAt)
- `Enums/` — `PaymentStatus`, `PaymentPlan`, `FestivalModule`, `DanceRole`, etc.
- `Common/BaseEntity.cs` — base class with protected Id setter (auto-generated Guid)

### Application
Business logic layer. Depends only on Domain.
- `Features/{Feature}/Commands/` — write operations (Create, Update, Delete handlers)
- `Features/{Feature}/Queries/` — read operations
- `Features/{Feature}/Services/` — stateful services (PaymentService, AuthService, etc.)
- `Features/{Feature}/Validators/` — FluentValidation validators
- `Features/{Feature}/Mappings/` — AutoMapper profiles
- `Interfaces/Repositories/` — repository contracts
- `Common/Exceptions/` — `NotFoundException`, `BusinessRuleException`, `ValidationException`

### Infrastructure
Implements Application interfaces. Depends on Application + Domain.
- `Repositories/` — EF Core repository implementations
- `Configurations/` — EF entity configurations (table names, FKs, indexes)
- `Migrations/` — EF migrations
- `Extensions/InfrastructureDependencyInjectionExtension.cs` — DI registration

### Api
ASP.NET Core Web API. Thin controllers — no business logic.
- `Controllers/` — one controller per feature, delegates to handlers/services
- `Middleware/GlobalExceptionMiddleware.cs` — catches domain exceptions, returns RFC 7807 errors
- JWT Bearer authentication
- CORS configured for the Admin origin

### Admin
Blazor Server application. Single project serving both:
- **Admin panel** (`/` routes) — festival management, registrations, reports
- **User Panel** (`/user-panel/` routes) — registration form, payments, accommodation booking

Key Admin concepts:
- `ActiveFestivalState` (Scoped) — tracks the selected festival across pages
- `Components/Layout/` — shared layouts, PageHeader, Sidebar, Topbar
- `Services/Api/` — typed HttpClient wrappers for every API endpoint
- `Contracts/` — mirror DTOs matching the Api contracts

## Key Patterns

### Command/Query handlers
Handlers are plain C# classes registered as Scoped services. Controllers resolve them directly — no MediatR.

```csharp
// Controller
public async Task<IActionResult> Create([FromBody] CreateRegistrationCommand command)
{
    RegistrationDto result = await _handler.HandleAsync(command);
    return Ok(result);
}
```

### Repository pattern
All data access goes through repository interfaces. EF Core DbContext is injected into repositories (not into handlers).

### Global exception middleware
Domain exceptions are caught centrally and converted to HTTP responses:
- `NotFoundException` → 404
- `BusinessRuleException` → 422
- `ValidationException` → 400

### Multi-tenancy
Tenancy is per-deployment (separate database + API + Admin per client). No shared database. Festival selection within a deployment is managed via `ActiveFestivalState`.