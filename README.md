# Polla Mundialista — API

Backend for a private World Cup prediction pool. Built as a technical assessment using .NET 8 Clean Architecture + CQRS.

Companion frontend: **polla-mundialista-web** (Angular).

---

## Architecture

The solution follows Clean Architecture with strict inward-only dependency rules:

```
Domain  ←  Application  ←  Infrastructure  ←  Api
```

- **Domain** — entities, enums, value objects, domain exceptions. Zero external dependencies.
- **Application** — MediatR commands/queries, handlers, validators (FluentValidation), DTOs (records), repository interfaces, `Result<T>` pattern.
- **Infrastructure** — EF Core + SQL Server, JWT, BCrypt, repository implementations, DB seeder.
- **Api** — thin controllers (only `mediator.Send`), Swagger, middleware, DI composition root.

All business errors use `Result<T>` — no exceptions for expected failures. Validation and logging run as MediatR pipeline behaviors.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+ (local) **or** Docker Desktop

---

## Running Locally (without Docker)

1. Start a SQL Server instance on `localhost,1433` with SA password `YourStrong@Passw0rd`, or update `appsettings.Development.json` with your own connection string.

2. Restore and run:

```bash
dotnet restore
dotnet run --project src/PollaMundialista.Api
```

EF Core migrations are applied automatically on startup. The database is also seeded on first run.

The API is available at `https://localhost:5001` (or `http://localhost:5000`).  
Swagger UI: `https://localhost:5001/swagger`

---

## Running with Docker

```bash
docker compose up --build
```

This starts:
- **SQL Server 2022** on port `1433`
- **API** on port `8080` — waits for SQL Server to be healthy, then applies migrations and seeds the DB.

API: `http://localhost:8080`  
Swagger UI: `http://localhost:8080/swagger`

---

## Seeded Credentials

| Role  | Email              | Password   |
|-------|--------------------|------------|
| Admin | admin@polla.com    | Admin123!  |
| User  | user@polla.com     | User123!   |

The seeder is idempotent — runs only when the Users table is empty.

### Seeded Matches (12 total)

**Group A** — Argentina, Brazil, France, Germany (full round-robin)

| Match                  | Date (UTC)          |
|------------------------|---------------------|
| Argentina vs Brazil    | 2026-06-15 18:00    |
| France vs Germany      | 2026-06-15 21:00    |
| Argentina vs France    | 2026-06-19 15:00    |
| Germany vs Brazil      | 2026-06-19 18:00    |
| Argentina vs Germany   | 2026-06-23 21:00    |
| Brazil vs France       | 2026-06-23 21:00    |

**Group B** — Spain, Portugal, England, Netherlands (full round-robin)

| Match                     | Date (UTC)          |
|---------------------------|---------------------|
| Spain vs Portugal         | 2026-06-16 18:00    |
| England vs Netherlands    | 2026-06-16 21:00    |
| Spain vs England          | 2026-06-20 15:00    |
| Portugal vs Netherlands   | 2026-06-20 18:00    |
| Spain vs Netherlands      | 2026-06-24 21:00    |
| Portugal vs England       | 2026-06-24 21:00    |

---

## API Endpoints

### Auth — `/api/auth`

| Method | Path               | Auth     | Description                        |
|--------|--------------------|----------|------------------------------------|
| POST   | `/register`        | Public   | Register a new user                |
| POST   | `/login`           | Public   | Login, returns JWT                 |

### Predictions — `/api/predictions`

| Method | Path               | Auth     | Description                        |
|--------|--------------------|----------|------------------------------------|
| GET    | `/upcoming`        | User     | List upcoming matches with your prediction (if any) |
| GET    | `/mine`            | User     | All your predictions with points   |
| POST   | `/`                | User     | Submit or update a prediction      |

### Admin — `/api/admin`

| Method | Path                          | Auth     | Description                          |
|--------|-------------------------------|----------|--------------------------------------|
| PUT    | `/matches/{matchId}/result`   | Admin    | Set match result, recalculates scores |

### Leaderboard — `/api/leaderboard`

| Method | Path                          | Auth     | Description                          |
|--------|-------------------------------|----------|--------------------------------------|
| GET    | `/`                           | User     | Ranked leaderboard (total points)    |
| GET    | `/users/{userId}/history`     | User     | Match-by-match history for a user    |

---

## Scoring Rules

| Outcome               | Points |
|-----------------------|--------|
| Exact score           | 3      |
| Correct winner / draw | 1      |
| Wrong outcome         | 0      |

Setting a match result recalculates points for **all** predictions on that match (idempotent).

---

## Configuration Reference

See `src/PollaMundialista.Api/appsettings.Example.json` for all required settings.

| Key                                | Description                              |
|------------------------------------|------------------------------------------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string          |
| `Jwt:SecretKey`                    | HS256 signing key (min 32 chars)         |
| `Jwt:Issuer`                       | JWT issuer claim                         |
| `Jwt:Audience`                     | JWT audience claim                       |
| `Jwt:ExpirationMinutes`            | Token lifetime (default: 480)            |
| `Cors:AllowedOrigins`              | Array of allowed frontend origins        |

---

## Database Schema

See [`docs/schema.md`](docs/schema.md) for the full ER diagram and table definitions.

---

## Running Tests

```bash
dotnet test
```

Unit tests cover all Application handlers and `ScoringService` edge cases (exact score, correct outcome, miss, draw, result recalculation).
