namespace BeautySalon.Application.Features.Reports;

public sealed record TopClientDto(Guid ClientId, string ClientFullName, decimal TotalSpent, int VisitCount);
