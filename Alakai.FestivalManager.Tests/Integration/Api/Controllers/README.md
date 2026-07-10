# Integration Tests

Integration tests use `WebApplicationFactory<Program>` to spin up the real API in-memory.
They require a test database (InMemory or real SQL Server via testcontainers).

## Setup required before writing integration tests:

1. The `Api` project's `Program.cs` must expose the class as `public` or use `[assembly: InternalsVisibleTo]`.
2. Add an `appsettings.Testing.json` to the Api project with InMemory DB connection string.
3. Create a `WebApplicationFactory` fixture.

## Planned tests:
- `RegistrationsControllerTests` — GET/POST/DELETE with auth
- `PaymentsControllerTests` — CreateSession, ProcessReturn
- `PublicRegistrationsControllerTests` — /register/{slug} availability, registration flow
