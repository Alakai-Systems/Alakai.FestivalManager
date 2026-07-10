# Creates complete documentation for Alakai FestivalManager
# Run from repo root: .\create_documentation.ps1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

New-Item -ItemType Directory -Path "docs" -Force | Out-Null

# ── README.md ─────────────────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("README.md", @'
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
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: README.md"

# ── docs/architecture.md ──────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\architecture.md", @'
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
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/architecture.md"

# ── docs/configuration.md ────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\configuration.md", @'
# Configuration Reference

All configuration lives in `appsettings.json` (committed, safe defaults) and `appsettings.Production.json` (not committed, secrets).

## Api — appsettings.json

### ConnectionStrings
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;"
}
```
Standard SQL Server connection string. Uses EF Core + SQL Server provider.

### Jwt
```json
"Jwt": {
  "Issuer": "AlakaiFestivalManager",
  "Audience": "AlakaiFestivalManager",
  "SecretKey": "CHANGE_IN_PRODUCTION",
  "ExpirationMinutes": 120,
  "RefreshTokenExpirationDays": 30
}
```
- `SecretKey` must be ≥ 32 characters. Used to sign JWT tokens.
- `ExpirationMinutes` — access token lifetime.
- `RefreshTokenExpirationDays` — refresh token lifetime.

### Redsys (payment gateway)
```json
"Redsys": {
  "MerchantCode": "367830569",
  "Terminal": "2",
  "Currency": "978",
  "SecretKey": "sq7HjrUOBfKmC576ILgskD5srU870gJ7",
  "MerchantName": "Windy Hoppers Emporda",
  "PaymentUrl": "https://sis-t.redsys.es:25443/sis/realizarPago",
  "NotificationUrl": "https://YOUR-API-DOMAIN/api/payments/redsys/notification",
  "UrlOk": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ok",
  "UrlKo": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ko"
}
```
- `PaymentUrl` — test: `https://sis-t.redsys.es:25443/sis/realizarPago`, production: `https://sis.redsys.es/sis/realizarPago`
- `NotificationUrl` — must be a publicly reachable HTTPS URL (Redsys calls this server-to-server)
- `MerchantCode`, `Terminal`, `SecretKey` — provided by your bank/Redsys contract

### Email (SMTP)
```json
"Email": {
  "Host": "mail.alakai-systems.com",
  "Port": 587,
  "UserName": "info@alakai-systems.com",
  "Password": "...",
  "UseSSL": true,
  "FromEmail": "info@alakai-systems.com",
  "FromName": "Festival Manager"
}
```

### FileStorage
```json
"FileStorage": {
  "RootPath": "wwwroot/uploads/email-images",
  "PublicBaseUrl": "https://YOUR-API-DOMAIN/uploads/email-images"
}
```
Stores images uploaded via the email template editor. `PublicBaseUrl` must be the public URL of the Api.

### ExternalAuth
```json
"ExternalAuth": {
  "Google": { "ClientId": "..." },
  "Apple": { "ClientId": "" }
}
```
Google OAuth2 client ID for social login in the User Panel.

### ApplicationUrls
```json
"ApplicationUrls": {
  "PortalUrl": "https://YOUR-ADMIN-DOMAIN/user-panel"
}
```
Used in emails as the link to the User Panel.

### GoogleAnalytics
```json
"GoogleAnalytics": {
  "CredentialsPath": "/path/to/service-account.json"
}
```
Path to the Google service account JSON file for GA4 data API access.

---

## Admin — appsettings.json

### ApiSettings
```json
"ApiSettings": {
  "BaseUrl": "https://YOUR-API-DOMAIN/"
}
```
The Admin uses this to call the Api. Must include trailing slash.

### ExternalAuth
```json
"ExternalAuth": {
  "GoogleClientId": "...",
  "AppleClientId": "",
  "AppleRedirectUri": ""
}
```
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/configuration.md"

# ── docs/production-checklist.md ─────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\production-checklist.md", @'
# Production Checklist

Everything that must be changed before deploying to production. Go through this list top to bottom.

---

## 1. API — appsettings.Production.json

