namespace BeautySalon.Application.Features.Reports;

public sealed record TopProductDto(Guid ProductId, string ProductName, int QuantitySold, decimal Revenue);
