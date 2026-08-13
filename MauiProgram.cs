using System.Globalization;
using BeautySalon.Application;
using BeautySalon.Application.Features.Settings;
using BeautySalon.Infrastructure;
using BeautySalon.Persistence;
using Beauty_Salon.Pages;
using Beauty_Salon.Services;
using Beauty_Salon.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Firebase.Crashlytics;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models.AndroidOption;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

namespace Beauty_Salon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Must run before any DbContext touches Sqlite - required on iOS/MacCatalyst
            // where the native provider isn't auto-registered.
            SQLitePCL.Batteries_V2.Init();

            RegisterGlobalCrashReporting();
            RemoveNativeUnderlines();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLocalNotification(config =>
                {
                    config.AddAndroid(android =>
                    {
                        android.AddChannel(new AndroidNotificationChannelRequest
                        {
                            Id = LocalNotificationScheduler.AppointmentReminderChannelId,
                            Name = "Appointment reminders",
                            Description = "Reminders for upcoming appointments",
                            Importance = AndroidImportance.High
                        });
                    });
                })
                .ConfigureLifecycleEvents(events =>
                {
                    // Crashlytics-only setup (no Analytics/Auth/etc.) - the mapping_file_id
                    // string resource in Platforms/Android/Resources/values/strings.xml works
                    // around a known .NET MAUI issue where Crashlytics otherwise crashes on
                    // startup with "The Crashlytics build ID is missing" (Gradle normally
                    // generates that id, and MSBuild doesn't run that step).
#if ANDROID
                    events.AddAndroid(android => android.OnCreate((activity, _) =>
                    {
                        CrossFirebase.Initialize(activity, () => Platform.CurrentActivity!);
                        CrossFirebaseCrashlytics.Current.SetCrashlyticsCollectionEnabled(true);
                    }));
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    // "InterRegular"/"InterSemibold" are the font family names the design
                    // system's styles reference. Real Inter-*.ttf files couldn't be downloaded
                    // in this environment (network egress to the font's release/CDN hosts is
                    // blocked) - OpenSans is aliased here as a visually close substitute. Drop
                    // real Inter TTFs into Resources/Fonts and repoint these two lines to swap
                    // in the real typeface; no other file needs to change.
                    fonts.AddFont("OpenSans-Regular.ttf", "InterRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "InterSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddPersistence($"Data Source={DatabasePaths.FullPath};Default Timeout=30");
            builder.Services.AddInfrastructure();
            builder.Services.AddApplication();

            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
            builder.Services.AddSingleton<IAppointmentNotificationScheduler, LocalNotificationScheduler>();
            builder.Services.AddSingleton<IDataBackupService, DataBackupService>();
            builder.Services.AddSingleton<IPersistedSessionStore, PersistedSessionStore>();

            // Temporary - for a one-off LinkedIn demo recording. Delete this registration plus
            // Services/IDemoDataSeeder.cs/DemoDataSeeder.cs and the Settings button once done.
            builder.Services.AddTransient<IDemoDataSeeder, DemoDataSeeder>();

            // ViewModels are transient - each page navigation gets its own instance.
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<AgendaViewModel>();
            builder.Services.AddTransient<AppointmentFormViewModel>();
            builder.Services.AddTransient<RescheduleViewModel>();
            builder.Services.AddTransient<FinishAppointmentViewModel>();
            builder.Services.AddTransient<ClientListViewModel>();
            builder.Services.AddTransient<ClientFormViewModel>();
            builder.Services.AddTransient<ClientDetailViewModel>();
            builder.Services.AddTransient<CatalogViewModel>();
            builder.Services.AddTransient<CategoryFormViewModel>();
            builder.Services.AddTransient<ServiceFormViewModel>();
            builder.Services.AddTransient<PaymentMethodListViewModel>();
            builder.Services.AddTransient<PaymentMethodFormViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<SalesViewModel>();
            builder.Services.AddTransient<ProductListViewModel>();
            builder.Services.AddTransient<ProductFormViewModel>();
            builder.Services.AddTransient<ProductSaleFormViewModel>();
            builder.Services.AddTransient<ClientDebtsViewModel>();
            builder.Services.AddTransient<DebtChargeFormViewModel>();
            builder.Services.AddTransient<DebtPaymentFormViewModel>();

            // Pages are transient too - Shell/DI resolves a fresh one per navigation.
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AgendaPage>();
            builder.Services.AddTransient<AppointmentFormPage>();
            builder.Services.AddTransient<ReschedulePage>();
            builder.Services.AddTransient<FinishAppointmentPage>();
            builder.Services.AddTransient<ClientListPage>();
            builder.Services.AddTransient<ClientFormPage>();
            builder.Services.AddTransient<ClientDetailPage>();
            builder.Services.AddTransient<CatalogPage>();
            builder.Services.AddTransient<CategoryFormPage>();
            builder.Services.AddTransient<ServiceFormPage>();
            builder.Services.AddTransient<PaymentMethodsPage>();
            builder.Services.AddTransient<PaymentMethodFormPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<SalesPage>();
            builder.Services.AddTransient<ProductListPage>();
            builder.Services.AddTransient<ProductFormPage>();
            builder.Services.AddTransient<ProductSaleFormPage>();
            builder.Services.AddTransient<ClientDebtsPage>();
            builder.Services.AddTransient<DebtChargeFormPage>();
            builder.Services.AddTransient<DebtPaymentFormPage>();
            builder.Services.AddTransient<CrashTestPage>();
            builder.Services.AddTransient<HelpPage>();

            var app = builder.Build();

            // One-time startup cost: applies pending migrations and seeds first-run
            // data before any page can query the database.
            app.Services.InitializeDatabaseAsync().GetAwaiter().GetResult();

            // x:Static resource bindings are resolved once at page construction, so the
            // saved language preference must be applied before any page/DataTemplate is built.
            ApplySavedCulture(app.Services);

            return app;
        }

        // A safety net for real, unexpected bugs - separate from CrashTestPage's deliberately
        // caught test exceptions. Some .NET exceptions (an unobserved Task fault, certain
        // managed/Java boundary crossings) don't always reach Android's native crash handler
        // on their own, so Crashlytics would silently miss them without these global hooks.
        private static void RegisterGlobalCrashReporting()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                if (args.ExceptionObject is Exception exception)
                    ReportToCrashlytics(exception);
            };

            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                ReportToCrashlytics(args.Exception);
                args.SetObserved();
            };

