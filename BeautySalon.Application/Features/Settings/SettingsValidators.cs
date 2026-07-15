using FluentValidation;

namespace BeautySalon.Application.Features.Settings;

public sealed class UpdateAppSettingsRequestValidator : AbstractValidator<UpdateAppSettingsRequest>
{
    public UpdateAppSettingsRequestValidator()
    {
        RuleFor(x => x.Language).NotEmpty().MaximumLength(5);
        RuleFor(x => x.DateFormat).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TimeFormat).NotEmpty().MaximumLength(20);
    }
}

public sealed class UpdateWorkingHoursRequestValidator : AbstractValidator<UpdateWorkingHoursRequest>
{
    public UpdateWorkingHoursRequestValidator()
    {
        RuleFor(x => x.Days).NotEmpty();
        RuleForEach(x => x.Days).ChildRules(day =>
        {
            day.RuleFor(d => d.EndTime)
                .GreaterThan(d => d.StartTime)
                .When(d => d.IsWorkingDay)
                .WithMessage("La hora de término debe ser posterior a la hora de inicio.");
        });
    }
}
