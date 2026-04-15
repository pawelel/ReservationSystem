# Reservation System

> 🇵🇱 [Polish version](README.pl.md)

![CI](../../actions/workflows/ci.yml/badge.svg)

Small desk reservation system. Focus: **data integrity under concurrency**.

Stack: .NET 10, ASP.NET Core (Blazor Server + Minimal API), EF Core, SQL Server.

## Running it

**Prerequisites:**
- .NET 10 SDK
- SQL Server 2019+ on `localhost:1433` with Windows Authentication (LocalDB works too). For SQL auth or a remote server, edit `Database:ConnectionString` in `src/ReservationSystem.Web/appsettings.Development.json`.
- `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

```bash
dotnet ef database update --project src/ReservationSystem.Infrastructure --startup-project src/ReservationSystem.Web
dotnet run --project src/ReservationSystem.Web
```

App runs at <http://localhost:5000>. Tests: `dotnet test`.

## Concurrency safety

The overlap check and the insert happen atomically inside the stored procedure [`usp_CreateReservation`](src/ReservationSystem.Infrastructure/Persistence/Scripts/usp_CreateReservation.sql), where the check query against `Reservations` uses the `WITH (UPDLOCK, HOLDLOCK)` hints inside a single transaction. `UPDLOCK` prevents two concurrent transactions from both reading "no conflict" for the same desk, while `HOLDLOCK` acquires key-range locks that block any insert falling into the checked time range until the current transaction commits — together they guarantee the "check → insert" sequence is indivisible, so only one of the racing requests can complete it successfully.

Interval logic: `[StartAt, EndAt)` with the overlap condition `Start1 < End2 AND End1 > Start2` — a reservation ending at 11:00 does not conflict with one starting at 11:00.
