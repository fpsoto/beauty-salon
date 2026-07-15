using BeautySalon.Application.Common.Interfaces;
using BeautySalon.Domain.Common;
using BeautySalon.Domain.Entities;
using FluentValidation;

namespace BeautySalon.Application.Features.Settings;

public sealed class SettingsAppService : ISettingsAppService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<UpdateAppSettingsRequest> _updateSettingsValidator;
    private readonly IValidator<UpdateWorkingHoursRequest> _updateWorkingHoursValidator;

    public SettingsAppService(
        IUnitOfWork unitOfWork,
        IValidator<UpdateAppSettingsRequest> updateSettingsValidator,
        IValidator<UpdateWorkingHoursRequest> updateWorkingHoursValidator)
    {
        _unitOfWork = unitOfWork;
        _updateSettingsValidator = updateSettingsValidator;
        _updateWorkingHoursValidator = updateWorkingHoursValidator;
    }

    public async Task<Result<AppSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.AppSettings.GetAsync(cancellationToken);
        return Result.Success(settings.ToDto());
    }

    public async Task<Result<AppSettingsDto>> UpdateSettingsAsync(UpdateAppSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateSettingsValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<AppSettingsDto>(Error.Validation("Settings.Invalid", validation.ToString(" ")));

        var settings = await _unitOfWork.AppSettings.GetAsync(cancellationToken);
        settings.Language = request.Language;
        settings.Theme = request.Theme;
        settings.Currency = request.Currency;
        settings.DateFormat = request.DateFormat;
        settings.TimeFormat = request.TimeFormat;

        _unitOfWork.AppSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(settings.ToDto());
    }

    public async Task<Result> UpdateNotificationRulesAsync(IReadOnlyList<NotificationRuleInput> rules, CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.AppSettings.GetAsync(cancellationToken);

        foreach (var input in rules)
        {
            var existing = settings.NotificationRules.FirstOrDefault(r => r.MinutesBefore == input.MinutesBefore);
            if (existing is not null)
            {
                existing.IsEnabled = input.IsEnabled;
            }
            else
            {
                settings.NotificationRules.Add(new NotificationRule
                {
                    AppSettingsId = settings.Id,
                    MinutesBefore = input.MinutesBefore,
                    IsEnabled = input.IsEnabled
                });
            }
        }

        _unitOfWork.AppSettings.Update(settings);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<WorkingHoursDto>>> GetWorkingHoursAsync(Guid professionalId, CancellationToken cancellationToken = default)
    {
        var hours = await _unitOfWork.WorkingHours.GetByProfessionalAsync(professionalId, cancellationToken);
        return Result.Success<IReadOnlyList<WorkingHoursDto>>(hours.OrderBy(h => h.DayOfWeek).Select(h => h.ToDto()).ToList());
    }

    public async Task<Result> UpdateWorkingHoursAsync(Guid professionalId, UpdateWorkingHoursRequest request, CancellationToken cancellationToken = default)
    {
        var validation = await _updateWorkingHoursValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure(Error.Validation("WorkingHours.Invalid", validation.ToString(" ")));

        var existing = await _unitOfWork.WorkingHours.GetByProfessionalAsync(professionalId, cancellationToken);

        foreach (var input in request.Days)
        {
            var match = existing.FirstOrDefault(w => w.DayOfWeek == input.DayOfWeek);
            if (match is not null)
            {
                match.StartTime = input.StartTime;
                match.EndTime = input.EndTime;
                match.IsWorkingDay = input.IsWorkingDay;
                _unitOfWork.WorkingHours.Update(match);
            }
            else
            {
                _unitOfWork.WorkingHours.Add(new WorkingHours
                {
                    ProfessionalId = professionalId,
                    DayOfWeek = input.DayOfWeek,
                    StartTime = input.StartTime,
                    EndTime = input.EndTime,
                    IsWorkingDay = input.IsWorkingDay
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
