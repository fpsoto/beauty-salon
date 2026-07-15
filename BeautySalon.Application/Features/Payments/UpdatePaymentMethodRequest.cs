namespace BeautySalon.Application.Features.Payments;

public sealed record UpdatePaymentMethodRequest(string Name, int SortOrder, bool IsActive);
