# BookingPlatform API

A local services booking platform backend built with ASP.NET Core 10 and PostgreSQL. Customers browse providers, book services into available time slots, and leave reviews. Providers manage their own catalog, pricing, and availability.

Built as a learning project focused on clean layered architecture, EF Core relationship modeling, and concurrency-safe booking logic — not a production-scale system.

---

## Tech Stack

- **ASP.NET Core 10** (Web API, minimal hosting model)
- **EF Core 10** with **Npgsql** (PostgreSQL provider)
- **PostgreSQL** (hosted on [Neon](https://neon.tech), free tier)
- **JWT Bearer Authentication** (hand-rolled, not ASP.NET Identity)
- **Serilog** (structured console logging)
- **Scalar** (interactive API documentation, replaces Swagger UI)

---

## Architecture

The solution is split into four projects enforcing a one-directional dependency chain — each layer can only reference the layers below it:

```
BookingPlatform.Api             → controllers, middleware, Program.cs
        ↓ depends on
BookingPlatform.Infrastructure  → EF Core, DbContext, service implementations
        ↓ depends on
BookingPlatform.Application     → interfaces, DTOs, custom exceptions
        ↓ depends on
BookingPlatform.Domain          → entities, enums (zero dependencies)
```

- **Domain** has no dependencies on anything — pure data shape.
- **Application** defines *what* the system does (interfaces, DTOs) without knowing *how*.
- **Infrastructure** implements the *how* — EF Core, PostgreSQL, JWT generation.
- **Api** wires everything together and exposes it over HTTP.

The repository pattern was deliberately skipped — `AppDbContext` is injected directly into Application-layer services, since EF Core's `DbContext` already functions as a unit-of-work/repository.

---

## Database Schema

8 tables, PostgreSQL, all primary keys are `Guid` (chosen over `int` to avoid predictable/enumerable IDs on public endpoints):

| Table | Purpose |
|---|---|
| `Users` | Base identity — email, password hash, role (Customer/Provider) |
| `Providers` | Business profile, one-to-one with a User |
| `Services` | Shared catalog of service types (e.g. "Haircut") |
| `ProviderServices` | Join table — links a Provider to a Service with their own price/duration |
| `AvailabilityTemplates` | Recurring weekly availability patterns (e.g. "Mondays 9–5") |
| `Slots` | Actual bookable time instances, generated from templates or added manually |
| `Bookings` | A customer's claim on a slot for a specific provider service |
| `Reviews` | One review per completed booking |

### Key design decisions

- **`ProviderServices` carries its own `Price` and `DurationMinutes`** rather than living on `Services`, since the same catalog service can be priced differently per provider.
- **Slots are discrete rows, not time ranges** — this naturally supports irregular daily availability (e.g. a lunch-break gap) without special-case logic; a gap is simply the absence of slot rows.
- **`AvailabilityTemplates` are patterns, not bookable entities** — the UI only ever shows and books against `Slots`. A provider can have multiple templates for the same weekday (e.g. a morning block and a separate afternoon block).
- **`PriceAtBooking` is snapshotted onto `Booking`** at creation time, so a later price change on `ProviderServices` never retroactively alters historical booking records.
- **No hard deletes on `Slots` or `Bookings` once tied to real history** — cancelled bookings are kept, not removed, and slots can only be deleted while still `Open`.

---

## Concurrency Handling

Two different concurrency problems are solved with two different mechanisms:

**1. Double-booking a slot (insert race)**
Solved with a **unique database constraint** on `Bookings.SlotId`. Two simultaneous booking attempts both pass the application-level status check, but the database itself rejects the second `INSERT`. The resulting `DbUpdateException` is caught and converted into a `409 Conflict`.

**2. Editing a listing while someone else edits it (update race)**
Solved with **EF Core optimistic concurrency** via a `RowVersion` column on `ProviderServices`. The client must send back the `RowVersion` it originally fetched; if the row changed in between, `SaveChangesAsync` throws `DbUpdateConcurrencyException`, converted into a `409 Conflict`.

---

## Authentication

Plain JWT, built manually rather than using ASP.NET Core Identity — deliberately, to understand every piece rather than let a framework hide it.

- Passwords hashed with `Microsoft.AspNetCore.Identity.PasswordHasher<T>` (the hasher only, not the full Identity framework).
- On login, a JWT is issued containing `NameIdentifier` (user ID), `Email`, and `Role` claims.
- `[Authorize]` protects any endpoint requiring login; `[Authorize(Roles = "Provider")]` restricts provider-only actions.
- Tokens are validated against `Issuer`, `Audience`, expiry, and signing key on every request — no per-request database lookup needed.

No refresh tokens — access tokens are long-lived enough for this project's scope, and refresh rotation was deliberately left out to avoid unnecessary complexity.

---

## Provider Deactivation (Soft Delete)

Deactivating a provider (`PATCH /api/providers/deactivate`) does **not** delete their account. Instead:

- `Provider.IsActive` is set to `false`.
- All `AvailabilityTemplates` are purged.
- All currently `Open` slots are purged (booked/completed slots are untouched — they're real history).
- The provider's profile and past `ProviderServices` pricing remain intact.
- Deactivated providers are filtered out of all public browsing endpoints (catalog, slots, provider lookup) but their **reviews remain visible**, since reviews represent past experience, not current availability.

Reactivation simply flips `IsActive` back to `true`; nothing is regenerated automatically.

---

## Project Structure

```
BookingPlatform.Api/
├── Controllers/
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs   ← global exception → HTTP status mapping
├── Extensions/
└── Program.cs

BookingPlatform.Application/
├── Interfaces/
├── DTOs/
└── Exceptions/                           ← ValidationException, NotFoundException,
                                             ConflictException, UnauthorizedException

BookingPlatform.Domain/
├── Entities/
└── Enums/

BookingPlatform.Infrastructure/
├── Data/
│   ├── AppDbContext.cs
│   └── Configurations/                   ← one IEntityTypeConfiguration<T> per entity
├── Auth/
├── Providers/
├── Catalog/
├── Scheduling/
├── Bookings/
├── Reviews/
└── Migrations/
```

Each feature area (Auth, Providers, Catalog, Scheduling, Bookings, Reviews) follows the same pattern: an interface in `Application`, an implementation in `Infrastructure`, DTOs describing the request/response shape, and domain exceptions instead of scattered try/catch blocks in controllers.

---

## Error Handling

All business-rule failures are thrown as one of four custom exceptions and caught centrally by `ExceptionHandlingMiddleware`, which maps them to the correct HTTP status and a clean JSON body — no raw stack traces are ever exposed to the client:

| Exception | Status |
|---|---|
| `ValidationException` | 400 |
| `UnauthorizedException` | 401 |
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| *(anything unhandled)* | 500, generic message, real error logged internally |

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- A PostgreSQL database (this project was built against a free [Neon](https://neon.tech) instance)

### Setup

```bash
git clone <repo-url>
cd BookingPlatform/BookingPlatform.Api

dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
dotnet user-secrets set "Jwt:Key" "<a long random secret, 32+ characters>"
```

Add to `appsettings.json`:
```json
"Jwt": {
  "Issuer": "BookingPlatform",
  "Audience": "BookingPlatformUsers",
  "ExpiryMinutes": 120
}
```

### Run migrations

```bash
dotnet ef database update --project ../BookingPlatform.Infrastructure --startup-project .
```

### Run the API

```bash
dotnet run
```

Scalar API docs will be available at `/scalar/v1` in development.

---

## API Overview

| Area | Endpoints |
|---|---|
| Auth | `POST /api/auth/register`, `POST /api/auth/login` |
| Providers | `POST /api/providers/profile`, `PATCH /api/providers/deactivate`, `PATCH /api/providers/reactivate` |
| Services | `GET /api/services`, `POST /api/services` |
| Provider Services | `GET /api/providerservices`, `GET /api/providerservices/{providerId}`, `POST /api/providerservices`, `PATCH /api/providerservices/{id}`, `DELETE /api/providerservices/{id}` |
| Slots | `POST /api/slots/templates`, `POST /api/slots/generate`, `POST /api/slots`, `GET /api/slots/provider/{providerId}`, `PATCH /api/slots/{id}/lock`, `PATCH /api/slots/{id}/unlock`, `DELETE /api/slots/{id}` |
| Bookings | `POST /api/bookings`, `GET /api/bookings/my`, `GET /api/bookings/provider/{providerId}`, `PATCH /api/bookings/{id}/cancel`, `PATCH /api/bookings/{id}/complete` |
| Reviews | `POST /api/reviews`, `GET /api/reviews/provider/{providerId}` |

---

## Known Limitations

This was scoped deliberately to stay learnable end-to-end, not to be production-ready. Notable omissions:

- **No payment integration** — booking and payment are treated as separate concerns; payment is assumed to happen in person.
- **No refresh tokens** — access tokens are long-lived instead.
- **Booking completion is manual**, not time-based — a provider explicitly marks a booking `Completed`, since only they know whether the appointment actually happened.
- **Timezone handling is simplified** — slot times are stored and compared as if UTC, without per-provider timezone conversion.
- **No rate limiting, caching, or horizontal scaling considerations** — deliberately out of scope for a solo learning project.
