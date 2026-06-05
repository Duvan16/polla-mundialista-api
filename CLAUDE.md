# CLAUDE.md — Polla Mundialista API

## Project

Backend for "Polla Mundialista" — a private World Cup prediction pool. .NET 8 Web API.
technical assessment. Companion frontend repo: polla-mundialista-web (Angular).

## Stack

- .NET 8 / C# 12 (nullable enabled, implicit usings)
- Clean Architecture + CQRS via MediatR
- EF Core 8 + SQL Server
- FluentValidation, AutoMapper
- JWT (JwtBearer) auth, role-based
- xUnit + Moq + FluentAssertions

## Architecture — layer rules (enforce strictly)

- **Domain**: entities, enums, value objects, domain exceptions. ZERO external dependencies.
- **Application**: commands/queries, MediatR handlers, DTOs (records), validators, interfaces (IRepository, IUnitOfWork, ICurrentUser), mapping profiles. Depends ONLY on Domain.
- **Infrastructure**: EF Core, DbContext, repo/UoW impls, JWT service, hashing, migrations, seeder. Depends on Application + Domain.
- **Api**: thin controllers, DI composition root, middleware, Swagger. Depends on Application + Infrastructure.

Dependency direction points inward. Never reference outward (Domain must not know Application, etc.).

## Hard rules (do not violate)

- Controllers are THIN: only `mediator.Send(...)` and map Result → status code. No business logic, no EF Core.
- NO EF Core / DbContext usage inside Application handlers. Go through repository interfaces only.
- Business errors use the `Result<T>` pattern — do NOT throw exceptions for expected failures (e.g. duplicate email, predicting on a finished match). Throw only for truly exceptional cases.
- All DTOs are `record` types. Entities are not exposed directly over the API.
- Every command/query has a FluentValidation validator where input validation applies.
- Validation + logging run as MediatR pipeline behaviors, not inline in handlers.
- `ScoringService` is a PURE function with no I/O — fully unit tested across all branches.

## Domain rules (the business)

- Scoring: 3 pts = exact score; 1 pt = correct outcome (winner/draw) only; 0 = wrong outcome.
- One prediction per user per match (unique constraint UserId + MatchId).
- Predictions can be created/updated only while the match is NOT finished.
- Setting a match result (Admin only) recalculates PointsAwarded for ALL predictions on that match. Must be idempotent.
- Roles: User (predicts), Admin (loads real results).

## Conventions

- Folders by feature inside Application (e.g. Features/Predictions/Commands/SubmitPrediction/).
- One handler per file. Command + Handler + Validator co-located.
- Async all the way; suffix async methods with `Async`; pass CancellationToken through.
- Mapping via AutoMapper profiles, not manual in handlers (unless trivial).
- Conventional commits (feat:, fix:, test:, chore:, refactor:).

## Testing

- Unit-test all Application handlers and ScoringService.
- Cover edge cases: duplicate email, predicting finished match, exact vs outcome-only vs miss, draws, result recalculation across multiple users.
- Use FluentAssertions. Mock repositories with Moq. No real DB in unit tests.

## Commands

- Build: `dotnet build`
- Test: `dotnet test`
- Run: `dotnet run --project src/PollaMundialista.Api`
- Migrations: `dotnet ef migrations add <Name> -p src/PollaMundialista.Infrastructure -s src/PollaMundialista.Api`
- Docker: `docker compose up` (API + SQL Server, migrations applied on startup)

## Workflow expectations

- Show the plan / folder tree before generating large amounts of code.
- After implementing a module, run tests and report results.
- Don't generate controllers until the corresponding handlers exist.
- Keep seeded credentials documented in README (Admin + User).
  /
