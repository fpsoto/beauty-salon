# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

Beauty Salon is a .NET MAUI app for a single-user beauty salon administrator (clients, weekly agenda, service catalog, charges, reports). It follows a staged build-out defined by the user:

1. **Fase 1 (done)** — Clean Architecture skeleton + domain model.
2. **Fase 2 (done)** — Persistence (EF Core/SQLite), migrations, audit/soft-delete interceptor, seeder.
3. **Fase 3 (done)** — Application services/use cases, DTOs, validators, and the core ViewModels (Login, Agenda, Client list/detail).
4. **Fase 4 (done)** — MAUI UI: XAML pages for every screen, Shell navigation, remaining CRUD ViewModels (Catalog, Payment Methods) built alongside their pages.
5. **Fase 5 (pending)** — Reports, local notifications, settings, i18n (`.resx`, es/en).
6. **Fase 6 (pending)** — Testing, optimization, docs.

**Fase 4 has never been run on a device/emulator** — no Android emulator or physical device is available in the dev environment this was built in. It's only been verified by `dotnet build` succeeding (which, combined with `MauiXamlInflator=SourceGen`, does catch binding-path/type/converter-key errors at compile time via `x:DataType` compiled bindings) — but visual layout, gesture behavior, and anything only observable at runtime has **not** been checked. Budget time to actually run it on Android before assuming any page works.

All C# identifiers (classes, properties, parameters, locals) are English, even though the domain/spec is Spanish — e.g. `Client.Name`/`LastName`, `Appointment.Date`/`StartTime`/`Status`, `AppointmentStatus.Booked`/`Completed`. Only genuine Spanish-language *content* the app shows the user stays in Spanish: seeded display strings (category/service/payment-method names like "Corte Hombre", "Efectivo"), and validation/business error messages returned in `Error.Message`. Don't reintroduce Spanish property names when extending entities/DTOs.

There is no git repository initialized in this folder; there is no test project yet.

Target platforms: **Android only today; iOS is kept for a future release** (buildable only from a macOS host — the condition in the `.csproj` drops it on Linux). Windows and MacCatalyst were deliberately removed — do not re-add them without being asked.

## Commands

Requires the .NET 10 SDK with the MAUI workload (`dotnet workload install maui`) and `dotnet-ef` (`dotnet tool install --global dotnet-ef`).

```
dotnet build "Beauty Salon.slnx"                       # build everything (all class libs + MAUI head)
dotnet build "Beauty Salon.csproj" -f net10.0-android   # build the app for Android
dotnet build "Beauty Salon.csproj" -f net10.0-ios       # build for iOS (macOS host only)

# EF Core migrations (run from repo root; the design-time factory needs no MAUI host):
dotnet ef migrations add <Name> --project "BeautySalon.Persistence/BeautySalon.Persistence.csproj" --output-dir Migrations
dotnet ef database update --project "BeautySalon.Persistence/BeautySalon.Persistence.csproj"
```

No test project exists yet (Fase 6).

**OneDrive caveat**: this repo lives under a synced OneDrive folder. Android/iOS builds occasionally fail with `Access to the path '...' is denied` while MSBuild tries to clear an `obj/Debug/<tfm>/...` intermediate directory — that's OneDrive transiently locking a file, not a real build error. Fix: delete the specific `obj/Debug/<tfm>` folder and rebuild.

