namespace BeautySalon.Application.Features.Reports;

public sealed record TopServiceDto(Guid ServiceId, string ServiceName, int Count, decimal Revenue);
