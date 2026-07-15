namespace BeautySalon.Application.Features.Payments;

public sealed record CreatePaymentMethodRequest(string Name, int SortOrder);
