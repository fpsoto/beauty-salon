namespace BeautySalon.Application.Features.Reports;

public sealed record ReportSummaryDto(
    DateOnly From,
    DateOnly To,
    decimal TotalRevenue,
    int CompletedAppointmentsCount,
    decimal AverageRevenuePerAppointment,
    int NewClientsCount,
    int InactiveClientsCount,
    int CancelledCount,
    int NoShowCount,
    IReadOnlyList<RevenueByPaymentMethodDto> RevenueByPaymentMethod,
    IReadOnlyList<TopClientDto> TopClients,
    IReadOnlyList<TopServiceDto> TopServices,
    IReadOnlyList<TopCategoryDto> TopCategories,
    IReadOnlyList<HourCountDto> BusiestHours);
