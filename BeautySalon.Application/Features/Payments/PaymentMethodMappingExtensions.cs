using BeautySalon.Domain.Entities;

namespace BeautySalon.Application.Features.Payments;

public static class PaymentMethodMappingExtensions
{
    public static PaymentMethodDto ToDto(this PaymentMethod paymentMethod) =>
        new(paymentMethod.Id, paymentMethod.Name, paymentMethod.IsActive, paymentMethod.SortOrder);
}
