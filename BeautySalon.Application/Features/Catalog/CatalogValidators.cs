using FluentValidation;

namespace BeautySalon.Application.Features.Catalog;

// #RRGGBB or #RRGGBBAA
internal static class ColorHexRule
{
    public const string Pattern = "^#[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})?$";
}

public sealed class CreateServiceCategoryRequestValidator : AbstractValidator<CreateServiceCategoryRequest>
{
    public CreateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ColorHex).NotEmpty().Matches(ColorHexRule.Pattern).WithMessage("El color debe tener formato #RRGGBB.");
    }
}

public sealed class UpdateServiceCategoryRequestValidator : AbstractValidator<UpdateServiceCategoryRequest>
{
    public UpdateServiceCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ColorHex).NotEmpty().Matches(ColorHexRule.Pattern).WithMessage("El color debe tener formato #RRGGBB.");
    }
}

public sealed class CreateSalonServiceRequestValidator : AbstractValidator<CreateSalonServiceRequest>
{
    public CreateSalonServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty);
        RuleFor(x => x.SuggestedPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.ColorHex).Matches(ColorHexRule.Pattern).When(x => !string.IsNullOrWhiteSpace(x.ColorHex));
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public sealed class UpdateSalonServiceRequestValidator : AbstractValidator<UpdateSalonServiceRequest>
{
    public UpdateSalonServiceRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CategoryId).NotEqual(Guid.Empty);
        RuleFor(x => x.SuggestedPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.ColorHex).Matches(ColorHexRule.Pattern).When(x => !string.IsNullOrWhiteSpace(x.ColorHex));
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