Create this file in `Alakai.FestivalManager.Api/` (never commit it):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PROD-SERVER;Database=PROD-DB;User Id=PROD-USER;Password=PROD-PASS;TrustServerCertificate=True"
  },
  "Jwt": {
    "SecretKey": "<random string, minimum 32 characters, never share>"
  },
  "Redsys": {
    "MerchantCode": "<your production merchant code from bank>",
    "Terminal": "<your production terminal number>",
    "SecretKey": "<your production Redsys secret key>",
    "PaymentUrl": "https://sis.redsys.es/sis/realizarPago",
    "NotificationUrl": "https://YOUR-API-DOMAIN/api/payments/redsys/notification",
    "UrlOk": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ok",
    "UrlKo": "https://YOUR-ADMIN-DOMAIN/user-panel/dashboard?payment=ko"
  },
  "Email": {
    "Host": "<production SMTP host>",
    "Port": 587,
    "UserName": "<production email>",
    "Password": "<production email password>",
    "FromEmail": "<production from email>",
    "FromName": "<festival name>"
  },
  "FileStorage": {
    "PublicBaseUrl": "https://YOUR-API-DOMAIN/uploads/email-images"
  },
  "ApplicationUrls": {
    "PortalUrl": "https://YOUR-ADMIN-DOMAIN/user-panel"
  },
  "GoogleAnalytics": {
    "CredentialsPath": "/var/secrets/ga-service-account.json"
  }
}
```

---

## 2. Admin — appsettings.Production.json

Create this file in `Alakai.FestivalManager.Admin/` (never commit it):

```json
{
  "ApiSettings": {
    "BaseUrl": "https://YOUR-API-DOMAIN/"
  },
  "ExternalAuth": {
    "GoogleClientId": "<production Google OAuth client ID>"
  }
}
```

---

## 3. Code changes required

### CORS (Api/Program.cs)
Change the hardcoded Admin origin:
```csharp
// Line ~82 — change localhost:7033 to your production Admin domain
policy.WithOrigins("https://YOUR-ADMIN-DOMAIN")
```

### Password reset URL (Application/Features/Auth/Services/AuthService.cs)
Change the hardcoded localhost URL:
```csharp
// Line ~283 — change to your production Admin domain
string resetPasswordUrl = $"https://YOUR-ADMIN-DOMAIN/user-panel/reset-password?token={encodedToken}";
```

### EmailNotificationApiClient (Admin/Extensions/ApplicationDependencyInjectionExtension.cs)
One HttpClient still has a hardcoded localhost URL (~line 192). Change to use config:
```csharp
// Change:
client.BaseAddress = new Uri("https://localhost:7157/");
// To:
client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);
```

---

## 4. Redsys — test vs production

| Setting | Test | Production |
|---|---|---|
| `PaymentUrl` | `https://sis-t.redsys.es:25443/sis/realizarPago` | `https://sis.redsys.es/sis/realizarPago` |
| `MerchantCode` | Test merchant code | Real merchant code from bank |
| `SecretKey` | Test key | Real key from bank |
| `NotificationUrl` | Any (not called in test) | Must be publicly reachable HTTPS |

The `NotificationUrl` is called server-to-server by Redsys to confirm payments. It **must** be reachable from the internet — localhost will not work.

---

## 5. Google Analytics

1. Create a Google Cloud service account with GA4 Data API access
2. Download the JSON credentials file
3. Set `GoogleAnalytics:CredentialsPath` to the path on the server
4. Set `GoogleAnalyticsPropertyId` on each Festival record in the database

---

## 6. Google OAuth (Social Login)

1. In Google Cloud Console, add your production domain to authorised redirect URIs
2. Update `ExternalAuth:Google:ClientId` in Api appsettings
3. Update `ExternalAuth:GoogleClientId` in Admin appsettings

---

## 7. Database

```bash
# Run migrations against production database
dotnet ef database update \
  --project Alakai.FestivalManager.Infrastructure \
  --startup-project Alakai.FestivalManager.Api \
  --connection "Server=PROD-SERVER;..."
```

---

## 8. File storage

The `wwwroot/uploads/` folder in the Api stores email images uploaded via the Admin. In production:
- Ensure the Api process has write access to this folder
- Consider using Azure Blob Storage or S3 for production (requires implementing `IFileStorageService`)
- Update `FileStorage:PublicBaseUrl` to point to your production API domain

---

## 9. Security checklist

- [ ] `Jwt:SecretKey` is unique per deployment, ≥ 32 characters, never committed to git
- [ ] `Email:Password` not in committed appsettings
- [ ] `Redsys:SecretKey` not in committed appsettings
- [ ] `ConnectionStrings:DefaultConnection` not in committed appsettings
- [ ] CORS `WithOrigins` contains only the production Admin domain
- [ ] Swagger UI disabled in production (already gated by `IsDevelopment()`)
- [ ] HTTPS enforced on both Api and Admin
- [ ] SQL Server user has minimum required permissions (no sa)
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/production-checklist.md"

# ── docs/modules.md ───────────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\modules.md", @'
# Festival Modules

Modules are enabled per-festival using the `FestivalModule` flags enum. Each festival can have any combination enabled.

```csharp
[Flags]
public enum FestivalModule
{
    None         = 0,
    Competitions = 1,
    Accommodation = 2,
    Transport    = 4,
    Meals        = 8
}
```

Modules are stored on the `Festival` entity as an integer bitmask. The Admin UI and the User Panel check `HasModule(FestivalModule.X)` to show/hide sections.

