# Reservation System

> 🇵🇱 [Polish version](README.pl.md)

![CI](../../actions/workflows/ci.yml/badge.svg)

Small desk reservation system. Focus: **data integrity under concurrency**.

Stack: .NET 10, ASP.NET Core (Blazor Server + Minimal API), EF Core, SQL Server. Architecture: Clean Architecture (`Domain` / `Application` / `Infrastructure` / `Web`).

## Running it

**Prerequisites:** .NET 10 SDK, SQL Server on `localhost` (or change the connection string in `src/ReservationSystem.Web/appsettings.Development.json`), `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`).

```bash
# 1. Database + stored procedure (two EF migrations)
dotnet ef database update \
  --project src/ReservationSystem.Infrastructure \
  --startup-project src/ReservationSystem.Web

# 2. Run the app
dotnet run --project src/ReservationSystem.Web
```

- UI:         <http://localhost:5000>
- Swagger:    <http://localhost:5000/swagger>
- Race demo:  <http://localhost:5000/race>

**Tests** (use a separate `ReservationSystem_Test` database, reset before each test class):

```bash
dotnet test
```

## Concurrency safety

The overlap check and the insert happen atomically inside the stored procedure [`usp_CreateReservation`](src/ReservationSystem.Infrastructure/Persistence/Scripts/usp_CreateReservation.sql), where the check query against `Reservations` uses the `WITH (UPDLOCK, HOLDLOCK)` hints inside a single transaction. `UPDLOCK` prevents two concurrent transactions from both reading "no conflict" for the same desk, while `HOLDLOCK` acquires key-range locks that block any insert falling into the checked time range until the current transaction commits — together they guarantee the "check → insert" sequence is indivisible, so only one of the racing requests can complete it successfully.

Interval logic: `[StartAt, EndAt)` with the overlap condition `Start1 < End2 AND End1 > Start2` — a reservation ending at 11:00 does not conflict with one starting at 11:00.

## What else is in the repo

- **Users + login-as** (cookie signed by `IDataProtector`, `UserId` never in the request body → no IDOR); ownership rule on cancel (DELETE someone else's reservation → `403`).
- **Race demo** at `/race` — slider for 2–200 concurrent "attackers", colour-coded result table (one winner in green, the rest as conflicts in yellow).
- **Retry** on transient SQL errors (deadlock / lock timeout) — up to 3 attempts.
- **i18n**: EN (default) + PL via `IStringLocalizer<SharedResources>` and `.resx`, language switcher in the header (cookie-based).
- **Typed repository errors** (sealed hierarchy `RepositoryError`) mapped in the service to `Result<T>` → endpoint returns 400/404/409/403.

## API

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/api/desks` / `/api/users` / `/api/reservations?deskId={id}` | Reads |
| `POST` | `/api/reservations` | Create — **201** / **400** / **404** / **409** |
| `DELETE` | `/api/reservations/{id}` | Cancel — **204** / **403** (not owner) / **404** |
| `POST` | `/api/session` (JSON `{userId}`) | Login-as (sets cookie) |
| `POST` | `/api/demo/race` | Trigger the race demo from the API |
