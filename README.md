# Beauty Salon

A .NET MAUI app for managing a single-owner beauty salon: weekly agenda, clients, service catalog, and payments. Built with Clean Architecture, EF Core/SQLite, and CommunityToolkit.Mvvm, targeting Android (iOS planned for a future release).

The weekly agenda is the app's home screen — not a dashboard. Creating, confirming, starting, finishing, cancelling, and rescheduling an appointment are all one tap away from the agenda itself.

## Features

- **Weekly agenda** — navigate weeks, jump to today, and drive the full appointment lifecycle (booked → confirmed → in progress → completed/cancelled/no-show/rescheduled) from one screen.
- **Clients** — CRUD, real-time search (name/phone/RUT), favorites, and a client detail view with visit history, total spent, and quick call/WhatsApp/email actions.
- **Service catalog** — categories and services, each with price, duration, and color.
- **Payments** — configurable payment methods; charged price, discount, and tip are recorded when an appointment is finished.
- **Smart scheduling** — prevents double-booking and booking outside configured working hours.

## Tech stack

- .NET 10, .NET MAUI, CommunityToolkit.Maui, CommunityToolkit.Mvvm
- EF Core + SQLite
- FluentValidation
- xUnit *(planned)*

## Architecture

Clean Architecture across 5 projects:

```
BeautySalon.Domain          Entities, value objects, enums — no dependencies
BeautySalon.Application     Use cases (AppServices), DTOs, validators
BeautySalon.Persistence     EF Core/SQLite implementation of Application's contracts
BeautySalon.Infrastructure  Non-MAUI platform services (password hashing, clock, current user)
Beauty Salon (MAUI head)    Pages, ViewModels, Shell navigation — the composition root
```

See `CLAUDE.md` for a detailed breakdown of each layer, key patterns (Result pattern, soft delete/audit, schedule availability), and known gotchas.

## Getting started

**Prerequisites**: .NET 10 SDK with the MAUI workload (`dotnet workload install maui`) and the `dotnet-ef` tool (`dotnet tool install --global dotnet-ef`).

```bash
dotnet build "Beauty Salon.slnx"                       # build everything
dotnet build "Beauty Salon.csproj" -f net10.0-android   # build the app for Android
```

Database migrations and seed data (admin user, starter catalog, payment methods, default working hours) are applied automatically on first launch.

**Seeded login**: `admin` / `admin123`

## Project status

| Phase | Scope | Status |
|---|---|---|
| 1 | Clean Architecture skeleton + domain model | ✅ Done |
| 2 | Persistence (EF Core/SQLite), migrations, seeder | ✅ Done |
| 3 | Application services, DTOs, validators, core ViewModels | ✅ Done |
| 4 | MAUI UI — pages and Shell navigation for every screen | ✅ Done |
| 5 | Reports, local notifications, settings, i18n (es/en) | ⏳ Planned |
| 6 | Testing, optimization, documentation | ⏳ Planned |

## License

All rights reserved.