---

## Core (always enabled)

### Registrations
The central module. Every festival has registrations.
- Public registration form at `/register/{slug}`
- Pass types with optional levels
- Dance roles (Leader / Follower / Individual)
- Partner linking
- Payment plans: FullOnline, SplitFiftyFifty, DeferredTenDays
- Redsys payment integration
- Discount codes with threshold or immediate activation

### Email Templates
Admin-configurable email templates with `{{variable}}` placeholders.
- Templates: RegistrationCreated, PaymentConfirmed, PaymentFailed, RegistrationCancelled, WaitingPartner, PartnerConfirmed, AccommodationConfirmed, AccommodationCancelled, AccommodationNewResponsible, BusConfirmed, BusCancelled, MenuConfirmed, MenuCancelled, PasswordReset
- Per-edition templates (falls back to global if no edition-specific one exists)
- WYSIWYG editor in Admin

### Invoices
PDF invoice generation using QuestPDF.
- Per-registration invoices
- Configurable invoice templates and issuer settings
- Auto-generated sequential invoice numbers

---

## Optional Modules

### Competitions (`FestivalModule.Competitions = 1`)
Competition management with capacity control.
- Multiple competitions per edition
- Formats: Individual / Partnered / Team
- Capacity per role (Leader / Follower / Individual)
- Threshold-based capacity with levels
- Status: WaitingPartner → Confirmed

### Accommodation (`FestivalModule.Accommodation = 2`)
Accommodation booking tied to registrations.
- Buildings with zones and individual accommodation units
- Reservation with responsible + occupants
- Occupant registration linking
- When the responsible registration is deleted, responsibility transfers automatically to another occupant
- Emails sent to all occupants on confirmation/cancellation

### Transport (`FestivalModule.Transport = 4`)
Bus reservation management.
- Outbound + return bus lines
- Per-bus capacity with pass type restrictions
- Email confirmation with departure details

### Meals (`FestivalModule.Meals = 8`)
Meal preference collection.
- Menu type selection
- Dietary requirements (celiac, gluten intolerant, allergies)
- Per-edition menu options

---

## User Panel

The User Panel (`/user-panel/`) is a self-service area for registered participants:
- Dashboard with registration and payment status
- Payment gateway (Redsys)
- Module sections gated by festival modules: Accommodation, Buses, Meals, Competitions

Access is JWT-authenticated. Users are created automatically when a registration is submitted.
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/modules.md"

# ── docs/testing.md ───────────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\testing.md", @'
# Testing

## Stack

| Package | Version | Purpose |
|---|---|---|
| xUnit | 2.9.3 | Test runner |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 8.3.1 | Readable assertions |
| Bogus | 35.6.3 | Test data generation |

## Running tests

```bash
# All tests
dotnet test Alakai.FestivalManager.Tests

# With coverage
dotnet test Alakai.FestivalManager.Tests --collect:"XPlat Code Coverage"

# Specific test class
dotnet test --filter "FullyQualifiedName~PaymentServiceTests"
```

## Test structure

```
Tests/
  Unit/
    Application/
      Common/
        Builders.cs                    ← shared test data builders (Bogus + reflection for protected Id)
      Features/
        Auth/
          AuthServiceLoginTests.cs     ← login, lockout, failed attempts
          JwtServiceTests.cs           ← token generation, claims, refresh tokens
        CompetitionEntries/
          CreateCompetitionEntryHandlerTests.cs
        DiscountCodes/
          DiscountCalculationServiceTests.cs
        Emails/
          EmailTemplateRendererServiceTests.cs
        Payments/
          PaymentServiceTests.cs       ← split payment flow, auth code storage
        Registrations/
          CreateRegistrationHandlerTests.cs
          DeleteRegistrationHandlerTests.cs
          UpdateRegistrationHandlerTests.cs
      Validators/
        CreateRegistrationCommandValidatorTests.cs
        CreateDiscountCodeCommandValidatorTests.cs
        CreateLevelValidatorTests.cs
        CreateEditionValidatorTests.cs
        CreateCompetitionCommandValidatorTests.cs
        LoginCommandValidatorTests.cs
  Integration/
    Api/
      Controllers/
        README.md                      ← integration tests not yet implemented
```

## Coverage areas