Seeded login: **admin / admin123** (single admin user; the schema is already multi-user-ready via `ProfessionalId` FKs, but there's no login UI yet).

## Architecture

Clean Architecture across 5 projects (flat layout, MAUI head unmoved at repo root):

```
BeautySalon.Domain/          <- no NuGet packages, no project references. Pure C#.
BeautySalon.Application/     <- Domain only. Contracts (interfaces), no implementations yet.
BeautySalon.Persistence/     <- Application + Domain. EF Core/SQLite implementation.
BeautySalon.Infrastructure/  <- Application + Domain. Non-MAUI platform services (hashing, clock, current user).
Beauty Salon.csproj          <- Application + Infrastructure + Persistence. MAUI head / composition root.
```

`Beauty Salon.csproj` has an explicit `<Compile Remove>` for the 4 sibling project folders — without it, the MAUI project's default recursive glob would also compile their `.cs` files directly, duplicating every type. Keep that in mind if new sibling class libraries are added.

**Domain** (`BeautySalon.Domain`): `Common/BaseEntity` (Guid v7 PK) → `AuditableEntity` (Created/Updated/Deleted At+By, `IsDeleted`, `Version` as a portable int concurrency token) is the base for every entity, without exception — even simple catalogs like `PaymentMethod`. `Common/Result`+`Error` is the Result-pattern used for *expected* business failures (schedule conflicts, duplicate RUT); real exceptions are reserved for genuinely unexpected IO/DB failures. `Common/IScheduleOccupant` (`Date`/`StartTime`/`EndTime`/`ProfessionalId`) is implemented by both `Appointment` and `ScheduleBlock` so overlap/availability logic can treat them as one merged list instead of duplicating the check per type. `ValueObjects/Rut` validates the Chilean check digit (the one identifier kept as-is — it's a proper-noun acronym, not a translatable word). Entities: `User`, `Client`, `ServiceCategory`, `SalonService` (named to avoid colliding with `...AppService`), `Appointment` (`Date`/`StartTime`/`EndTime`/`Status`/`SuggestedPrice`/`ChargedPrice`/`Discount`/`Tip`/`InternalNotes`), `AppointmentServiceItem` (bridge table with a *snapshot* of price/name/duration at booking time — catalog changes must never alter historical appointments), `ScheduleBlock` (`Type`/`Reason`), `WorkingHours`, `PaymentMethod`, `AppSettings`+`NotificationRule`, `Product` (minimal, no inventory yet). `AppointmentStatus` = `Booked`/`Confirmed`/`InProgress`/`Completed`/`Cancelled`/`NoShow`/`Rescheduled`; `ScheduleBlockType` = `Lunch`/`Vacation`/`Meeting`/`DayOff`/`Unavailable`/`Other`.

**Application** (`BeautySalon.Application`): `Common/Interfaces` holds every port this app depends on — `IRepository<T>` (generic CRUD) + one specific repository interface per aggregate, `IUnitOfWork` (aggregates all repos + `SaveChangesAsync`), `IDateTimeProvider`, `ICurrentUserContext`, `IPasswordHasher`, `IWorkingHoursProvider`, `IScheduleAvailabilityChecker`. `Common/Exceptions` (`ConcurrencyConflictException`, `DataAccessException`) are what Persistence throws instead of ever letting an EF Core/Sqlite exception type cross into Application. `Common/WellKnownIds.AdminUserId` is the seeded admin's fixed id, shared between the seeder and `ICurrentUserContext`'s stub implementation.

`Features/<Auth|Clients|Catalog|Payments|Schedule>` hold the Fase 3 AppServices, one per aggregate, each with: DTOs, `Create`/`Update` request records, a `FluentValidation` validator per request (validates shape/format only — see below), a `*MappingExtensions` static class (`ToDto()`), and an `I*AppService`/`*AppService` pair. Every AppService method returns `Result`/`Result<TDto>`, injects `IUnitOfWork` only, and follows the same shape: validate → load/check business rules → mutate → `SaveChangesAsync` → map to DTO. `Features/Schedule` additionally holds `WorkingHoursProvider`/`ScheduleAvailabilityChecker` (the concrete implementations of the Fase 1 contracts) and `WeekHelper` (Monday–Sunday week bounds). Business-rule checks that need data access (schedule overlap, duplicate RUT, "category still has services") live in the AppService or `ScheduleAvailabilityChecker`, never in the FluentValidation validators. `ApplicationServiceCollectionExtensions.AddApplication()` registers everything, including `AddValidatorsFromAssembly`.

Note on `AppointmentAppService`: `RescheduleAsync` does **not** edit the appointment in place — it creates a new `Appointment` (status `Booked`) linked via `RescheduledFromAppointmentId`, and flips the original to `Rescheduled`. `CancelAsync`/`ConfirmAsync`/`StartAsync`/`MarkNoShowAsync` all funnel through a private `TransitionAsync` helper that checks the allowed source statuses before mutating — extend that helper (with its optional `mutate` callback) rather than hand-rolling a new transition method.

**Persistence** (`BeautySalon.Persistence`): `BeautySalonDbContext` applies every `IEntityTypeConfiguration<T>` from `Configurations/` via assembly scanning, then in `OnModelCreating` applies the `Version` concurrency token *and* the `!IsDeleted` query filter to every `AuditableEntity` type generically via reflection — don't add per-entity soft-delete filters, extend that loop instead. `Interceptors/AuditableEntitySaveChangesInterceptor` stamps Created/Updated/Deleted(At+By) and converts a `Remove` into a soft-delete (`IsDeleted=true` + `EntityState.Modified`) — repositories call `DbSet.Remove(...)` normally and never need to know this. `Repositories/EfRepository<T>` is the generic base; specific repos add only real query methods (search, date ranges, overlap checks). **`ClientRepository.SearchAsync` filters in-memory, not via SQL** — `Client.Rut` is a `HasConversion` value object, and translating a `Like`/equality check against it via `EF.Property<string>` throws `InvalidCastException` at runtime (a real bug hit and fixed during Fase 3); loading all clients and filtering client-side is deliberate and fine at this app's scale, don't try to push it back into SQL without re-verifying that translation actually works. `UnitOfWork.SaveChangesAsync` is the *only* place that translates `DbUpdateConcurrencyException`/`DbUpdateException` into the Application-level exceptions — this is deliberate, don't add try/catch-and-rethrow elsewhere. `BeautySalonDbContextFactory` (`IDesignTimeDbContextFactory`) lets `dotnet ef` run without the MAUI host (pass `--startup-project` pointing at Persistence itself, or `dotnet-ef` may pick the MAUI head and fail on its multi-targeting). `Seed/DatabaseSeeder` creates the admin user (fixed id `WellKnownIds.AdminUserId`), a starter catalog, payment methods, Mon–Fri working hours, and `AppSettings`+`NotificationRule`s — `PersistenceServiceCollectionExtensions.InitializeDatabaseAsync` runs `PRAGMA journal_mode=WAL`, `Database.MigrateAsync`, then the seeder; called once from `MauiProgram.CreateMauiApp()`.

**Infrastructure** (`BeautySalon.Infrastructure`): `Services/DateTimeProvider`, `BCryptPasswordHasher`. `CurrentUserContext` is a stub that always resolves to the seeded admin (`WellKnownIds.AdminUserId`) — replace with a real session once Fase 4's login exists; nothing else should need to change since it's behind `ICurrentUserContext`. WhatsApp deep links and local notifications are *not* here — they depend on MAUI Essentials APIs, so they'll live directly in the MAUI head behind an `Application`-defined interface when Fase 5 gets to them.

**Presentation / composition root** (`Beauty Salon.csproj`, `MauiProgram.cs`, `ViewModels/`): `MauiProgram.cs` calls `SQLitePCL.Batteries_V2.Init()` first, resolves the SQLite path via `FileSystem.AppDataDirectory`, wires `AddPersistence(connectionString)` + `AddInfrastructure()` + `AddApplication()`, registers the Fase 3 ViewModels as transient, then runs `InitializeDatabaseAsync()` synchronously (`GetAwaiter().GetResult()`) before returning the built `MauiApp` — migrations/seed must be done before any page can query the database. `RootNamespace` stays `Beauty_Salon` (underscore) even though new projects use `BeautySalon.*` (no underscore) — don't "fix" this, it'd break existing XAML `x:Class` references; the `ViewModels/` folder namespace follows the same `Beauty_Salon.ViewModels` convention.

`ViewModels/ViewModelBase` (`ObservableObject`) exposes `IsBusy`/`ErrorMessage` and a `SafeExecuteAsync(Func<Task>)` wrapper that catches unexpected exceptions, logs via injected `ILogger`, and sets `ErrorMessage` — expected business failures should already arrive as a failed `Result` (use `SetError(result.Error)`), not an exception. **`SafeExecuteAsync` is not reentrant**: never call another `SafeExecuteAsync`-wrapped method from inside one (it no-ops because `IsBusy` is already true) — every ViewModel that needs to refresh itself after a mutation splits a private `...CoreAsync` method out for that (see `AgendaViewModel.LoadWeekCoreAsync`, `ClientListViewModel.SearchCoreAsync`, `CatalogViewModel.LoadCoreAsync`) and calls the core method internally instead. `AgendaViewModel` and `AppointmentFormViewModel` hardcode `_professionalId = WellKnownIds.AdminUserId` with a `TODO(Fase 4)`/`TODO(Fase 4)` until real login/session exists.

### Fase 4 — UI layer (`Pages/`, `Services/`, `Converters/`, `AppShell.xaml`, `App.xaml.cs`)

**Login → Shell swap**: the app does *not* start on `AppShell`. `App.xaml.cs` resolves `LoginPage` (DI) as the window's initial page (no Shell chrome). On successful login, `LoginPage.xaml.cs` resolves a fresh `AppShell` via `IServiceProvider` and replaces it: `Application.Current!.Windows[0].Page = shell;`. There is no session/logout flow yet — that's for whenever real multi-user auth arrives.

**`AppShell.xaml`** is a `TabBar` with 4 tabs (`agenda`/`clients`/`catalog`/`paymentmethods`), `FlyoutBehavior="Disabled"`. Agenda is the first tab = the default/home screen, per the brief ("weekly agenda, not a dashboard"). Non-tab pages (forms, detail, modals) are registered as plain routes in `AppShell.xaml.cs`'s constructor via `Routing.RegisterRoute("route/name", typeof(SomePage))` — add new ones there, not as `ShellContent`.

**DI-resolved page navigation**: every `Page` and `ViewModel` is registered `AddTransient` in `MauiProgram.cs`. Shell/`{DataTemplate pages:SomePage}`/`Routing.RegisterRoute` all resolve page instances through the app's `IServiceProvider`, so a page's constructor can take its ViewModel (and anything else) as a normal DI dependency — no manual `new SomePage(new SomeViewModel(...))` anywhere. When adding a new page: register both the page and its ViewModel in `MauiProgram.cs`, or navigation will throw at resolve time.

**Passing data on navigation**: pages that need an id/object use `[QueryProperty(nameof(X), "X")]` + a public settable property `X` that forwards into the ViewModel (e.g. `ReschedulePage.AppointmentId`, `CategoryFormPage.Category`). The Shell `GoToAsync(route, new Dictionary<string, object> { ["X"] = value })` overload passes the object by reference in-process — you can pass whole DTOs (not just primitive ids), which `CategoryFormPage`/`ServiceFormPage`/`PaymentMethodFormPage` rely on to prefill their edit forms instead of re-fetching.

**Appointment actions live in the agenda, not a detail page**: tapping an appointment in `AgendaPage` opens a `DisplayActionSheetAsync` whose options are computed from the appointment's current `AppointmentStatus` (`AgendaPage.BuildAvailableActions`) — Confirm/Start/Cancel/NoShow call the ViewModel directly; Finish/Reschedule navigate to their dedicated pages (they need more input than a confirm dialog). Extend `BuildAvailableActions` rather than adding a separate "appointment detail" page.

**Known XAML/MVVM gotcha already hit once**: `IsVisible="{Binding SomeObservableCollection, Converter=...}"` never refreshes after the initial bind — the collection *reference* never changes, so no `PropertyChanged` fires when you `Add`/`Clear` it. Bind to `SomeObservableCollection.Count` instead (`ObservableCollection<T>` does raise `PropertyChanged("Count")` on mutation) with `Converters/CountToBoolConverter`. `ItemsSource` bindings on `CollectionView` don't have this problem (`CollectionView` subscribes to `CollectionChanged` directly) — this only bites plain scalar bindings like `IsVisible`/`Text`.

**Display-only helpers stay in `ViewModels/`, not `Domain`**: `AppointmentStatusDisplay`/`ScheduleBlockTypeDisplay` map the English enums to Spanish labels (and `AppointmentStatusDisplay` also maps to an accent `Color`) purely for UI — the enums themselves stay English per the project-wide rule. `AgendaEntry`/`DayAgendaGroup`/`CategoryServiceGroup` are Presentation-only wrapper types (not Application DTOs) that exist solely to give `CollectionView` a uniform/grouped shape to bind against.

**Converters** (`Converters/`, registered as keyed resources in `App.xaml`): `FavoriteColorConverter` (bool→gold/gray star), `HexToColorConverter` (`"#RRGGBB"` string→`Color`, used for category color swatches), `CountToBoolConverter` (see gotcha above). Plus CommunityToolkit.Maui's `InvertedBoolConverter`, `IsStringNotNullOrEmptyConverter`, `IsListNotNullOrEmptyConverter`, `IsNotNullConverter`.

**Deliberately deferred, not forgotten**: `IAppointmentAppService` has no `GetByIdAsync` — `FinishAppointmentPage` doesn't prefill the suggested price because of this; if that's annoying in practice, add the method rather than fetching the whole week to find one appointment. `Client.DateOfBirth` is optional in the domain but `DatePicker.Date` isn't nullable — `ClientFormViewModel` uses a `HasDateOfBirth` bool + a real `DateTime` field instead of a nullable date, gated by a checkbox in the form.
