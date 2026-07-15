using BeautySalon.Application;
using BeautySalon.Infrastructure;
using BeautySalon.Persistence;
using Beauty_Salon.Pages;
using Beauty_Salon.Services;
using Beauty_Salon.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace Beauty_Salon
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Must run before any DbContext touches Sqlite - required on iOS/MacCatalyst
            // where the native provider isn't auto-registered.
            SQLitePCL.Batteries_V2.Init();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "beautysalon.db3");
            builder.Services.AddPersistence($"Data Source={databasePath};Default Timeout=30");
            builder.Services.AddInfrastructure();
            builder.Services.AddApplication();

            builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

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

            var app = builder.Build();

            // One-time startup cost: applies pending migrations and seeds first-run
            // data before any page can query the database.
            app.Services.InitializeDatabaseAsync().GetAwaiter().GetResult();

            return app;
        }
    }
}
