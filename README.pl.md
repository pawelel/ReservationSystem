# Reservation System

> 🇬🇧 [English version](README.md)

![CI](../../actions/workflows/ci.yml/badge.svg)

Mini system rezerwacji biurek. Focus: **integralność danych pod współbieżnością**.

Stack: .NET 10, ASP.NET Core (Blazor Server + Minimal API), EF Core, SQL Server. Architektura: Clean Architecture (`Domain` / `Application` / `Infrastructure` / `Web`).

## Uruchomienie

**Wymagania:** .NET 10 SDK, SQL Server pod `localhost` (albo zmień connection string w `src/ReservationSystem.Web/appsettings.Development.json`), narzędzie `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

```bash
# 1. Baza + procedura składowana (dwie migracje EF)
dotnet ef database update \
  --project src/ReservationSystem.Infrastructure \
  --startup-project src/ReservationSystem.Web

# 2. Uruchomienie aplikacji
dotnet run --project src/ReservationSystem.Web
```

- UI:          <http://localhost:5000>
- Swagger:     <http://localhost:5000/swagger>
- Demo wyścigu: <http://localhost:5000/race>

**Testy** (osobna baza `ReservationSystem_Test`, resetowana przed każdą klasą):

```bash
dotnet test
```

## Zabezpieczenie przed współbieżnością

Sprawdzenie kolizji i wstawienie rekordu odbywa się atomowo w procedurze składowanej [`usp_CreateReservation`](src/ReservationSystem.Infrastructure/Persistence/Scripts/usp_CreateReservation.sql), gdzie zapytanie kontrolne na tabeli `Reservations` używa hintów `WITH (UPDLOCK, HOLDLOCK)` w ramach jednej transakcji. `UPDLOCK` uniemożliwia dwóm transakcjom jednoczesne odczytanie braku kolizji dla tego samego biurka, a `HOLDLOCK` zakłada blokady zakresowe (key-range) blokujące insert wpadający w badany przedział aż do commita — razem gwarantują, że sekwencja „sprawdź → wstaw" jest niepodzielna i tylko jedno z konkurujących żądań może ją zakończyć sukcesem.

Logika kolizji przedziałów: `[StartAt, EndAt)` z warunkiem `Start1 < End2 AND End1 > Start2` — rezerwacja kończąca się o 11:00 nie koliduje z rozpoczynającą o 11:00.

## Co jeszcze w repo

- **Użytkownicy + login-as** (cookie podpisywany przez `IDataProtector`, `UserId` nigdy nie leci w body requestu → brak IDOR); reguła własności przy anulowaniu (DELETE cudzej rezerwacji → `403`).
- **Demo wyścigu** pod `/race` — slider 2–200 równoległych „atakujących", kolorowa tabela wyników (1 zwycięzca na zielono, reszta konfliktów na żółto).
- **Retry** na transient SQL errors (deadlock / lock timeout) — do 3 prób.
- **i18n**: EN (domyślne) + PL przez `IStringLocalizer<SharedResources>` i `.resx`, przełącznik w nagłówku (cookie).
- **Typed repository errors** (sealed hierarchy `RepositoryError`) mapowane w serwisie na `Result<T>` → endpoint zwraca 400/404/409/403.

## API

| Metoda | Ścieżka | Opis |
|---|---|---|
| `GET` | `/api/desks` / `/api/users` / `/api/reservations?deskId={id}` | Odczyty |
| `POST` | `/api/reservations` | Tworzenie — **201** / **400** / **404** / **409** |
| `DELETE` | `/api/reservations/{id}` | Anulowanie — **204** / **403** (nie właściciel) / **404** |
| `POST` | `/api/session` (JSON `{userId}`) | Login-as (ustawia cookie) |
| `POST` | `/api/demo/race` | Uruchomienie demo wyścigu z poziomu API |