#if ANDROID
            Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, args) =>
            {
                ReportToCrashlytics(args.Exception);
                args.Handled = true;
            };
#endif
        }

        // Every boxed-looking input in this app is a plain Entry/Editor nested inside our own
        // InputContainer Border (see CLAUDE.md's design-system notes) - but Android's native
        // Material text field still draws its own underline via the platform view's Background
        // drawable regardless, giving every field a double-border look. Setting BackgroundColor
        // in XAML doesn't touch that native drawable, so it has to be cleared here instead.
        private static void RemoveNativeUnderlines()
        {
#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
                handler.PlatformView.Background = null;
            });

            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
                handler.PlatformView.Background = null;
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
                handler.PlatformView.Background = null;
            });

            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
                handler.PlatformView.Background = null;
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("RemoveUnderline", (handler, _) =>
            {
                handler.PlatformView.Background = null;
            });
#endif
        }

        private static void ReportToCrashlytics(Exception exception)
        {
            try
            {
                CrossFirebaseCrashlytics.Current.RecordException(exception);
            }
            catch
            {
                // Never let crash reporting itself become the crash.
            }
        }

        private static void ApplySavedCulture(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var settingsAppService = scope.ServiceProvider.GetRequiredService<ISettingsAppService>();
            var result = settingsAppService.GetSettingsAsync().GetAwaiter().GetResult();
            if (!result.IsSuccess)
            {
                return;
            }

            var culture = new CultureInfo(result.Value.Language);
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
        }
    }
}
