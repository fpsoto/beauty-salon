using FluentValidation;

namespace BeautySalon.Application.Features.Schedule;

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEqual(Guid.Empty);
        RuleFor(x => x.ProfessionalId).NotEqual(Guid.Empty);
        RuleFor(x => x.ServiceIds).NotEmpty().WithMessage("La cita debe incluir al menos un servicio.");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{
    public RescheduleAppointmentRequestValidator()
    {
        RuleFor(x => x.AppointmentId).NotEqual(Guid.Empty);
    }
}

public sealed class FinishAppointmentRequestValidator : AbstractValidator<FinishAppointmentRequest>
{
    public FinishAppointmentRequestValidator()
    {
        RuleFor(x => x.AppointmentId).NotEqual(Guid.Empty);
        RuleFor(x => x.PaymentMethodId).NotEqual(Guid.Empty);
        RuleFor(x => x.ChargedPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).When(x => x.Discount.HasValue);
        RuleFor(x => x.Tip).GreaterThanOrEqualTo(0).When(x => x.Tip.HasValue);
    }
}

public sealed class CreateScheduleBlockRequestValidator : AbstractValidator<CreateScheduleBlockRequest>
{
    public CreateScheduleBlockRequestValidator()
    {
        RuleFor(x => x.ProfessionalId).NotEqual(Guid.Empty);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("La hora de término debe ser posterior a la hora de inicio.");
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
