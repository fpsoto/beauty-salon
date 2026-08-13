namespace BeautySalon.Application.Features.Debts;

public sealed record ClientBalanceDto(Guid ClientId, string ClientFullName, decimal Balance);
