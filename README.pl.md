# Reservation System

> 🇬🇧 [English version](README.md)

![CI](../../actions/workflows/ci.yml/badge.svg)

Mini system rezerwacji biurek. Focus: **integralność danych pod współbieżnością**.

Stack: .NET 10, ASP.NET Core (Blazor Server + Minimal API), EF Core, SQL Server.

## Uruchomienie

**Wymagania:**
- .NET 10 SDK
- SQL Server 2019+ pod `localhost:1433` z Windows Authentication (LocalDB też działa). Na Linuksie / bez Windows Auth uruchom dev-image:
  ```bash
  docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
  i ustaw `Database:ConnectionString` w `src/ReservationSystem.Web/appsettings.Development.json` na `Server=localhost;Database=ReservationSystem;User Id=sa;Password=Your_password123;TrustServerCertificate=True;`.
- Narzędzie `dotnet-ef`: `dotnet tool install --global dotnet-ef`

```bash
dotnet ef database update --project src/ReservationSystem.Infrastructure --startup-project src/ReservationSystem.Web
dotnet run --project src/ReservationSystem.Web
```

Aplikacja startuje pod <http://localhost:5000>. Testy: `dotnet test`.

## Zabezpieczenie przed współbieżnością

Sprawdzenie kolizji i wstawienie rekordu odbywa się atomowo w procedurze składowanej [`usp_CreateReservation`](src/ReservationSystem.Infrastructure/Persistence/Scripts/usp_CreateReservation.sql), gdzie zapytanie kontrolne na tabeli `Reservations` używa hintów `WITH (UPDLOCK, HOLDLOCK)` w ramach jednej transakcji. `UPDLOCK` uniemożliwia dwóm transakcjom jednoczesne odczytanie braku kolizji dla tego samego biurka, a `HOLDLOCK` zakłada blokady zakresowe (key-range) blokujące insert wpadający w badany przedział aż do commita — razem gwarantują, że sekwencja „sprawdź → wstaw" jest niepodzielna i tylko jedno z konkurujących żądań może ją zakończyć sukcesem.

Logika kolizji przedziałów: `[StartAt, EndAt)` z warunkiem `Start1 < End2 AND End1 > Start2` — rezerwacja kończąca się o 11:00 nie koliduje z rozpoczynającą o 11:00.
