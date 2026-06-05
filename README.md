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

2. Set the JWT signing key. `appsettings.json` ships with an empty `Jwt:SecretKey` so production startups must inject one. For local dev:

```bash
cd src/PollaMundialista.Api
dotnet user-secrets set "Jwt:SecretKey" "<paste a 32+ character random string>"
```

Or use an environment variable:

```bash
export Jwt__SecretKey="<paste a 32+ character random string>"
```

`appsettings.Development.json` already includes a dev secret so you can skip this step when running with `ASPNETCORE_ENVIRONMENT=Development`.

3. Restore and run:

```bash
dotnet restore
dotnet run --project src/PollaMundialista.Api
```

EF Core migrations are applied automatically on startup. The seeder only runs when `Seeding:Enabled` is `true` (default in `appsettings.Development.json`, off in `appsettings.json`).

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

All auth endpoints are rate-limited to **10 requests / minute / IP** (fixed-window).

| Method | Path        | Auth   | Description                                          |
|--------|-------------|--------|------------------------------------------------------|
| POST   | `/register` | Public | Register, returns access + refresh tokens            |
| POST   | `/login`    | Public | Login, returns access + refresh tokens               |
| POST   | `/refresh`  | Public | Rotate refresh token, returns new access + refresh   |
| POST   | `/logout`   | Public | Revoke a refresh token (idempotent, returns 204)     |

Access tokens have a short lifetime (default 15 min). Clients must call `/refresh` with the opaque refresh token to obtain a new pair. Each refresh **rotates** the token — replaying a used refresh token returns `401`.

### Predictions — `/api/predictions`

| Method | Path               | Auth     | Description                        |
|--------|--------------------|----------|------------------------------------|
| GET    | `/upcoming`        | User     | List upcoming matches with your prediction (if any) |
| GET    | `/mine`            | User     | All your predictions with points   |
| POST   | `/`                | User     | Submit or update a prediction      |

### Admin — `/api/admin`

| Method | Path                          | Auth     | Description                          |
|--------|-------------------------------|----------|--------------------------------------|
| GET    | `/matches`                    | Admin    | All matches with current results     |
| PUT    | `/matches/{matchId}/result`   | Admin    | Set match result, recalculates scores |

### Leaderboard — `/api/leaderboard`

| Method | Path                          | Auth     | Description                          |
|--------|-------------------------------|----------|--------------------------------------|
| GET    | `/`                           | User     | Ranked leaderboard (total points)    |
| GET    | `/users/{userId}/history`     | User     | Match-by-match history (self only — returns `403` for other users) |

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
| `Jwt:SecretKey`                    | HS256 signing key (min 32 bytes). **Must be set via user-secrets or env var in non-Dev.** |
| `Jwt:Issuer`                       | JWT issuer claim                         |
| `Jwt:Audience`                     | JWT audience claim                       |
| `Jwt:ExpirationMinutes`            | Access-token lifetime (default: 15)      |
| `Jwt:RefreshTokenDays`             | Refresh-token lifetime (default: 14)     |
| `Cors:AllowedOrigins`              | Array of allowed frontend origins        |
| `Seeding:Enabled`                  | Run the demo-data seeder on startup (default: `false`) |
| `Seeding:AdminPassword`            | Required when `Seeding:Enabled=true`     |
| `Seeding:UserPassword`             | Required when `Seeding:Enabled=true`     |

### Environment-variable overrides

Production deployments should provide secrets via environment variables rather than `appsettings.json`. The standard .NET binding maps `:` → `__`:

```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;"
export Jwt__SecretKey="<32+ char value>"
export Seeding__AdminPassword="<strong password>"
export Seeding__UserPassword="<strong password>"
```

The app **fails fast** at startup if `Jwt:SecretKey` is missing or shorter than 32 bytes.

### Security hardening summary

- **OWASP A02/A05** — Secrets removed from `appsettings.json`; HSTS in non-Dev; security headers middleware (`nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy`, `Cache-Control: no-store` on `/api/auth`); CORS narrowed to known methods + headers.
- **OWASP A07** — Short-lived access JWTs (15 min) + rotating refresh tokens stored as SHA-256 hashes (plaintext is never persisted); replay rejected by `RevokedAt` chain. Fixed-window rate limit on `/api/auth/*`.
- **OWASP A01** — `GET /api/leaderboard/users/{userId}/history` is now self-only.
- **OWASP A09** — Login success/failure logged with email + remote IP via `ICurrentUser`.

### Follow-ups (out of scope for this pass)

- Move refresh-token storage to an HttpOnly cookie (requires Angular interceptor changes).
- Centralize secrets in Azure Key Vault / AWS Secrets Manager.
- 2FA / MFA.

---

## Database Schema

See [`docs/schema.md`](docs/schema.md) for the full ER diagram and table definitions.

---

## Running Tests

```bash
dotnet test
```

Unit tests cover all Application handlers and `ScoringService` edge cases (exact score, correct outcome, miss, draw, result recalculation).
