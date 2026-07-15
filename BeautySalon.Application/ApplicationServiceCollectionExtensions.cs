using System.Reflection;
using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Application.Features.Auth;
using BeautySalon.Application.Features.Catalog;
using BeautySalon.Application.Features.Clients;
using BeautySalon.Application.Features.Payments;
using BeautySalon.Application.Features.Reports;
using BeautySalon.Application.Features.Schedule;
using BeautySalon.Application.Features.Settings;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BeautySalon.Application;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IWorkingHoursProvider, WorkingHoursProvider>();
        services.AddScoped<IScheduleAvailabilityChecker, ScheduleAvailabilityChecker>();

        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IClientAppService, ClientAppService>();
        services.AddScoped<ICatalogAppService, CatalogAppService>();
        services.AddScoped<IPaymentMethodAppService, PaymentMethodAppService>();
        services.AddScoped<IAppointmentAppService, AppointmentAppService>();
        services.AddScoped<IScheduleBlockAppService, ScheduleBlockAppService>();
        services.AddScoped<ISettingsAppService, SettingsAppService>();
        services.AddScoped<IReportAppService, ReportAppService>();

        return services;
    }
}
