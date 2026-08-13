using FluentValidation;

namespace BeautySalon.Application.Features.Sales;

public sealed class CreateProductSaleRequestValidator : AbstractValidator<CreateProductSaleRequest>
{
    public CreateProductSaleRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.PaymentMethodId).NotEmpty();
        RuleFor(x => x.ProfessionalId).NotEmpty();
    }
}

public sealed class UpdateProductSaleRequestValidator : AbstractValidator<UpdateProductSaleRequest>
{
    public UpdateProductSaleRequestValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
        RuleFor(x => x.PaymentMethodId).NotEmpty();
    }
}
