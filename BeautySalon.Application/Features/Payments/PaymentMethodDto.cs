namespace BeautySalon.Application.Features.Payments;

public sealed record PaymentMethodDto(Guid Id, string Name, bool IsActive, int SortOrder);