| Area | Tests | Key scenarios |
|---|---|---|
| `EmailTemplateRendererService` | 11 | Substitution, null values, HTML tag stripping in placeholders, whitespace |
| `DiscountCalculationService` | 10 | Percentage/fixed discounts, expired/inactive codes, threshold activation |
| `PaymentService` | 13 | CreateSession, split payment guard, auth codes, declined payments |
| `DeleteRegistrationHandler` | 10 | Cascade cleanup, accommodation responsibility transfer, partner nulling |
| `AuthService` (login) | 8 | User not found, inactive, locked, lockout expiry, failed attempt counter, email normalisation |
| `JwtService` | 9 | Token generation, claims, refresh token uniqueness, invalid token handling |
| `CreateRegistrationHandler` | 9 | Not found, duplicate email, level validation, payment due dates, user creation |
| `UpdateRegistrationHandler` | 6 | Not found, Paid→Confirmed status, duplicate email on change |
| `CreateCompetitionEntryHandler` | 9 | Not found, duplicate entry, capacity full, unlimited capacity, partner/no-partner status |
| Validators | 48 | Required fields, length limits, email format, date ordering, enum values |

## Builders

`Builders.cs` provides factory methods for domain entities. Because `BaseEntity.Id` has a `protected set`, the builders use reflection to assign a specific Id when needed:

```csharp
Registration reg = Builders.BuildRegistration(
    id: Guid.NewGuid(),
    paymentStatus: PaymentStatus.PartiallyPaid,
    paymentPlan: PaymentPlan.SplitFiftyFifty,
    finalPrice: 450m,
    amountPaid: 225m);
```

## What is not tested

- **Controllers** — thin delegation only, no business logic
- **Simple CRUD handlers** (GetAll, GetById, Delete without logic) — no branching
- **Services that delegate entirely** to repositories (FestivalService, EditionService, etc.)
- **Integration tests** — require a test database; planned for future implementation
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/testing.md"

# ── docs/api.md ──────────────────────────────────────────────────────────────
[System.IO.File]::WriteAllText("docs\api.md", @'
# API Reference

The API is a standard ASP.NET Core Web API with JWT Bearer authentication.

## Base URL

- Development: `https://localhost:7157`
- Production: configured per deployment

## Swagger

Available at `/swagger` in development only.

## Authentication

All endpoints except public ones require a JWT Bearer token:

```
Authorization: Bearer <token>
```

Tokens are obtained via `POST /api/auth/login`.

### Public endpoints (no auth required)
- `GET /api/public-festivals` — list active festivals for public registration form
- `GET /api/public-registrations/{slug}` — festival info for registration form
- `POST /api/public-registrations` — submit a registration
- `POST /api/auth/login` — obtain JWT token
- `POST /api/auth/refresh-token` — refresh access token
- `POST /api/auth/forgot-password` — request password reset
- `POST /api/auth/reset-password` — submit new password
- `POST /api/payments/redsys/return` — Redsys payment return URL
- `POST /api/payments/redsys/notification` — Redsys server-to-server notification

## Response format

All responses follow `ApiResponse<T>`:

```json
{
  "success": true,
  "data": { ... },
  "errors": [],
  "message": "..."
}
```

Errors return RFC 7807 Problem Details via `GlobalExceptionMiddleware`:

| Exception | HTTP Status |
|---|---|
| `NotFoundException` | 404 |
| `BusinessRuleException` | 422 |
| `ValidationException` | 400 |
| Unhandled | 500 |

## Main endpoint groups

| Prefix | Description |
|---|---|
| `/api/festivals` | Festival CRUD |
| `/api/editions` | Edition CRUD |
| `/api/pass-types` | Pass type CRUD |
| `/api/levels` | Level CRUD |
| `/api/registrations` | Registration CRUD + status management |
| `/api/payments` | Redsys payment session creation and callbacks |
| `/api/competitions` | Competition CRUD |
| `/api/competition-entries` | Competition entry management |
| `/api/accommodations` | Accommodation unit management |
| `/api/accommodation-buildings` | Building + zone management |
| `/api/accommodation-reservations` | Reservation CRUD |
| `/api/buses` | Bus line management |
| `/api/bus-reservations` | Bus booking |
| `/api/meal-preferences` | Meal preference management |
| `/api/email-templates` | Email template CRUD |
| `/api/emails` | Manual email sending |
| `/api/email-logs` | Sent email history |
| `/api/invoices` | Invoice generation and retrieval |
| `/api/discount-codes` | Discount code management |
| `/api/reports` | Excel report exports |
| `/api/dashboard` | KPI stats and revenue data |
| `/api/analytics` | Google Analytics 4 data |
| `/api/users` | User management |
| `/api/auth` | Authentication |
| `/api/user-panel` | User Panel specific endpoints |
| `/api/uploads` | Image upload for email templates |
'@, [System.Text.Encoding]::UTF8)
Write-Host "OK: docs/api.md"

Write-Host ""
Write-Host "Documentation created:"
Write-Host "  README.md"
Write-Host "  docs/architecture.md"
Write-Host "  docs/configuration.md"
Write-Host "  docs/production-checklist.md"
Write-Host "  docs/modules.md"
Write-Host "  docs/testing.md"
Write-Host "  docs/api.md"