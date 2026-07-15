using BeautySalon.Domain.ValueObjects;
using FluentValidation;

namespace BeautySalon.Application.Features.Clients;

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Rut)
            .NotEmpty()
            .Must(rut => Rut.TryCreate(rut, out _, out _))
            .WithMessage("El RUT no tiene un formato válido.");
    }
}

public sealed class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Rut)
            .NotEmpty()
            .Must(rut => Rut.TryCreate(rut, out _, out _))
            .WithMessage("El RUT no tiene un formato válido.");
    }
}
